using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Office2016.Excel;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.CheckInAndOut;
using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.Reservation;
using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;
using FrontOfficeApplication.ProjectServiceLayer;
using FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.Interfaces;
using System.Data;
using System.Text.Json;

namespace FrontOfficeApplication.ProjectInfrastructure.CheckInAndOut
{
    public class CheckIn_DAL(IDbConnectionFactory connectionFactory, CommonMethod commonMethod) : ICheckIn_Repository
    {
        private readonly IDbConnectionFactory _factory = connectionFactory;
        private readonly CommonMethod _commonMethod = commonMethod;
        private static readonly JsonSerializerOptions _camelCaseJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<bool> CheckExistingCheckIn(string CheckInNo, string PropertyCode, string BranchCode, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = $"SELECT COUNT(1) FROM Tbl_Reservation_Master WHERE CheckInNo = @checkinno And PropertyCode = @propertycode And BranchCode = @branchcode";
            var count = await connection.ExecuteAsync(sql, new { checkinno = CheckInNo, propertycode = PropertyCode, branchcode = BranchCode }, transaction);
            return count > 0;
        }

        public async Task<CheckInSaveResult> SaveUpdate_GuestProcedure(CheckInGuestDetailModel GuestDetail, CheckInRequestModel ChechinDetail, IDbConnection connection, IDbTransaction transaction)
        {
            // Map parameters for Stored Procedure [sp_SaveUpdate_Guest_Master]
            var parameters = new DynamicParameters();

            // Pass GuestId (set to null/0 for Insert, or provide an existing ID for Update)
            //parameters.Add("@GuestId", guestDetail.GuestId > 0 ? guestDetail.GuestId : (int?)null, DbType.Int32);
            int? guestIdParam = (GuestDetail != null && GuestDetail.GuestId > 0) ? GuestDetail.GuestId : (int?)null;
            parameters.Add("@GuestId", guestIdParam, DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            // Identification & Basic Info
            parameters.Add("@GuestCode", GuestDetail.GuestCode, DbType.String);
            parameters.Add("@GuestTitle", GuestDetail.GuestTitle, DbType.String);
            parameters.Add("@GuestName", GuestDetail.GuestName, DbType.String);
            parameters.Add("@GuestGender", GuestDetail.GuestGender, DbType.String);
            parameters.Add("@GuestDOB", GuestDetail.GuestDOB, DbType.DateTime);
            parameters.Add("@GuestDOA", GuestDetail.GuestDOA, DbType.DateTime);

            // Address & Contact Info
            parameters.Add("@GuestAddress", GuestDetail.GuestAddress, DbType.String);
            parameters.Add("@GuestCity", GuestDetail.GuestCity, DbType.String);
            parameters.Add("@GuestState", GuestDetail.GuestState, DbType.String);
            parameters.Add("@GuestNation", GuestDetail.GuestNation, DbType.String);
            parameters.Add("@GuestPinCode", GuestDetail.GuestPinCode, DbType.String);
            parameters.Add("@GuestMobileNo", GuestDetail.GuestMobileNo, DbType.String);
            parameters.Add("@GuestEmail", GuestDetail.GuestEmail, DbType.String);

            // Identity Proof Details (Adjusted defaults based on your old snippet or DTO)
            parameters.Add("@IdentityproofCode", GuestDetail.IdentityproofCode, DbType.String);
            parameters.Add("@IdentityproofName", GuestDetail.IdentityproofName, DbType.String);

            // Vehicle Details (Set to null if not in your current DTO)
            parameters.Add("@GuestVehicleType", GuestDetail.GuestVehicleType, DbType.String);
            parameters.Add("@GuestVehicleNo", GuestDetail.GuestVehicleNo, DbType.String);

            // Status & Audit Fields
            parameters.Add("@IsGuestActive", true, DbType.Boolean);
            parameters.Add("@UserCode", ChechinDetail.UserCode, DbType.String);
            parameters.Add("@IPAddress", ChechinDetail.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", ChechinDetail.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", ChechinDetail.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", ChechinDetail.UserCode, DbType.String);
            parameters.Add("@IsExistingReservation", ChechinDetail.IsExistingCheckIn, DbType.Boolean);

            // Execute stored procedure
            await connection.ExecuteAsync("[SP_SaveUpdate_Guest_Master]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

            //int finalGuestId = parameters.Get<int>("@GuestId");

            return new CheckInSaveResult
            {
                InsertId = parameters.Get<int>("@GuestId"),
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<CheckInSaveResult> SaveUpdate_FormCProcedure(FormCRequestModel FormCDetail, CheckInRequestModel ChechinDetail, IDbConnection connection, IDbTransaction transaction)
        {
            // Map parameters for Stored Procedure [sp_SaveUpdate_Guest_Master]
            var parameters = new DynamicParameters();

            parameters.Add("@FormCId", FormCDetail.FormCId == 0 ? (int?)null : FormCDetail.FormCId, DbType.Int32, ParameterDirection.InputOutput);

            parameters.Add("@FormCCode", FormCDetail.FormCCode, DbType.String);
            parameters.Add("@GuestCode", FormCDetail.GuestCode, DbType.String);
            parameters.Add("@CompanyCode", FormCDetail.CompanyCode, DbType.String);
            parameters.Add("@CheckinNo", FormCDetail.CheckinNo, DbType.String);

            // India Address Details
            parameters.Add("@IndiaAddress", FormCDetail.IndiaDetails?.IndiaAddress, DbType.String);
            parameters.Add("@IndiaCity", FormCDetail.IndiaDetails?.IndiaCity, DbType.String);
            parameters.Add("@IndiaState", FormCDetail.IndiaDetails?.IndiaState, DbType.String);
            parameters.Add("@IndiaPinCode", FormCDetail.IndiaDetails?.IndiaPinCode, DbType.String);
            parameters.Add("@IndiaCountry", FormCDetail.IndiaDetails?.IndiaCountry, DbType.String);

            // Permanent Address Details
            parameters.Add("@PermanentAddress", FormCDetail.PermanentDetails?.PermanentAddress, DbType.String);
            parameters.Add("@PermanentCity", FormCDetail.PermanentDetails?.PermanentCity, DbType.String);
            parameters.Add("@PermanentState", FormCDetail.PermanentDetails?.PermanentState, DbType.String);
            parameters.Add("@PermanentPinCode", FormCDetail.PermanentDetails?.PermanentPinCode, DbType.String);
            parameters.Add("@PermanentCountry", FormCDetail.PermanentDetails?.PermanentCountry, DbType.String);

            // Passport Details
            parameters.Add("@PassportNo", FormCDetail.PassportDetails.PassportNo, DbType.String);
            parameters.Add("@PassportIssueCity", FormCDetail.PassportDetails.PassportIssueCity, DbType.String);
            parameters.Add("@PassportIssueCountry", FormCDetail.PassportDetails.PassportIssueCountry, DbType.String);
            parameters.Add("@PassportIssueDate", FormCDetail.PassportDetails.PassportIssueDate, DbType.Date);
            parameters.Add("@PassportExpiryDate", FormCDetail.PassportDetails.PassportExpiryDate, DbType.Date);

            // Visa Details
            parameters.Add("@VisaNo", FormCDetail.VisaDetails.VisaNo, DbType.String);
            parameters.Add("@VisaIssueCity", FormCDetail.VisaDetails.VisaIssueCity, DbType.String);
            parameters.Add("@VisaIssueCountry", FormCDetail.VisaDetails.VisaIssueCountry, DbType.String);
            parameters.Add("@VisaIssueDate", FormCDetail.VisaDetails.VisaIssueDate, DbType.Date);
            parameters.Add("@VisaExpiryDate", FormCDetail.VisaDetails.VisaExpiryDate, DbType.Date);
            parameters.Add("@VisaType", FormCDetail.VisaDetails.VisaType, DbType.String);

            // Travel & Stay Details
            parameters.Add("@ArrivalCity", FormCDetail.ArrivalDetails.ArrivalCity, DbType.String);
            parameters.Add("@ArrivalCountry", FormCDetail.ArrivalDetails.ArrivalCountry, DbType.String);
            parameters.Add("@ArrivalDate", FormCDetail.ArrivalDetails.ArrivalDate, DbType.Date);
            parameters.Add("@HotelArrivalDate", FormCDetail.ArrivalDetails.HotelArrivalDate, DbType.Date);
            parameters.Add("@HotelArrivalTime", FormCDetail.ArrivalDetails.HotelArrivalTime, DbType.Time);
            parameters.Add("@NoofStay", FormCDetail.ArrivalDetails.NoofStay, DbType.Int32);
            parameters.Add("@VisitPurpose", FormCDetail.ArrivalDetails.VisitPurpose, DbType.String);
            parameters.Add("@DepartureDate", FormCDetail.ArrivalDetails.DepartureDate, DbType.Date);

            // Next Destination Details
            parameters.Add("@NextDestinationPlace", FormCDetail.NextDestinationDetails.NextDestinationPlace, DbType.String);
            parameters.Add("@NextDestinationCity", FormCDetail.NextDestinationDetails.NextDestinationCity, DbType.String);
            parameters.Add("@NextDestinationState", FormCDetail.NextDestinationDetails.NextDestinationState, DbType.String);
            parameters.Add("@NextDestinationCountry", FormCDetail.NextDestinationDetails.NextDestinationCountry, DbType.String);

            // Contact & Audit Metadata
            parameters.Add("@MobileNo", FormCDetail.ContactDetails.MobileNo, DbType.String);
            parameters.Add("@Email", FormCDetail.ContactDetails.Email, DbType.String);

            parameters.Add("@UserCode", FormCDetail.UserCode, DbType.Int32);
            parameters.Add("@IPAddress", FormCDetail.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", FormCDetail.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", FormCDetail.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", FormCDetail.UserCode, DbType.Int32);
            parameters.Add("@IsExistingReservation", ChechinDetail.IsExistingCheckIn, DbType.Boolean);

            // --- Output Parameters ---
            parameters.Add("@ActionType", dbType: DbType.AnsiString, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            // Execute stored procedure
            await connection.ExecuteAsync("[SP_SaveUpdate_FormCMaster]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

            //int finalGuestId = parameters.Get<int>("@GuestId");

            return new CheckInSaveResult
            {
                InsertId = parameters.Get<int>("@FormCId"),
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<CheckInSaveResult> SaveUpdate_GRCBookingProcedure(string NextGRCCode, string PrimaryGuestCode, CheckInRequestModel ChechinDetail, IDbConnection connection, IDbTransaction transaction)
        {
            List<CheckInGuestDetailModel> guestdetails = ChechinDetail.GuestDetails;
            var companycode = guestdetails.Select(g => g.CompanyCode).FirstOrDefault();

            string formattedRoomNo = ChechinDetail.RoomNo != null ? string.Join(",", ChechinDetail.RoomNo) : "0";

            // Map parameters for Stored Procedure [sp_SaveUpdate_Guest_Master]
            var parameters = new DynamicParameters();

            parameters.Add("@GRCId", 0, DbType.Int32, ParameterDirection.InputOutput);
            parameters.Add("@GRCNo", NextGRCCode, DbType.String);
            parameters.Add("@CheckInNo", ChechinDetail.CheckInNo, DbType.String);
            parameters.Add("@GuestCode", PrimaryGuestCode, DbType.String);
            parameters.Add("@CompanyCode", companycode, DbType.String);
            parameters.Add("@RoomNo", formattedRoomNo, DbType.String);

            // --- Stay Schedule Details ---
            parameters.Add("@CheckInDate", ChechinDetail.CheckInDate, DbType.Date);
            parameters.Add("@CheckInTime", ChechinDetail.CheckInTime, DbType.Time);
            parameters.Add("@CheckOutDate", ChechinDetail.CheckOutDate, DbType.Date);
            parameters.Add("@CheckOutTime", ChechinDetail.CheckOutTime, DbType.Time);

            // --- Audit & Context Metadata (with fallback to parent CheckinDetail) ---
            parameters.Add("@UserCode", ChechinDetail.UserCode, DbType.Int32);
            parameters.Add("@IPAddress", ChechinDetail.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", ChechinDetail.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", ChechinDetail.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", ChechinDetail.UserCode, DbType.Int32);
            parameters.Add("@IsExistingReservation", ChechinDetail.IsExistingCheckIn, DbType.Boolean);

            // --- Output Parameters ---
            parameters.Add("@ActionType", dbType: DbType.AnsiString, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            // Execute stored procedure
            await connection.ExecuteAsync("[SP_SaveUpdate_GRCBookings]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

            //int finalGuestId = parameters.Get<int>("@GuestId");

            return new CheckInSaveResult
            {
                InsertId = parameters.Get<int>("@GRCId"),
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<CheckInSaveResult> SaveUpdate_CompanyMasterProcedure(CheckInGuestDetailModel GuestDetail, CheckInRequestModel ChechinDetail, IDbConnection connection, IDbTransaction transaction)
        {
            List<CheckInGuestDetailModel> guestdetails = ChechinDetail.GuestDetails;
            var companydetails = guestdetails.Select(g => g).FirstOrDefault();

            var parameters = new DynamicParameters();

            // --- Primary Identifiers ---
            parameters.Add("@CompanyId", 0, DbType.Int32, ParameterDirection.InputOutput);
            parameters.Add("@CompanyCode", companydetails.CompanyCode, DbType.String);
            parameters.Add("@CompanyName", companydetails.CompanyName, DbType.String);
            parameters.Add("@CompanyType", null, DbType.String);

            // --- Contact Details ---
            parameters.Add("@CompanyContactPerson", companydetails.GuestName, DbType.String);
            parameters.Add("@CompanyMobileNo", companydetails.GuestMobileNo, DbType.String);
            parameters.Add("@CompanyEmail", companydetails.GuestEmail, DbType.String);

            // --- Address Details ---
            parameters.Add("@CompanyAddress", companydetails.GuestAddress, DbType.String);
            parameters.Add("@CompanyCity", companydetails.GuestCity, DbType.String);
            parameters.Add("@CompanyState", companydetails.GuestState, DbType.String);
            parameters.Add("@CompanyNation", companydetails.GuestNation, DbType.String);
            parameters.Add("@CompanyPincode", companydetails.GuestPinCode, DbType.String);

            // --- Status & GST Details ---
            parameters.Add("@IsCompanyActive", true, DbType.Boolean);
            parameters.Add("@CompanyGSTNo", companydetails.GSTNo, DbType.String);
            parameters.Add("@CompanyGSTType", null, DbType.String);
            parameters.Add("@CompanyGSTStateCode", null, DbType.String);
            parameters.Add("@CompanyOpeningBalance", 0, DbType.Decimal);

            // --- Audit & Metadata ---
            parameters.Add("@UserCode", ChechinDetail.UserCode, DbType.Int32);
            parameters.Add("@IPAddress", ChechinDetail.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", ChechinDetail.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", ChechinDetail.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", ChechinDetail.UserCode, DbType.Int32);
            parameters.Add("@IsExistingReservation", ChechinDetail.IsExistingCheckIn, DbType.Boolean);

            // --- Output Parameters ---
            parameters.Add("@ActionType", dbType: DbType.AnsiString, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            // Execute Stored Procedure
            await connection.ExecuteAsync("[SP_SaveUpdate_CompanyMaster]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

            return new CheckInSaveResult
            {
                InsertId = parameters.Get<int?>("@CompanyId") ?? 0,
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<CheckInSaveResult> SaveUpdate_CheckInProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction)
        {
            var guest = request.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? request.GuestDetails.First();
            var bookingJson = JsonSerializer.Serialize(request.BookingDetail ?? [], _camelCaseJsonOptions);
            var parameters = new DynamicParameters();

            int? id = request.CheckInId > 0 ? request.CheckInId : null;
            parameters.Add("@CheckInId", id, DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("@BookingNo", null, DbType.String);
            parameters.Add("@CheckInNo", request.CheckInNo, DbType.String);
            parameters.Add("@CheckInType", request.CheckInType, DbType.String);
            parameters.Add("@ReservationNo", string.IsNullOrWhiteSpace(request.ReservationNo) ? "0" : request.ReservationNo, DbType.String);
            parameters.Add("@GuestCode", guest.GuestCode, DbType.String);
            parameters.Add("@CompanyCode", guest.CompanyCode, DbType.String);
            parameters.Add("@RoomCode", null, DbType.String);
            parameters.Add("@Traiff", null, DbType.Decimal);
            parameters.Add("@StayDays", request.NoofDays, DbType.Int32);
            parameters.Add("@NoofRooms", request.BookingDetail?.Count ?? 0, DbType.Int32);
            parameters.Add("@BookingDetailsJson", bookingJson, DbType.String);
            parameters.Add("@CUpdate", false, DbType.Boolean);
            parameters.Add("@SUpdate", false, DbType.Boolean);
            parameters.Add("@EUpdate", false, DbType.Boolean);
            parameters.Add("@Reference", request.Reference, DbType.String);
            parameters.Add("@GroupCode", request.GroupCode, DbType.String);
            parameters.Add("@SegmentName", guest.SegmentName, DbType.String);
            parameters.Add("@SubSegmentName", guest.SubSegmentName, DbType.String);
            parameters.Add("@PurposeName", guest.PurposeName, DbType.String);
            parameters.Add("@SalesExecutiveName", guest.SalesExecutiveName, DbType.String);
            parameters.Add("@CheckInPayModeName", null, DbType.String);
            parameters.Add("@DiscountType", request.DiscountType, DbType.String);
            parameters.Add("@DiscountValue", request.DiscountValue, DbType.Decimal);
            parameters.Add("@GracePeriod", request.GracePeriod, DbType.Decimal);
            parameters.Add("@FlagResChkIn", request.CheckInType?.Equals("Reservation", StringComparison.OrdinalIgnoreCase) ?? false, DbType.Boolean);
            parameters.Add("@UserCode", request.UserCode, DbType.String);
            parameters.Add("@IPAddress", request.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", request.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", request.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", request.UserCode, DbType.String);
            parameters.Add("@IsExistingCheckIn", request.IsExistingCheckIn, DbType.Boolean);

            parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("SP_SaveUpdate_CheckIn_Master", parameters, transaction, commandType: CommandType.StoredProcedure);

            return new CheckInSaveResult
            {
                InsertId = parameters.Get<int>("@CheckInId"),
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<CheckInSaveResult> SaveUpdate_CheckInSlaveProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction)
        {
            var guest = request.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? request.GuestDetails.First();
            var roomStatus = guest.GuestNation.Equals("INDIA", StringComparison.OrdinalIgnoreCase) ? "O" : "F";
            CheckInSaveResult lastResult = new();

            var bookingDetails = request.BookingDetail ?? [];

            foreach (var row in bookingDetails)
            {
                var parameters = new DynamicParameters();

                int? id = row.CheckInSlaveId > 0 ? row.CheckInSlaveId : null;
                parameters.Add("@CheckInSlaveId", id, DbType.Int32, direction: ParameterDirection.InputOutput);

                parameters.Add("@CheckInNo", request.CheckInNo, DbType.String);
                parameters.Add("@CheckInType", request.CheckInType, DbType.String);
                parameters.Add("@ReservationNo", string.IsNullOrWhiteSpace(request.ReservationNo) ? "0" : request.ReservationNo, DbType.String);
                parameters.Add("@ArrivalDate", request.CheckInDate.Date, DbType.Date);
                parameters.Add("@ArrivalTime", request.CheckInTime, DbType.Time);
                parameters.Add("@DepatureDate", request.CheckOutDate.Date, DbType.Date);
                parameters.Add("@DepatureTime", request.CheckOutTime, DbType.Time);
                parameters.Add("@GuestCode", guest.GuestCode, DbType.String);
                parameters.Add("@CompanyCode", guest.CompanyCode, DbType.String);
                parameters.Add("@RoomNo", row.RoomNo, DbType.String);
                parameters.Add("@RoomCode", row.RoomCode, DbType.String);
                parameters.Add("@NoofRooms", row.NoofRoom > 0 ? row.NoofRoom : 1, DbType.Int32);
                parameters.Add("@AdultPax", row.NoofAdult, DbType.Int32);
                parameters.Add("@ChildPax", row.NoofChild, DbType.Int32);
                parameters.Add("@TotalPax", row.NoofAdult + row.NoofChild, DbType.Int32);
                parameters.Add("@Traiff", row.Traiff, DbType.Decimal);
                parameters.Add("@TaxAmount", row.TaxCalculationDetails?.TotalTaxAmount ?? 0, DbType.Decimal);
                parameters.Add("@Discount", row.TaxCalculationDetails?.DiscountAmount ?? 0, DbType.Decimal);
                parameters.Add("@NetPayTraiff", row.Netpayable, DbType.Decimal);
                parameters.Add("@TaxCode", null, DbType.String);
                parameters.Add("@PlanCode", row.PlanCode, DbType.String);
                parameters.Add("@DiscountType", row.DiscountType, DbType.String);
                parameters.Add("@RoomAllocated", row.Allocated, DbType.Int32);
                parameters.Add("@GUpdate", false, DbType.Boolean);
                parameters.Add("@CUpdate", false, DbType.Boolean);
                parameters.Add("@SUpdate", false, DbType.Boolean);
                parameters.Add("@RUpdate", false, DbType.Boolean);
                parameters.Add("@GroupCode", request.GroupCode, DbType.String);
                parameters.Add("@GSTNo", guest.GSTNo, DbType.String);
                parameters.Add("@UserCode", request.UserCode, DbType.String);
                parameters.Add("@IPAddress", request.IPAddress, DbType.String);
                parameters.Add("@PropertyCode", request.PropertyCode, DbType.String);
                parameters.Add("@BranchCode", request.BranchCode, DbType.String);
                parameters.Add("@ModifiedBy", request.UserCode, DbType.String);
                parameters.Add("@IsExistingCheckIn", request.IsExistingCheckIn, DbType.Boolean);

                parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
                parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("SP_SaveUpdate_CheckIn_Slave", parameters, transaction, commandType: CommandType.StoredProcedure);

                var statusCode = parameters.Get<int>("@StatusCode");
                if (statusCode == 0)
                    throw new InvalidOperationException($"Failed to save check-in slave for room {row.RoomNo}: {parameters.Get<string>("@Message")}");

                const string roomSql = """
                    UPDATE Tbl_Room_Details SET RoomStatus = @RoomStatus, GroupCode = @GroupCode, CurrentCheckIn = @CheckInNo
                    WHERE RoomNo = @RoomNo AND Priority = 'P' AND PropertyCode = @PropertyCode AND BranchCode = @BranchCode
                    """;

                await connection.ExecuteAsync(roomSql, new
                {
                    RoomStatus = roomStatus,
                    request.GroupCode,
                    request.CheckInNo,
                    row.RoomNo,
                    request.PropertyCode,
                    request.BranchCode
                }, transaction);

                lastResult = new CheckInSaveResult
                {
                    InsertId = parameters.Get<int>("@CheckInSlaveId"),
                    ActionType = parameters.Get<string>("@ActionType"),
                    StatusCode = statusCode,
                    Message = parameters.Get<string>("@Message")
                };
            }

            return lastResult;
        }

        public async Task<CheckInSaveResult> SaveUpdate_ForeverDataProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction)
        {
            var isReservation = request.CheckInType.Equals("Reservation", StringComparison.OrdinalIgnoreCase);
            CheckInSaveResult lastResult = new();

            foreach (var row in request.BookingDetail)
            {
                if (isReservation)
                {
                    const string sql = """
                        UPDATE Tbl_ForEver_Data SET CheckInFlag = 'S', CheckInNo = @CheckInNo, TranscationType = 'C', TranscationStatus = 'C'
                        WHERE ReservationNo = @ReservationNo AND RoomCode = @RoomCode AND CheckInFlag = 'N' AND PropertyCode = @PropertyCode AND BranchCode = @BranchCode
                        """;

                    var updated = await connection.ExecuteAsync(sql, new
                    {
                        request.CheckInNo,
                        request.ReservationNo,
                        row.RoomCode,
                        request.PropertyCode,
                        request.BranchCode
                    }, transaction);

                    if (updated == 0)
                        throw new InvalidOperationException($"No reservation inventory found for room type {row.RoomCode}.");

                    lastResult = new CheckInSaveResult { StatusCode = 1, ActionType = "UPDATE", Message = "Reservation inventory checked in." };
                    continue;
                }

                var parameters = new DynamicParameters();
                int? id = row.ForEverId > 0 ? row.ForEverId : null;
                parameters.Add("@ForEverId", id, DbType.Int32, direction: ParameterDirection.InputOutput);
                parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
                parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);
                parameters.Add("@BookingNo", null, DbType.String);
                parameters.Add("@CheckInNo", request.CheckInNo, DbType.String);
                parameters.Add("@ReservationNo", "0", DbType.String);
                parameters.Add("@RoomCode", row.RoomCode, DbType.String);
                parameters.Add("@RoomType", row.RoomType, DbType.String);
                parameters.Add("@NoofRooms", 1, DbType.Int32);
                parameters.Add("@RoomStatus", "B", DbType.String);
                parameters.Add("@ArrivalDate", request.CheckInDate.Date, DbType.Date);
                parameters.Add("@DepatureDate", request.CheckOutDate.Date, DbType.Date);
                parameters.Add("@TillTime", request.CheckOutTime, DbType.Time);
                parameters.Add("@ReservationStatus", "CNF", DbType.String);
                parameters.Add("@TranscationType", "C", DbType.String);
                parameters.Add("@TranscationStatus", "C", DbType.String);
                parameters.Add("@PMSCMS", "PMS", DbType.String);
                parameters.Add("@FlagPMSCMS", false, DbType.Boolean);
                parameters.Add("@RandomNo", "0", DbType.String);
                parameters.Add("@CheckInFlag", "S", DbType.String);
                parameters.Add("@UserCode", request.UserCode, DbType.Int32);
                parameters.Add("@IPAddress", request.IPAddress, DbType.String);
                parameters.Add("@PropertyCode", request.PropertyCode, DbType.String);
                parameters.Add("@BranchCode", request.BranchCode, DbType.String);
                parameters.Add("@ModifiedBy", request.UserCode, DbType.Int32);
                parameters.Add("@IsExistingReservation", false, DbType.Boolean);

                await connection.ExecuteAsync("SP_ForEver_Data", parameters, transaction, commandType: CommandType.StoredProcedure);

                if (parameters.Get<int>("@StatusCode") == 0)
                    throw new InvalidOperationException(parameters.Get<string>("@Message"));

                lastResult = new CheckInSaveResult
                {
                    InsertId = parameters.Get<int>("@ForEverId"),
                    ActionType = parameters.Get<string>("@ActionType"),
                    StatusCode = parameters.Get<int>("@StatusCode"),
                    Message = parameters.Get<string>("@Message")
                };
            }

            return lastResult;
        }

        public async Task<CheckInSaveResult> SaveUpdate_ChannelManagerProcedure(CheckInRequestModel RequestDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                CheckInGuestDetailModel guestDetail = RequestDetail.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? RequestDetail.GuestDetails.First();
                CheckInSaveResult lastResult = new();

                string selectQuery = "SELECT * FROM Tbl_ForEver_Data WHERE ReservationNo = @reservationno AND CheckInFlag = 'N' And PropertyCode = @propertycode And BranchCode = @branchcode";
                var foreverrecords = await connection.QueryAsync<dynamic>(selectQuery,
                    new { reservationno = RequestDetail.ReservationNo, RequestDetail.PropertyCode, RequestDetail.BranchCode }, transaction: transaction);

                if (foreverrecords.Any())
                {
                    string deleteQuery = @"
                        DELETE FROM Tbl_ChannelManager WHERE ReservationNo = @reservationno AND GuestCode = @guestcode 
                        AND PropertyCode = @propertycode AND BranchCode = @branchcode";

                    int deletedRecords = await connection.ExecuteAsync(deleteQuery,
                        new
                        {
                            reservationno = RequestDetail.ReservationNo,
                            guestcode = guestDetail.GuestCode,
                            propertycode = RequestDetail.PropertyCode,
                            branchcode = RequestDetail.BranchCode
                        }, transaction: transaction);
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
                    cmParams.Add("@BookingSourceName", guestDetail.BookingSourceName, DbType.String); // Change if your source name comes from a column
                    cmParams.Add("@UserCode", rec.UserCode, DbType.String);
                    cmParams.Add("@IPAddress", rec.IPAddress, DbType.String);
                    cmParams.Add("@PropertyCode", (string)rec.PropertyCode, DbType.String);
                    cmParams.Add("@BranchCode", (string)rec.BranchCode, DbType.String);
                    cmParams.Add("@ModifiedBy", rec.UserCode, DbType.String);
                    cmParams.Add("@IsExistingReservation", RequestDetail.IsExistingCheckIn, DbType.Boolean);

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

                    lastResult = new CheckInSaveResult
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

        public async Task<CheckInSaveResult> SaveUpdate_AdvanceReceiptProcedure(CheckInRequestModel RequestDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                List<PaymentDetailModel> paymentdetail = RequestDetail.PaymentDetail;
                CheckInGuestDetailModel guestDetail = RequestDetail.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? RequestDetail.GuestDetails.First();
                CheckInSaveResult lastResult = new();

                //var FinDetails = await _commonMethod.GetFinancialMaster(RequestDetail.BranchCode, RequestDetail.PropertyCode);

                foreach (var record in paymentdetail)
                {
                    string currentReceiptNo = await _commonMethod.GetNextTransactionCode("Tbl_Advance_Receipt", "ReceiptNo", "ARC", RequestDetail.PropertyCode, RequestDetail.BranchCode);

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
                    advrecptparameters.Add("@ReservationNo", RequestDetail.ReservationNo, DbType.String);
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

                    advrecptparameters.Add("@UserCode", RequestDetail.UserCode, DbType.String);
                    advrecptparameters.Add("@IPAddress", RequestDetail.IPAddress, DbType.String);
                    advrecptparameters.Add("@PropertyCode", RequestDetail.PropertyCode, DbType.String);
                    advrecptparameters.Add("@BranchCode", RequestDetail.BranchCode, DbType.String);
                    advrecptparameters.Add("@ModifiedBy", RequestDetail.UserCode, DbType.String);
                    advrecptparameters.Add("@IsExistingReservation", RequestDetail.IsExistingCheckIn, DbType.Boolean);

                    await connection.ExecuteAsync("SP_SaveUpdate_AdvanceReceipt", advrecptparameters, transaction: transaction, commandType: CommandType.StoredProcedure);

                    // Capture output values
                    int statusCode = advrecptparameters.Get<int>("@StatusCode");
                    string message = advrecptparameters.Get<string>("@Message");

                    if (statusCode == 0)
                    {
                        // Throwing an exception forces the outer transaction to roll back entirely
                        throw new Exception($"Failed to save AdvanceReceipt data for room {record.MainModeType}: {message}");
                    }

                    lastResult = new CheckInSaveResult
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

        public async Task<CheckInSaveResult> SaveUpdate_TaxDetailsProcedure(TaxBreakdownResponseDto TaxDetail, CheckInRequestModel RequestDetail, IDbConnection connection, IDbTransaction transaction)
        {
            // 1. Serialize the dictionary of lists into a single JSON string
            //string categorizedTaxesJson = JsonSerializer.Serialize(TaxDetail.CategorizedTaxes, new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //    WriteIndented = false
            //});

            CheckInGuestDetailModel guestDetail = RequestDetail.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? RequestDetail.GuestDetails.First();
            string categorizedTaxesJson = JsonSerializer.Serialize(TaxDetail.CategorizedTaxes ?? [], _camelCaseJsonOptions);

            // 2. Set up Dapper Parameters
            var parameters = new DynamicParameters();

            int? idParam = (TaxDetail != null && TaxDetail.TaxDetailsId > 0) ? TaxDetail.TaxDetailsId : (int?)null;
            parameters.Add("@TaxDetailsId", idParam, DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("@ActionType", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            parameters.Add("@ModuleName", "Reservation", DbType.String);
            parameters.Add("@ModuleCode", RequestDetail.ReservationNo, DbType.String);
            parameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
            parameters.Add("@CompanyCode", guestDetail.CompanyCode, DbType.String);

            parameters.Add("@TariffAmount", TaxDetail.TariffAmount, DbType.Decimal);
            parameters.Add("@DiscountAmount", TaxDetail.DiscountAmount, DbType.Decimal);
            parameters.Add("@TotalTaxAmount", TaxDetail.TotalTaxAmount, DbType.Decimal);
            parameters.Add("@GrandTotal", TaxDetail.GrandTotal, DbType.Decimal);
            parameters.Add("@CategorizedTaxesJson", categorizedTaxesJson, DbType.String);
            parameters.Add("@UserCode", RequestDetail.UserCode, DbType.String);
            parameters.Add("@IPAddress", RequestDetail.IPAddress, DbType.String);
            parameters.Add("@PropertyCode", RequestDetail.PropertyCode, DbType.String);
            parameters.Add("@BranchCode", RequestDetail.BranchCode, DbType.String);
            parameters.Add("@ModifiedBy", RequestDetail.UserCode, DbType.String);
            parameters.Add("@IsExistingReservation", RequestDetail.IsExistingCheckIn, DbType.Boolean);


            // 3. Execute Stored Procedure
            await connection.ExecuteAsync("[dbo].[SP_SaveUpdate_TaxDetails]", parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

            return new CheckInSaveResult
            {
                InsertId = parameters.Get<int>("@TaxDetailsId"),
                ActionType = parameters.Get<string>("@ActionType"),
                StatusCode = parameters.Get<int>("@StatusCode"),
                Message = parameters.Get<string>("@Message")
            };
        }

        public async Task<CheckInSaveResult> SaveUpdate_CheckInChargesProcedure(CheckInRequestModel RequestDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                List<ReservationChargeModel> ReservationChargeDetail = RequestDetail.CheckInChargeDetail;
                CheckInGuestDetailModel guestDetail = RequestDetail.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? RequestDetail.GuestDetails.First();
                CheckInSaveResult lastResult = new();

                foreach (var row in ReservationChargeDetail)
                {
                    var parameters = new DynamicParameters();

                    int? idParam = (row.ReservationChargeId > 0) ? row.ReservationChargeId : (int?)null;
                    parameters.Add("@ReservationChargeId", idParam, DbType.Int32, direction: ParameterDirection.InputOutput);
                    parameters.Add("@BookingNo", null, DbType.String);
                    parameters.Add("@CheckInNo", null, DbType.String);
                    parameters.Add("@ReservationNo", RequestDetail.ReservationNo, DbType.String);
                    parameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
                    parameters.Add("@CompanyCode", guestDetail.CompanyCode, DbType.String);

                    parameters.Add("@Category", row.Category, DbType.String);
                    parameters.Add("@ChargesDescription", row.ChargesDescription, DbType.String);
                    parameters.Add("@Quantity", row.Quantity, DbType.Int32);
                    parameters.Add("@Rate", row.Rate, DbType.Decimal);
                    parameters.Add("@TaxAmount", row.TaxAmount, DbType.Decimal);
                    parameters.Add("@TotalAmount", row.TotalAmount, DbType.Decimal);
                    parameters.Add("@NetAmount", row.NetAmount, DbType.Decimal);

                    parameters.Add("@UserCode", RequestDetail.UserCode, DbType.String);
                    parameters.Add("@IPAddress", RequestDetail.IPAddress, DbType.String);
                    parameters.Add("@PropertyCode", RequestDetail.PropertyCode, DbType.String);
                    parameters.Add("@BranchCode", RequestDetail.BranchCode, DbType.String);
                    parameters.Add("@ModifiedBy", RequestDetail.UserCode, DbType.String);
                    parameters.Add("@IsExistingReservation", RequestDetail.IsExistingCheckIn, DbType.Boolean);

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

                    lastResult = new CheckInSaveResult
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

        public async Task<CheckInSaveResult> SaveUpdate_SpecialInstructionProcedure(CheckInRequestModel RequestDetail, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                List<SpecialInstructionModel> InstructionDetail = RequestDetail.InstructionDetail;
                CheckInGuestDetailModel guestDetail = RequestDetail.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? RequestDetail.GuestDetails.First();
                CheckInSaveResult lastResult = new();

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
                    parameters.Add("@ReservationNo", RequestDetail.ReservationNo, DbType.String);
                    parameters.Add("@GuestCode", guestDetail.GuestCode, DbType.String);
                    parameters.Add("@CompanyCode", guestDetail.CompanyCode, DbType.String);

                    parameters.Add("@UserCode", RequestDetail.UserCode, DbType.String);
                    parameters.Add("@IPAddress", RequestDetail.IPAddress, DbType.String);
                    parameters.Add("@PropertyCode", RequestDetail.PropertyCode, DbType.String);
                    parameters.Add("@BranchCode", RequestDetail.BranchCode, DbType.String);
                    parameters.Add("@ModifiedBy", RequestDetail.UserCode, DbType.String);
                    parameters.Add("@IsExistingReservation", RequestDetail.IsExistingCheckIn, DbType.Boolean);

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

                    lastResult = new CheckInSaveResult
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


        public async Task<int> InsertDiscountValue(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction)
        {
            int newId = 0;

            try
            {
                CheckInGuestDetailModel guestDetail = request.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? request.GuestDetails.First();

                const string insertquery = @"
                INSERT INTO dbo.Tbl_Discount_Slave
                (
                    CheckInNo, GuestCode, CompanyCode, RoomNo, CheckInDate, CheckOutDate, 
                    DiscountType, DiscountValue, SplitId, UserCode, IPAddress, PropertyCode, BranchCode 
                )
                VALUES
                (
                    @checkinno, @guestcode, @companycode, @roomno, @checkindate, @checkoutdate, 
                    @discounttype, @discountvalue, @splitid, @usercode, @ipaddress, @propertycode, @branchcode 
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS DiscountSlaveId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    checkinno = request.CheckInNo,
                    guestcode = guestDetail.GuestCode,
                    companycode = guestDetail.CompanyCode,
                    roomno = request.RoomNo,
                    checkindate = request.CheckInDate,
                    checkoutdate = request.CheckOutDate,
                    discounttype = request.DiscountType,
                    discountvalue = request.DiscountValue,
                    splitid = 0,
                    usercode = request.UserCode,
                    ipaddress = request.IPAddress,
                    propertycode = request.PropertyCode,
                    branchcode = request.BranchCode
                }, transaction: transaction);

                return newId;
            }
            catch
            {
                throw;
            }
        }

    }
}
