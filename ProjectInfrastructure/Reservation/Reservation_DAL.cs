using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.Reservation;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;
using FrontOfficeApplication.ProjectServiceLayer;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Timers;

namespace FrontOfficeApplication.ProjectInfrastructure.Reservation
{
    public class Reservation_DAL(IDbConnectionFactory connectionFactory, CommonMethod commonMethod) : IReservation_Repository
    {
        private readonly IDbConnectionFactory _factory = connectionFactory;
        private readonly CommonMethod _commonMethod = commonMethod;
        private static readonly JsonSerializerOptions _camelCaseJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        //public Reservation_DAL(IDbConnectionFactory connectionFactory, CommonMethod commonMethod)
        //{
        //    _factory = connectionFactory;
        //    _commonMethod = commonMethod;
        //}

        #region ReservationTransaction

        public async Task<IEnumerable<RoomAllocated>> GetRoomAllocated(string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @" SELECT RoomCode, Allocated FROM Tbl_Room_Master 
                Where IsRoomActive = 1 And PropertyCode = @propertycode And BranchCode = @branchcode ORDER BY RoomCode";

            var roomAllocatedList = await connection.QueryAsync<RoomAllocated>(selectQuery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return roomAllocatedList;
        }

        public async Task<int> GetOccupiedRooms( string RoomCode, DateTime CheckInDate, DateTime CheckOutDate, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectquery = @" SELECT ISNULL(SUM(NoofRooms), 0) FROM Tbl_ForEver_Data WHERE RoomCode = @roomcode AND RoomStatus = 'B'
                AND ArrivalDate >= @checkindate AND DepatureDate <= @checkoutdate And PropertyCode = @propertycode And BranchCode = @branchcode";

            var ReservationPayModelist = await connection.ExecuteScalarAsync<int>(selectquery, new { roomcode = RoomCode, checkindate = CheckInDate.Date, checkoutdate = CheckOutDate.Date, propertycode = PropertyCode, branchcode = BranchCode });
            return ReservationPayModelist;
        }

        public async Task<IEnumerable<TaxDetailsModel>> GetTaxDetails(string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectquery = @" Select TM.TaxCode, TM.TaxName, TDM.TaxDescriptionCode, TDM.TaxDescriptionName, TDM.TaxPercentage, 
                TDM.FromAmount, TDM.ToAmount, TDM.FromDate, TDM.ToDate, TDM.PropertyCode, TDM.BranchCode
                From Tbl_Tax_Master TM
                Inner Join Tbl_TaxDescription_Master TDM on TDM.TaxCode = TM.TaxCode And TM.PropertyCode = TDM.PropertyCode And TM.BranchCode = TDM.BranchCode
                Where TM.IsTaxActive = 1 And TDM.IsTaxDescriptionActive = 1 And TDM.PropertyCode = @propertycode And TDM.BranchCode = @branchcode 
                ORDER BY TM.TaxCode; ";

            var result = await connection.QueryAsync<TaxDetailsModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });

            return result;
        }

        public async Task<TaxRulesDetailsModel> GetTaxRulesDetails(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select TaxRuleId, TaxRuleCode, Traiff, TraiffOperation, Type1, Operation1, Type2, Operation2, Type3, 
                IsTaxRuleActive, IsExtraBedTax, PropertyCode, BranchCode
                From Tbl_TaxRule_Master
                Where IsTaxRuleActive = 1 And PropertyCode = @propertyCode And BranchCode = @branchCode Order By TaxRuleId; ";

            var TaxRulelist = await connection.QueryFirstOrDefaultAsync<TaxRulesDetailsModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return TaxRulelist!;
        }

        public async Task<IEnumerable<PlanDetailsModel>> GetPlanDetails(string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectquery = @"SELECT PlanCode, PlanName, FoodRate, TaxCode, PropertyCode, BranchCode FROM Tbl_Plan_Master
                Where IsPlanActive = 1 And PropertyCode = @propertycode And BranchCode = @branchcode
                ORDER BY PlanName;";

            var result = await connection.QueryAsync<PlanDetailsModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });

            return result;
        }

        public async Task<IEnumerable<RoomTraiffDetailModel>> GetRoomTraiffDetails(string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectquery = @" SELECT RoomCode, RoomName, Allocated, RoomPlanType, DefaultTraiff, PAX, PAXRates, 
                ExtraAdult, ExtraChild, PropertyCode, BranchCode
                FROM Tbl_Room_Master 
                Where IsRoomActive = 1 And PropertyCode = @propertycode And BranchCode = branchcode ORDER BY RoomCode;";

            var result = await connection.QueryAsync<RoomTraiffDetailModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });

            return result;
        }

        public async Task<IEnumerable<CompanyTraiffDetailModel>> GetCompanyTraiffDetails(string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectquery = @" SELECT CompanyCode, RoomCode, TraiffCategory, RoomPlanType, DefaultTraiff, PAX, PAXRates, 
                ExtraAdult, ExtraChild, PropertyCode, BranchCode
                FROM Tbl_Company_Traiff 
                Where PropertyCode = @propertycode And BranchCode = branchcode ORDER BY RoomCode;";

            var result = await connection.QueryAsync<CompanyTraiffDetailModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });

            return result;
        }

        public async Task<ReservationSaveResult> SaveUpdate_GuestProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            //// 1. Handle Update if rvguestcode is provided and valid
            //if (!string.IsNullOrEmpty(rvguestcode) && rvguestcode != "0")
            //{
            //    var updateSql = @"UPDATE Tbl_Guest_Master 
            //                  SET GuestName = @GuestName, Mobile = @Mobile, Email = @Email, 
            //                      GuestAddress = @GuestAddress, City = @City, State = @State 
            //                  WHERE GuestCode = @GuestCode";

            //    await connection.ExecuteAsync(updateSql, new
            //    {
            //        dto.GuestName,
            //        dto.Mobile,
            //        dto.Email,
            //        dto.GuestAddress,
            //        dto.City,
            //        dto.State,
            //        GuestCode = rvguestcode
            //    });
            //    return Ok(new { success = true, message = "Guest updated successfully via rvguestcode." });
            //}

            //// 2. Handle Update if existing GuestCode length is greater than 8
            //if (!string.IsNullOrEmpty(dto.GuestCode) && dto.GuestCode.Length > 8)
            //{
            //    var updateSql = @"UPDATE Tbl_Guest_Master 
            //                  SET GuestName = @GuestName, Mobile = @Mobile, Email = @Email, 
            //                      GuestAddress = @GuestAddress, City = @City, State = @State 
            //                  WHERE GuestCode = @GuestCode";

            //    await connection.ExecuteAsync(updateSql, new
            //    {
            //        dto.GuestName,
            //        dto.Mobile,
            //        dto.Email,
            //        dto.GuestAddress,
            //        dto.City,
            //        dto.State,
            //        dto.GuestCode
            //    });
            //    return Ok(new { success = true, message = "Guest updated successfully via GuestCode." });
            //}

            // 3. Handle Insert Logic
            // Get the next row ID (mimicking GetNextTblRecord)
            //long nextRowId = await connection.ExecuteScalarAsync<long>(
            //    "SELECT ISNULL(MAX(RNo), 0) + 1 FROM Tbl_Guest_Master"
            //);

            //string originalGuestCode = guestDetail.GuestCode;
            //if (string.IsNullOrEmpty(originalGuestCode) || originalGuestCode.Length < 8)
            //{
            //    originalGuestCode = $"{finCode}G{nextRowId:D9}";
            //}

            ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;

            // Map parameters for Stored Procedure [sp_SaveUpdate_Guest_Master]
            var parameters = new DynamicParameters();

            // Pass GuestId (set to null/0 for Insert, or provide an existing ID for Update)
            //parameters.Add("@GuestId", guestDetail.GuestId > 0 ? guestDetail.GuestId : (int?)null, DbType.Int32);
            int? guestIdParam = (guestDetail != null && guestDetail.GuestId > 0) ? guestDetail.GuestId : (int?)null;
            parameters.Add("@GuestId", guestIdParam, DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            // Identification & Basic Info
            parameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
            parameters.Add("@GuestTitle", guestDetail.GuestTitle, DbType.String);
            parameters.Add("@GuestName", guestDetail.GuestName, DbType.String);
            parameters.Add("@GuestGender", guestDetail.GuestGender, DbType.String);
            parameters.Add("@GuestDOB", null, DbType.DateTime);
            parameters.Add("@GuestDOA", null, DbType.DateTime);

            // Address & Contact Info
            parameters.Add("@GuestAddress", guestDetail.GuestAddress, DbType.String);
            parameters.Add("@GuestCity", guestDetail.GuestCity, DbType.String);
            parameters.Add("@GuestState", guestDetail.GuestState, DbType.String);
            parameters.Add("@GuestNation", guestDetail.GuestNation, DbType.String);
            parameters.Add("@GuestPinCode", guestDetail.GuestPinCode, DbType.String);
            parameters.Add("@GuestMobileNo", guestDetail.GuestMobileNo, DbType.String);
            parameters.Add("@GuestEmail", guestDetail.GuestEmail, DbType.String);

            // Identity Proof Details (Adjusted defaults based on your old snippet or DTO)
            parameters.Add("@IdentityproofCode", null, DbType.String);
            parameters.Add("@IdentityproofName", null, DbType.String);

            // Vehicle Details (Set to null if not in your current DTO)
            parameters.Add("@GuestVehicleType", null, DbType.String);
            parameters.Add("@GuestVehicleNo", null, DbType.String);

            // Status & Audit Fields
            parameters.Add("@IsGuestActive", true, DbType.Boolean);
            parameters.Add("@UserCode", ReservationDetail.UserCode, DbType.String);
            parameters.Add("@IPAddress", ReservationDetail.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", ReservationDetail.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", ReservationDetail.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", ReservationDetail.UserCode, DbType.String);
            parameters.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);

            // Execute stored procedure
            await connection.ExecuteAsync("[SP_SaveUpdate_Guest_Master]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

            //int finalGuestId = parameters.Get<int>("@GuestId");

            return new ReservationSaveResult
            {
                InsertId = parameters.Get<int>("@GuestId"),
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<bool> CheckExistingReservation(string ReservationNo, string PropertyCode, string BranchCode, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = $"SELECT COUNT(1) FROM Tbl_Reservation_Master WHERE ReservationNo = @reservationno And PropertyCode = @propertycode And BranchCode = @branchcode";
            var count = await connection.ExecuteAsync(sql, new { reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode }, transaction);
            return count > 0;
        }

        public async Task<ReservationSaveResult> SaveUpdate_ReservationProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;
            //string bookingDetailsJson = JsonSerializer.Serialize(ReservationDetail.BookingDetail ?? new List<BookingDetailModel>(), new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //});

            string bookingDetailsJson = JsonSerializer.Serialize( ReservationDetail.BookingDetail ?? new List<BookingDetailModel>(), _camelCaseJsonOptions );

            // Map parameters for Stored Procedure [sp_SaveUpdate_Guest_Master]
            var parameters = new DynamicParameters();

            // Pass GuestId (set to null/0 for Insert, or provide an existing ID for Update)
            int? reservationIdParam = (ReservationDetail != null && ReservationDetail.ReservationId > 0) ? ReservationDetail.ReservationId : (int?)null;
            parameters.Add("@ReservationId", reservationIdParam, DbType.Int32, direction: ParameterDirection.InputOutput);

            parameters.Add("@ReservationId", reservationIdParam, DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            // Identification & Basic Info
            parameters.Add("@BookingNo", null, DbType.String);
            parameters.Add("@CheckInNo", null, DbType.String);
            parameters.Add("@ReservationNo", ReservationDetail.ReservationNo, DbType.String);
            parameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
            parameters.Add("@CompanyCode", guestDetail.CompanyCode, DbType.String);
            parameters.Add("@BookingDetailsJson", bookingDetailsJson, DbType.String);
            parameters.Add("@RoomCode", null, DbType.String);
            parameters.Add("@Traiff", null, DbType.Decimal);
            parameters.Add("@StayDays", ReservationDetail.NoofDays, DbType.Int32);
            parameters.Add("@NoofRooms", null, DbType.Int32);
            parameters.Add("@CUpdate", false, DbType.Boolean);
            parameters.Add("@SUpdate", false, DbType.Boolean);
            parameters.Add("@EUpdate", false, DbType.Boolean);
            parameters.Add("@Reference", guestDetail.Reference, DbType.String);
            parameters.Add("@BookingSourceName", guestDetail.BookingSourceName, DbType.String);
            parameters.Add("@SegmentName", guestDetail.SegmentName, DbType.String);
            parameters.Add("@SubSegmentName", guestDetail.SubSegmentName, DbType.String);
            parameters.Add("@PurposeName", guestDetail.PurposeName, DbType.String);
            parameters.Add("@SalesExecutiveName", guestDetail.SalesExecutiveName, DbType.String);
            parameters.Add("@ReservationPayModeName", guestDetail.ReservationPayModeName, DbType.String);
            parameters.Add("@PMSCMS", ReservationDetail.PMSCMS, DbType.String);

            parameters.Add("@FlagPMSCMS", false, DbType.Boolean);
            parameters.Add("@FlagAmend", false, DbType.Boolean);

            parameters.Add("@UserCode", ReservationDetail.UserCode, DbType.String);
            parameters.Add("@IPAddress", ReservationDetail.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", ReservationDetail.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", ReservationDetail.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", ReservationDetail.UserCode, DbType.String);
            parameters.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);

            await connection.ExecuteAsync("SP_SaveUpdate_Reservation_Master", parameters, transaction, commandType: CommandType.StoredProcedure);

            return new ReservationSaveResult
            {
                InsertId = parameters.Get<int>("@ReservationId"),
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<ReservationSaveResult> SaveUpdate_ReservationSlaveProcedure( ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;
                ReservationSaveResult lastResult = new();

                foreach (var row in ReservationDetail.BookingDetail)
                {
                    // Generate unique slave code for each detail row
                    string currentReservationSlaveCode = await _commonMethod.GetNextTransactionCode(
                        "Tbl_Reservation_Slave", "ReservationSlaveCode", "RSC", ReservationDetail.PropertyCode, ReservationDetail.BranchCode);

                    var slaveParams = new DynamicParameters();

                    // Input/Output Parameters
                    // Note: Pass the row's ID if you are updating an existing slave record, otherwise 0/null for inserts
                    int? slaveIdParam = (row.ReservationSlaveId > 0) ? row.ReservationSlaveId : (int?)null;
                    slaveParams.Add("@ReservationSlaveId", slaveIdParam, DbType.Int32, direction: ParameterDirection.InputOutput);

                    slaveParams.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
                    slaveParams.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    slaveParams.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                    slaveParams.Add("@ReservationSlaveCode", currentReservationSlaveCode, DbType.String);
                    slaveParams.Add("@ReservationNo", ReservationDetail.ReservationNo, DbType.String);
                    slaveParams.Add("@ArrivalDate", ReservationDetail.CheckInDate.Date, DbType.Date);
                    slaveParams.Add("@ArrivalTime", ReservationDetail.CheckInTime, DbType.Time);
                    slaveParams.Add("@DepatureDate", ReservationDetail.CheckOutDate.Date, DbType.Date);
                    slaveParams.Add("@DepatureTime", ReservationDetail.CheckOutTime, DbType.Time);
                    slaveParams.Add("@GuestCode", guestDetail?.GuestCode, DbType.String);
                    slaveParams.Add("@CompanyCode", guestDetail?.CompanyCode, DbType.String); ;
                    slaveParams.Add("@RoomCode", row.RoomCode, DbType.String);
                    slaveParams.Add("@NoofRooms", row.NoofRoom, DbType.Int32);
                    slaveParams.Add("@AdultPax", row.NoofAdult, DbType.Int32);
                    slaveParams.Add("@ChildPax", row.NoofChild, DbType.Int32);
                    slaveParams.Add("@TotalPax", row.NoofAdult + row.NoofChild, DbType.Int32);
                    slaveParams.Add("@Traiff", row.Traiff, DbType.Decimal);

                    // Ensure these match your stored procedure parameters
                    slaveParams.Add("@TaxAmount", row.TaxCalculationDetails?.TotalTaxAmount ?? 0, DbType.Decimal);
                    slaveParams.Add("@Discount", row.TaxCalculationDetails?.DiscountAmount ?? 0, DbType.Decimal);
                    slaveParams.Add("@NetPayTraiff", row.Netpayable, DbType.Decimal);
                    slaveParams.Add("@TaxCode", null, DbType.String); // Or company tax code if available
                    slaveParams.Add("@PlanCode", row.PlanCode, DbType.String);
                    slaveParams.Add("@DiscountType", row.DiscountType, DbType.String);
                    slaveParams.Add("@RoomAllocated", row.Allocated, DbType.Int32);

                    slaveParams.Add("@GUpdate", false, DbType.Boolean);
                    slaveParams.Add("@CUpdate", false, DbType.Boolean);
                    slaveParams.Add("@SUpdate", false, DbType.Boolean);
                    slaveParams.Add("@RUpdate", false, DbType.Boolean);
                    slaveParams.Add("@UserCode", ReservationDetail.UserCode, DbType.Int32);
                    slaveParams.Add("@IPAddress", ReservationDetail.IPAddress, DbType.String);
                    slaveParams.Add("@PropertyCode", ReservationDetail.PropertyCode, DbType.String);
                    slaveParams.Add("@BranchCode", ReservationDetail.BranchCode, DbType.String);
                    slaveParams.Add("@ModifiedBy", ReservationDetail.UserCode, DbType.Int32);
                    slaveParams.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);

                    // Execute stored procedure for the current row
                    await connection.ExecuteAsync("SP_SaveUpdate_Reservation_Slave", slaveParams, transaction: transaction, commandType: CommandType.StoredProcedure);

                    // Capture output values
                    int statusCode = slaveParams.Get<int>("@StatusCode");
                    string message = slaveParams.Get<string>("@Message");

                    if (statusCode == 0)
                    {
                        // Throwing an exception forces the outer transaction to roll back entirely
                        throw new Exception($"Failed to save reservation slave for room {row.RoomCode}: {message}");
                    }

                    lastResult = new ReservationSaveResult
                    {
                        InsertId = slaveParams.Get<int>("@ReservationSlaveId"),
                        ActionType = slaveParams.Get<string>("@ActionType"),
                        StatusCode = statusCode,
                        Message = message
                    };
                }

                return lastResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReservationSaveResult> SaveUpdate_ForeverDataProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;
                ReservationSaveResult lastResult = new();

                foreach (var row in ReservationDetail.BookingDetail)
                {
                    Random random = new();
                    string RandomNo = (random.Next(10, 20) + 100).ToString() + row.RoomCode;

                    var foreverParams = new DynamicParameters();

                    // Input/Output Parameters
                    int? foreverIdParam = (row.ForEverId > 0) ? row.ForEverId : (int?)null;
                    foreverParams.Add("@ForEverId", foreverIdParam, DbType.Int32, direction: ParameterDirection.InputOutput);

                    foreverParams.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
                    foreverParams.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    foreverParams.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                    foreverParams.Add("@BookingNo", null, DbType.String);
                    foreverParams.Add("@CheckInNo", null, DbType.String);
                    foreverParams.Add("@ReservationNo", ReservationDetail.ReservationNo, DbType.String);
                    foreverParams.Add("@RoomCode", row.RoomCode, DbType.String);
                    foreverParams.Add("@RoomType", row.RoomType, DbType.String);
                    foreverParams.Add("@NoofRooms", row.NoofRoom, DbType.Int32);
                    foreverParams.Add("@RoomStatus", "B", DbType.String);
                    foreverParams.Add("@ArrivalDate", ReservationDetail.CheckInDate.Date, DbType.Date);
                    foreverParams.Add("@DepatureDate", ReservationDetail.CheckOutDate.Date, DbType.Date);
                    foreverParams.Add("@TillTime", ReservationDetail.CheckOutTime, DbType.Time);
                    foreverParams.Add("@ReservationStatus", "CNF", DbType.String);
                    foreverParams.Add("@TranscationType", "RES", DbType.String);
                    foreverParams.Add("@TranscationStatus", "C", DbType.String);
                    foreverParams.Add("@PMSCMS", ReservationDetail.PMSCMS, DbType.String);
                    foreverParams.Add("@FlagPMSCMS", false, DbType.Boolean);
                    foreverParams.Add("@RandomNo", RandomNo, DbType.String);
                    foreverParams.Add("@CheckInFlag", "N", DbType.String);
                    foreverParams.Add("@UserCode", ReservationDetail.UserCode, DbType.Int32);
                    foreverParams.Add("@IPAddress", ReservationDetail.IPAddress, DbType.String);
                    foreverParams.Add("@PropertyCode", ReservationDetail.PropertyCode, DbType.String);
                    foreverParams.Add("@BranchCode", ReservationDetail.BranchCode, DbType.String);
                    foreverParams.Add("@ModifiedBy", ReservationDetail.UserCode, DbType.Int32);
                    foreverParams.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);

                    // Execute stored procedure for the current row
                    await connection.ExecuteAsync("SP_ForEver_Data", foreverParams, transaction: transaction, commandType: CommandType.StoredProcedure);

                    // Capture output values
                    int statusCode = foreverParams.Get<int>("@StatusCode");
                    string message = foreverParams.Get<string>("@Message");

                    if (statusCode == 0)
                    {
                        // Throwing an exception forces the outer transaction to roll back entirely
                        throw new Exception($"Failed to save ForEver data for room {row.RoomCode}: {message}");
                    }

                    lastResult = new ReservationSaveResult
                    {
                        InsertId = foreverParams.Get<int>("@ForEverId"),
                        ActionType = foreverParams.Get<string>("@ActionType"),
                        StatusCode = statusCode,
                        Message = message
                    };
                }

                return lastResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReservationSaveResult> SaveUpdate_ChannelManagerProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;
                ReservationSaveResult lastResult = new();

                string selectQuery = "SELECT * FROM Tbl_ForEver_Data WHERE ReservationNo = @reservationno AND CheckInFlag = 'N' And PropertyCode = @propertycode And BranchCode = @branchcode";
                var foreverrecords = await connection.QueryAsync<dynamic>(selectQuery,
                    new { reservationno = ReservationDetail.ReservationNo, ReservationDetail.PropertyCode, ReservationDetail.BranchCode }, transaction: transaction);

                if (foreverrecords.Any())
                {
                    string deleteQuery = @"
                        DELETE FROM Tbl_ChannelManager WHERE ReservationNo = @reservationno AND GuestCode = @guestcode 
                        AND PropertyCode = @propertycode AND BranchCode = @branchcode";

                    int deletedRecords = await connection.ExecuteAsync(deleteQuery, 
                        new { reservationno = ReservationDetail.ReservationNo, guestcode = guestDetail.GuestCode, propertycode = ReservationDetail.PropertyCode,
                            branchcode = ReservationDetail.BranchCode }, transaction: transaction );
                }

                foreach (var rec in foreverrecords)
                {
                    var cmParams = new DynamicParameters();

                    // Output Parameters from the Stored Procedure
                    int? IdParam = 0;
                    cmParams.Add("@ChannelManagerId", IdParam, DbType.Int32, ParameterDirection.Output);
                    cmParams.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
                    cmParams.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    cmParams.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                    // Input Parameters mapped from Tbl_ForEver_Data and method arguments
                    cmParams.Add("@BookingNo", null, DbType.String);
                    cmParams.Add("@CheckInNo", null, DbType.String);
                    cmParams.Add("@ReservationNo", rec.ReservationNo, DbType.String);
                    cmParams.Add("@RoomCode", rec.RoomCode, DbType.String);
                    cmParams.Add("@RoomType", rec.RoomType, DbType.String);
                    cmParams.Add("@NoofRooms", rec.NoofRooms, DbType.Int32);
                    cmParams.Add("@ArrivalDate", rec.ArrivalDate, DbType.Date);
                    cmParams.Add("@DepatureDate", rec.DepatureDate, DbType.Date);
                    cmParams.Add("@TillTime", rec.TillTime, DbType.Time);
                    cmParams.Add("@TranscationType", rec.TranscationType, DbType.String);
                    cmParams.Add("@TranscationStatus", rec.TranscationStatus, DbType.String);
                    cmParams.Add("@PMSCMS", rec.PMSCMS, DbType.String);
                    cmParams.Add("@FlagPMSCMS", rec.FlagPMSCMS, DbType.Boolean);
                    cmParams.Add("@RandomNo", rec.RandomNo, DbType.String);
                    cmParams.Add("@BookingSourceName", ReservationDetail.GuestDetail.BookingSourceName, DbType.String); // Change if your source name comes from a column
                    cmParams.Add("@UserCode", rec.UserCode, DbType.String);
                    cmParams.Add("@IPAddress", rec.IPAddress, DbType.String);
                    cmParams.Add("@PropertyCode", (string)rec.PropertyCode, DbType.String);
                    cmParams.Add("@BranchCode", (string)rec.BranchCode, DbType.String);
                    cmParams.Add("@ModifiedBy", rec.UserCode, DbType.String);
                    cmParams.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);

                    // Execute the stored procedure
                    await connection.ExecuteAsync("SP_ChannelManager_Data", cmParams, transaction: transaction, commandType: CommandType.StoredProcedure);

                    // Capture output values
                    int statusCode = cmParams.Get<int>("@StatusCode");
                    string message = cmParams.Get<string>("@Message");

                    if (statusCode == 0)
                    {
                        // Throwing an exception forces the outer transaction to roll back entirely
                        throw new Exception($"Failed to save ChannerManager data for room {rec.RoomCode}: {message}");
                    }

                    lastResult = new ReservationSaveResult
                    {
                        InsertId = cmParams.Get<int>("@ChannelManagerId"),
                        ActionType = cmParams.Get<string>("@ActionType"),
                        StatusCode = statusCode,
                        Message = message
                    };
                }

                return lastResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReservationSaveResult> SaveUpdate_AdvanceReceiptProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                List<PaymentDetailModel> paymentdetail = ReservationDetail.PaymentDetail;
                ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;
                ReservationSaveResult lastResult = new();
                
                //var FinDetails = await _commonMethod.GetFinancialMaster(ReservationDetail.BranchCode, ReservationDetail.PropertyCode);
                
                foreach (var record in paymentdetail)
                {
                    string currentReceiptNo = await _commonMethod.GetNextTransactionCode("Tbl_Advance_Receipt", "ReceiptNo", "ARC", ReservationDetail.PropertyCode, ReservationDetail.BranchCode);

                    //currentReceiptNo = $"{FinDetails?.FinCode ?? 0}{currentReceiptNo}";

                    string referenceNo = string.Empty;
                    string holderName = string.Empty;
                    string bankName = string.Empty;
                    DateTime transactionDate = _commonMethod.ConvertUtcToIst();

                    switch (record.MainModeType?.ToLower())
                    {
                        case "cheque":
                            referenceNo = record.ChequeDetail.ChequeNo;
                            bankName = record.ChequeDetail.BankName;
                            transactionDate = record.ChequeDetail.ChequeDate == DateTime.MinValue ? transactionDate : record.ChequeDetail.ChequeDate;
                            break;

                        case "card":
                            referenceNo = record.CardDetail.CardNumber;
                            holderName = record.CardDetail.CardHolderName;
                            break;

                        case "online":
                            referenceNo = record.OnlineDetail.AccountNo;
                            holderName = record.OnlineDetail.AccountHolderName;
                            bankName = record.OnlineDetail.BankName;
                            transactionDate = record.OnlineDetail.OnlineDate == DateTime.MinValue ? transactionDate : record.OnlineDetail.OnlineDate;
                            break;
                    }

                    var advrecptparameters = new DynamicParameters();

                    int? IdParam = (record != null && record.AdvanceReceiptId > 0) ? record.AdvanceReceiptId : (int?)null;
                    advrecptparameters.Add("@AdvanceReceiptId", IdParam, DbType.Int32, direction: ParameterDirection.InputOutput);
                    advrecptparameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
                    advrecptparameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    advrecptparameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                    // Identification & Basic Info
                    advrecptparameters.Add("@ReceiptNo", currentReceiptNo, DbType.String);
                    advrecptparameters.Add("@BookingNo", null, DbType.String);
                    advrecptparameters.Add("@CheckInNo", null, DbType.String);
                    advrecptparameters.Add("@ReservationNo", ReservationDetail.ReservationNo, DbType.String);
                    advrecptparameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
                    advrecptparameters.Add("@CompanyCode", guestDetail.CompanyCode, DbType.String);

                    // Address & Contact Info
                    advrecptparameters.Add("@MainModeId", record.MainModeId, DbType.Int32);
                    advrecptparameters.Add("@MainModeType", record.MainModeType, DbType.String);
                    advrecptparameters.Add("@SubModeId", record.SubModeId, DbType.Int32);
                    advrecptparameters.Add("@SubModeType", record.SubModeType, DbType.String);
                    advrecptparameters.Add("@ReferenceNo", referenceNo, DbType.String);
                    advrecptparameters.Add("@HolderName", holderName, DbType.String);
                    advrecptparameters.Add("@BankName", bankName, DbType.String);
                    advrecptparameters.Add("@TransactionDate", transactionDate, DbType.DateTime);
                    advrecptparameters.Add("@AmountPaid", record.AmountPaid, DbType.Decimal);
                    advrecptparameters.Add("@AmountBalance", record.AmountBalance, DbType.Decimal);

                    advrecptparameters.Add("@UserCode", ReservationDetail.UserCode, DbType.String);
                    advrecptparameters.Add("@IPAddress", ReservationDetail.IPAddress, DbType.String);
                    advrecptparameters.Add("@PropertyCode", ReservationDetail.PropertyCode, DbType.String);
                    advrecptparameters.Add("@BranchCode", ReservationDetail.BranchCode, DbType.String);
                    advrecptparameters.Add("@ModifiedBy", ReservationDetail.UserCode, DbType.String);
                    advrecptparameters.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);

                    await connection.ExecuteAsync("SP_SaveUpdate_AdvanceReceipt", advrecptparameters, transaction: transaction, commandType: CommandType.StoredProcedure);

                    // Capture output values
                    int statusCode = advrecptparameters.Get<int>("@StatusCode");
                    string message = advrecptparameters.Get<string>("@Message");

                    if (statusCode == 0)
                    {
                        // Throwing an exception forces the outer transaction to roll back entirely
                        throw new Exception($"Failed to save AdvanceReceipt data for room {record.MainModeType}: {message}");
                    }

                    lastResult = new ReservationSaveResult
                    {
                        InsertId = advrecptparameters.Get<int>("@ChannelManagerId"),
                        ActionType = advrecptparameters.Get<string>("@ActionType"),
                        StatusCode = statusCode,
                        Message = message
                    };
                }

                return lastResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReservationSaveResult> SaveUpdate_TaxDetailsProcedure(TaxBreakdownResponseDto TaxDetail,ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            // 1. Serialize the dictionary of lists into a single JSON string
            //string categorizedTaxesJson = JsonSerializer.Serialize(TaxDetail.CategorizedTaxes, new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //    WriteIndented = false
            //});
            ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;
            string categorizedTaxesJson = JsonSerializer.Serialize( TaxDetail.CategorizedTaxes ?? [], _camelCaseJsonOptions);

            // 2. Set up Dapper Parameters
            var parameters = new DynamicParameters();

            int? idParam = (TaxDetail != null && TaxDetail.TaxDetailsId > 0) ? TaxDetail.TaxDetailsId : (int?)null;
            parameters.Add("@TaxDetailsId", idParam, DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            parameters.Add("@ModuleName", "Reservation", DbType.String);
            parameters.Add("@ModuleCode", ReservationDetail.ReservationNo, DbType.String);
            parameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
            parameters.Add("@CompanyCode", guestDetail.CompanyCode, DbType.String);

            parameters.Add("@TariffAmount", TaxDetail.TariffAmount, DbType.Decimal);
            parameters.Add("@DiscountAmount", TaxDetail.DiscountAmount, DbType.Decimal);
            parameters.Add("@TotalTaxAmount", TaxDetail.TotalTaxAmount, DbType.Decimal);
            parameters.Add("@GrandTotal", TaxDetail.GrandTotal, DbType.Decimal);
            parameters.Add("@CategorizedTaxesJson", categorizedTaxesJson, DbType.String);
            parameters.Add("@UserCode", ReservationDetail.UserCode, DbType.String);
            parameters.Add("@IPAddress", ReservationDetail.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", ReservationDetail.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", ReservationDetail.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", ReservationDetail.UserCode, DbType.String);
            parameters.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);


            // 3. Execute Stored Procedure
            await connection.ExecuteAsync("[dbo].[SP_SaveUpdate_TaxDetails]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

            return new ReservationSaveResult
            {
                InsertId = parameters.Get<int>("@TaxDetailsId"),
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<ReservationSaveResult> SaveUpdate_ReservationChargesProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                List<ReservationChargeModel> ReservationChargeDetail = ReservationDetail.ReservationChargeDetail;
                ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;
                ReservationSaveResult lastResult = new();

                foreach (var row in ReservationChargeDetail)
                {
                    var parameters = new DynamicParameters();

                    int? idParam = (row.ReservationChargeId > 0) ? row.ReservationChargeId : (int?)null;
                    parameters.Add("@ReservationChargeId", idParam, DbType.Int32, direction: ParameterDirection.InputOutput);
                    parameters.Add("@BookingNo", null, DbType.String);
                    parameters.Add("@CheckInNo", null, DbType.String);
                    parameters.Add("@ReservationNo", ReservationDetail.ReservationNo, DbType.String);
                    parameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
                    parameters.Add("@CompanyCode", guestDetail.CompanyCode, DbType.String);

                    parameters.Add("@Category", row.Category, DbType.String);
                    parameters.Add("@ChargesDescription", row.ChargesDescription, DbType.String);
                    parameters.Add("@Quantity", row.Quantity, DbType.Int32);
                    parameters.Add("@Rate", row.Rate, DbType.Decimal);
                    parameters.Add("@TaxAmount", row.TaxAmount, DbType.Decimal);
                    parameters.Add("@TotalAmount", row.TotalAmount, DbType.Decimal);
                    parameters.Add("@NetAmount", row.NetAmount, DbType.Decimal);

                    parameters.Add("@UserCode", ReservationDetail.UserCode, DbType.String);
                    parameters.Add("@IPAddress", ReservationDetail.IPAddress, DbType.String);
                    parameters.Add("@PropertyCode", ReservationDetail.PropertyCode, DbType.String);
                    parameters.Add("@BranchCode", ReservationDetail.BranchCode, DbType.String);
                    parameters.Add("@ModifiedBy", ReservationDetail.UserCode, DbType.String);
                    parameters.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);

                    // Output status parameters
                    parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
                    parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                    // 3. Execute Stored Procedure
                    await connection.ExecuteAsync("[dbo].[SP_SaveUpdate_ReservationCharges]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);
                    
                    int statusCode = parameters.Get<int>("@StatusCode");
                    string message = parameters.Get<string>("@Message");

                    if (statusCode == 0)
                    {
                        // Throwing an exception forces the outer transaction to roll back entirely
                        throw new Exception($"Failed to save ReservationCharges data for room {row.Category}: {message}");
                    }

                    lastResult = new ReservationSaveResult
                    {
                        InsertId = parameters.Get<int>("@ReservationChargeId"),
                        ActionType = parameters.Get<string>("@ActionType"),
                        StatusCode = statusCode,
                        Message = message
                    };
                }

                return lastResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReservationSaveResult> SaveUpdate_SpecialInstructionProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                List<SpecialInstructionModel> InstructionDetail = ReservationDetail.InstructionDetail;
                ReservationGuestDetailModel guestDetail = ReservationDetail.GuestDetail;
                ReservationSaveResult lastResult = new();

                foreach (var row in InstructionDetail)
                {
                    var parameters = new DynamicParameters();

                    int? idParam = (row.SpecialInstructionId > 0) ? row.SpecialInstructionId : (int?)null;
                    parameters.Add("@SpecialInstructionId", idParam, DbType.Int32, direction: ParameterDirection.InputOutput);
                    parameters.Add("@InstructionDepartmentCode", row.InstructionDepartmentCode, DbType.String);
                    parameters.Add("@InstructionDepartmentName", row.InstructionDescriptionName, DbType.String);
                    parameters.Add("@SpecialInstructionDescription", row.SpecialInstructionDescription, DbType.String);
                    parameters.Add("@SpecialInstructionRemark", row.SpecialInstructionRemark, DbType.String);

                    parameters.Add("@BookingNo", null, DbType.String);
                    parameters.Add("@CheckInNo", null, DbType.String);
                    parameters.Add("@ReservationNo", ReservationDetail.ReservationNo, DbType.String);
                    parameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
                    parameters.Add("@CompanyCode", guestDetail.CompanyCode, DbType.String);

                    parameters.Add("@UserCode", ReservationDetail.UserCode, DbType.String);
                    parameters.Add("@IPAddress", ReservationDetail.IPAddress, DbType.String);
                    parameters.Add("@PropertyCode", ReservationDetail.PropertyCode, DbType.String);
                    parameters.Add("@BranchCode", ReservationDetail.BranchCode, DbType.String);
                    parameters.Add("@ModifiedBy", ReservationDetail.UserCode, DbType.String);
                    parameters.Add("@IsExistingReservation", ReservationDetail.IsExistingReservation, DbType.Boolean);

                    // Output status parameters
                    parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
                    parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                    // 3. Execute Stored Procedure
                    await connection.ExecuteAsync("[dbo].[SP_SaveUpdate_SpecialInstruction]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

                    int statusCode = parameters.Get<int>("@StatusCode");
                    string message = parameters.Get<string>("@Message");

                    if (statusCode == 0)
                    {
                        // Throwing an exception forces the outer transaction to roll back entirely
                        throw new Exception($"Failed to save SpecialInstruction data for room {row.InstructionDescriptionName}: {message}");
                    }

                    lastResult = new ReservationSaveResult
                    {
                        InsertId = parameters.Get<int>("@SpecialInstructionId"),
                        ActionType = parameters.Get<string>("@ActionType"),
                        StatusCode = statusCode,
                        Message = message
                    };
                }

                return lastResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteMasterRecord(string ReservationNo, string PropertyCode, string BranchCode, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = $"DELETE FROM Tbl_Reservation_Master WHERE ReservationNo = @reservationno And PropertyCode = @propertycode And BranchCode = @branchcode";
            var result = await connection.ExecuteAsync(sql, new { reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode }, transaction);
            return result > 0;
        }

        public async Task<int> DeleteReservation(string ReservationNo, string PropertyCode, string BranchCode, IDbConnection connection, IDbTransaction transaction)
        {
            string[] tables = {
            "Tbl_Reservation_Master", "Tbl_Reservation_Slave",
            "Tbl_ForEver_Data", "Tbl_ChannelManager",
            "Tbl_Advance_Receipt", "Tbl_Reservation_Charge",
            "Tbl_TaxDetails"
            };

            int totalRowsDeleted = 0;

            foreach (var table in tables)
            {
                var sql = $"DELETE FROM {table} WHERE ReservationNo = @reservationno OR BookingNo = @reservationno And PropertyCode = @propertycode And BranchCode = @branchcode";
                int rowsAffected = await connection.ExecuteAsync(sql, new { reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                totalRowsDeleted += rowsAffected;
            }

            return totalRowsDeleted;
        }

        public async Task<int> BookingCancellationDetail(BookingCancellationModel requestDetail)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                int totalRowsupdate = 0;

                string[] tables ={ 
                    "Tbl_Reservation_Master",
                    "Tbl_Reservation_Slave",
                    "Tbl_ForEver_Data"
                };

                string updateReservationQuery = string.Empty;

                foreach (var table in tables)
                {
                    if (table == "Tbl_ForEver_Data")
                    {
                        updateReservationQuery = @$"
                        UPDATE {table}
                        SET
                        RoomStatus = 'C',
                        ReservationStatus = 'CIN',
                        ModifiedDate = COALESCE(@modifieddate, modifieddate),
                        ModifiedBy = COALESCE(@modifiedby, modifiedby)
                        WHERE ReservationNo = @reservationno And PropertyCode = @propertycode And BranchCode = @branchcode;";
                    }
                    else
                    {
                        updateReservationQuery = @$"
                        UPDATE {table}
                        SET
                        CUpdate = 1,
                        ModifiedDate = COALESCE(@modifieddate, modifieddate),
                        ModifiedBy = COALESCE(@modifiedby, modifiedby)
                        WHERE ReservationNo = @reservationno And GuestCode = @guestcode  
                        And PropertyCode = @propertycode And BranchCode = @branchcode;";
                    }

                    var rowsAffected = await connection.ExecuteAsync(
                        updateReservationQuery,
                        new
                        {
                            reservationno = requestDetail.ModuleCode,
                            propertycode = requestDetail.PropertyCode,
                            branchcode = requestDetail.BranchCode,
                            guestcode = requestDetail.GuestCode,
                            modifiedby = requestDetail.UserCode,
                            modifieddate = _commonMethod.ConvertUtcToIst(),

                        });

                    totalRowsupdate += rowsAffected;
                }

                if (totalRowsupdate > 0)
                {
                    int insertresult = await InsertCancellationDetail(requestDetail);
                }
                return totalRowsupdate;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> InsertCancellationDetail(BookingCancellationModel requestDetail)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_BookingCancellation
                (
                    ModuleName, ModuleCode, GuestCode, CompanyCode, CancelDate, CancelTime, CancelRemarks,
                    MainModeId, MainModeType, SubModeId, SubModeType, ReferenceNo, HolderName, BankName, TransactionDate, 
                    NetAmount, AmountPaid, RefundPercentage, RefundAmount,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @modulename, @modulecode, @guestcode, @companycode, @canceldate, @canceltime, @cancelremarks,
                    @mainmodeid, @mainmodetype, @submodeid, @submodetype, @referenceno, @holdername, @bankname, @transactiondate, 
                    @netamount, @amountpaid, @refundpercentage, @refundamount,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS CancelId;";
                       
                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    modulename = requestDetail.ModuleName,
                    modulecode = requestDetail.ModuleCode,
                    guestcode = requestDetail.GuestCode,
                    companycode = requestDetail.CompanyCode,
                    canceldate = requestDetail.CancelDate,
                    canceltime = requestDetail.CancelTime,
                    cancelremarks = requestDetail.CancelRemarks,

                    mainmodeid = requestDetail.MainModeId,
                    mainmodetype = requestDetail.MainModeType,
                    submodeid = requestDetail.SubModeId,
                    submodetype = requestDetail.SubModeType,
                    referenceno = requestDetail.ReferenceNo,
                    holdername = requestDetail.HolderName,
                    bankname = requestDetail.BankName,
                    transactiondate = requestDetail.TransactionDate,
                    netamount = requestDetail.NetAmount,
                    amountpaid = requestDetail.AmountPaid,
                    refundpercentage = requestDetail.RefundPercentage,
                    refundamount = requestDetail.RefundAmount,

                    usercode = requestDetail.UserCode,
                    ipaddress = requestDetail.IPAddress,
                    propertycode = requestDetail.PropertyCode,
                    branchcode = requestDetail.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<GuestSearchDetailModel>> GetGuestSearchList(string SearchingName, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select GM.GuestId, GM.GuestCode, GM.GuestName, GM.GuestMobileNo, GM.GuestEmail, RM.ReservationNo
                From Tbl_Guest_Master GM
                Inner Join Tbl_Reservation_Master RM on RM.GuestCode = GM.GuestCode And RM.PropertyCode = GM.PropertyCode And RM.BranchCode = GM.BranchCode
                Where GM.PropertyCode = @propertycode And GM.BranchCode = @branchcode And GM.GuestName Like @guestname";

            var SearchDetailList = await connection.QueryAsync<GuestSearchDetailModel>(selectQuery, 
                new { guestname = $"%{SearchingName}%", propertycode = PropertyCode, branchcode = BranchCode });
            return SearchDetailList;
        }

        public async Task<GuestMaster> GetGuestDetails(string GuestCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select GuestId, GuestCode, GuestTitle, GuestName, GuestGender, GuestDOB, GuestDOA, GuestAddress, GuestCity, GuestState, 
                GuestNation, GuestPinCode, GuestMobileNo, GuestEmail, IdentityproofCode, IdentityproofName, GuestVehicleType, GuestVehicleNo, 
                IsGuestActive, UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Guest_Master 
                Where PropertyCode = @propertycode And BranchCode = @branchcode And GuestCode = @guestcode";

            var DetailList = await connection.QueryFirstOrDefaultAsync<GuestMaster>(selectQuery, 
                new { guestcode = GuestCode, propertycode = PropertyCode, branchcode = BranchCode });
            return DetailList;
        }

        public async Task<ReservationMaster> GetReservationMasterDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select ReservationId, BookingNo, CheckInNo, ReservationNo, GuestCode, CompanyCode, RoomCode, Traiff, StayDays, NoofRooms,
                CUpdate, SUpdate, EUpdate, Reference, BookingSourceName, SegmentName, SubSegmentName, PurposeName,
                SalesExecutiveName, ReservationPayModeName, PMSCMS, FlagPMSCMS, FlagAmend
                From Tbl_Reservation_Master 
                Where PropertyCode = @propertycode And BranchCode = @branchcode And GuestCode = @guestcode And ReservationNo = @reservationno";

            var DetailList = await connection.QueryFirstOrDefaultAsync<ReservationMaster>(selectQuery, 
                new { guestcode = GuestCode, reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode });
            return DetailList;
        }

        public async Task<List<ReservationSlave>> GetReservationSlaveDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select ReservationSlaveId, ReservationSlaveCode, ReservationNo, ArrivalDate, ArrivalTime, DepatureDate, DepatureTime, GuestCode, CompanyCode,
                RoomCode, NoofRooms, AdultPax, ChildPax, TotalPax, Traiff, TaxAmount, Discount, NetPayTraiff, TaxCode, PlanCode, DiscountType,
                RoomAllocated, GUpdate, CUpdate, SUpdate, RUpdate
                From Tbl_Reservation_Slave 
                Where PropertyCode = @propertycode And BranchCode = @branchcode And GuestCode = @guestcode And ReservationNo = @reservationno ";

            var DetailList = await connection.QueryAsync<ReservationSlave>(selectQuery, 
                new { guestcode = GuestCode, reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode });
            return DetailList.ToList();
        }

        public async Task<List<ForEverData>> GetForEverDataDetails(string ReservationNo, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select ForEverId, BookingNo, CheckInNo, ReservationNo, RoomCode, RoomType, NoofRooms, RoomStatus, ArrivalDate, DepatureDate, 
                TillTime, ReservationStatus, TranscationType, TranscationStatus, PMSCMS, FlagPMSCMS, RandomNo, CheckInFlag
                From Tbl_ForEver_Data
                Where PropertyCode = @propertycode And BranchCode = @branchcode And ReservationNo = @reservationno ";

            var DetailList = await connection.QueryAsync<ForEverData>(selectQuery,
                new { reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode });
            return DetailList.ToList();
        }

        public async Task<List<AdvanceReceipt>> GetAdvanceReceiptDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select AdvanceReceiptId, ReceiptNo, BookingNo, CheckInNo, ReservationNo, GuestCode, CompanyCode, MainModeId, MainModeType, 
                SubModeId, SubModeType, ReferenceNo, HolderName, BankName, TransactionDate, AmountPaid, AmountBalance	
                From Tbl_Advance_Receipt
                Where PropertyCode = @propertycode And BranchCode = @branchcode And GuestCode = @guestcode And ReservationNo = @reservationno ";

            var DetailList = await connection.QueryAsync<AdvanceReceipt>(selectQuery,
                new { guestcode = GuestCode, reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode });
            return DetailList.ToList();
        }

        public async Task<List<ReservationCharge>> GetReservationChargeDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select ReservationChargeId, BookingNo, CheckInNo, ReservationNo, GuestCode, CompanyCode, Category, ChargesDescription,
                Quantity, Rate, TaxAmount, TotalAmount, NetAmount 
                From Tbl_Reservation_Charge
                Where PropertyCode = @propertycode And BranchCode = @branchcode And GuestCode = @guestcode And ReservationNo = @reservationno ";

            var DetailList = await connection.QueryAsync<ReservationCharge>(selectQuery,
                new { guestcode = GuestCode, reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode });
            return DetailList.ToList();
        }

        public async Task<List<SpecialInstruction>> GetSpecialInstructionDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select SpecialInstructionId, InstructionDepartmentCode, InstructionDepartmentName, SpecialInstructionDescription, 
                SpecialInstructionRemark, BookingNo, CheckInNo, ReservationNo, GuestCode, CompanyCode
                From Tbl_SpecialInstruction
                Where PropertyCode = @propertycode And BranchCode = @branchcode And GuestCode = @guestcode And ReservationNo = @reservationno ";

            var DetailList = await connection.QueryAsync<SpecialInstruction>(selectQuery,
                new { guestcode = GuestCode, reservationno = ReservationNo, propertycode = PropertyCode, branchcode = BranchCode });
            return DetailList.ToList();
        }

        public async Task<TaxDetails> GetTaxDetailsDetails(string GuestCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectQuery = @"
                Select TaxDetailsId, ModuleName, ModuleCode, GuestCode,	CompanyCode, TariffAmount, DiscountAmount, 
                TotalTaxAmount, GrandTotal,CategorizedTaxesJson
                From Tbl_TaxDetails
                Where PropertyCode = @propertycode And BranchCode = @branchcode And GuestCode = @guestcode ";

            var detail = await connection.QueryFirstOrDefaultAsync<TaxDetails>(selectQuery,
                new { guestcode = GuestCode, propertycode = PropertyCode, branchcode = BranchCode });

            if (detail == null)
            {
                return new();
            }

            // 2. Safely deserialize the JSON string into the dictionary right inside the repository
            if (!string.IsNullOrEmpty(detail.CategorizedTaxesJson))
            {
                detail.TaxCalculationDetails = JsonSerializer.Deserialize<Dictionary<string, List<TaxItemDto>>>(
                    detail.CategorizedTaxesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            return detail;
        }

        #endregion
    }
}
