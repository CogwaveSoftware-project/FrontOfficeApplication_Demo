using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectServiceLayer;
using FrontOfficeApplication.ProjectServiceLayer.Master.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FrontOfficeApplication.ProjectInfrastructure.Master
{
    public class Master_DAL : IMaster_Repository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly CommonMethod _commonMethod;

        public Master_DAL(IDbConnectionFactory connectionFactory, CommonMethod commonMethod)
        {
            _factory = connectionFactory;
            _commonMethod = commonMethod;
        }

        #region Floor Master

        public async Task<IEnumerable<FloorMasterModel>> GetFloorMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select FloorId, FloorCode, FloorName, FloorDescription, FloorStatus, MaxRoom, Allocated, UnAllocated, 
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Floor_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By FloorId; ";

            var FloorMasterList = await connection.QueryAsync<FloorMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return FloorMasterList;
        }


        public async Task<int> CreateFloorMasterDetail(FloorMasterRequestModel floormaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Floor_Master 
                (
                    FloorCode, FloorName, FloorDescription, FloorStatus, MaxRoom, 
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES 
                (
                    @FloorCode, @FloorName, @FloorDescription, @FloorStatus, @MaxRoom,
                    @UserCode, @IPAddress, @PropertyCode, @BranchCode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT); AS FloorId";

                newId = await connection.QuerySingleAsync<int>(insertquery, floormaster);

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateFloorMasterDetail(FloorMasterRequestModel floormaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                // =========================================
                // UPDATE USER MASTER
                // =========================================
                var updatefloorQuery = @"
                UPDATE Tbl_Floor_Master
                SET
                FloorName = COALESCE(@floorname, floorname),
                FloorDescription = COALESCE(@floordescription, floordescription),
                FloorStatus = COALESCE(@floorstatus, floorstatus),
                MaxRoom = COALESCE(@maxroom, maxroom),
                IPAddress = COALESCE(@ipaddress, ipaddress),
                ModifiedDate = COALESCE(@modifieddate, modifieddate),
                ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE FloorCode = @floorcode And PropertyCode = @propertycode And BranchCode = @branchcode;";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        floorcode = floormaster.FloorCode,
                        floorname = floormaster.FloorName,
                        floordescription = floormaster.FloorDescription,
                        floorstatus = floormaster.FloorStatus,
                        maxroom = floormaster.MaxRoom,
                        ipaddress = floormaster.IPAddress,
                        propertycode = floormaster.PropertyCode,
                        branchcode = floormaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = floormaster.UserCode, 
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteFloorMasterDetail(int FloorId, string FloorCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Floor_Master 
                             WHERE FloorId = @floorid And FloorCode = @floorcode And PropertyCode = @propertycode And BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { floorid = FloorId, floorcode = FloorCode, propertycode = PropertyCode, branchcode = BranchCode });

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Financial Master

        public async Task<FinancialMasterModel> GetFinancialMasterList(string branchcode, string propertycode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select FinId, FinFromDate, FinToDate, FinCurrentYear, FinNewYear, CurrentStatus, BackUpTook, FinalClose, 
                FinCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Financial_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode; ";

            var FinMasterList = await connection.QueryFirstOrDefaultAsync<FinancialMasterModel>(selectquery, new { PropertyCode = propertycode, BranchCode = branchcode });
            return FinMasterList!;
        }

        public async Task<int> SaveorUpdateFinancialMasterDetail(FinancialMasterModel financialmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string sql = @"
                IF EXISTS (SELECT 1 FROM Tbl_Financial_Master WHERE PropertyCode = @propertycode And BranchCode = @branchcode)
                BEGIN
                UPDATE Tbl_Financial_Master
                SET
                    FinFromDate = COALESCE(@finfromdate, finfromdate),
                    FinToDate = COALESCE(@fintodate, fintodate),
                    FinCurrentYear = COALESCE(@fincurrentyear, fincurrentyear),
                    FinNewYear = COALESCE(@finnewyear, finnewyear),
                    CurrentStatus = COALESCE(@currentstatus, currentstatus),
                    BackUpTook = COALESCE(@backuptook, backuptook),
                    FinalClose = COALESCE(@finalclose, finalclose),
                    FinCode = COALESCE(@fincode, fincode),
                    UserCode = COALESCE(@usercode, usercode),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE FinId = @finid And PropertyCode = @propertycode And BranchCode = @branchcode;
                END
                ELSE
                BEGIN
                INSERT INTO Tbl_Financial_Master
                ( FinFromDate, FinToDate, FinCurrentYear, FinNewYear, CurrentStatus, BackUpTook, FinalClose, FinCode, UserCode, IPAddress, PropertyCode, BranchCode )
                VALUES
                ( @finfromdate, @fintodate, @fincurrentyear, @finnewyear, @currentstatus, @backuptook, @finalclose, @fincode, @usercode, @ipaddress, @propertycode, @branchcode);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                END";

                var parameters = new
                {
                    finid = financialmaster.FinId,
                    finfromdate = financialmaster.FinFromDate,
                    fintodate = financialmaster.FinToDate,
                    fincurrentyear = financialmaster.FinCurrentYear,
                    finnewyear = financialmaster.FinNewYear,
                    currentstatus = financialmaster.CurrentStatus,
                    backuptook = financialmaster.BackUpTook,
                    finalclose = financialmaster.FinalClose,
                    fincode = financialmaster.FinCode,
                    usercode = financialmaster.UserCode,
                    ipaddress = financialmaster.IPAddress,
                    propertycode = financialmaster.PropertyCode,
                    branchcode = financialmaster.BranchCode,
                    modifieddate = _commonMethod.ConvertUtcToIst(),
                    modifiedby = financialmaster.UserCode,

                };
                int rowsAffected = await connection.ExecuteAsync(sql, parameters);
                
                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteFinancialMasterDetail(int FinId, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Financial_Master WHERE FinId = @finid And PropertyCode = @propertycode And BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { finid = FinId, propertycode = PropertyCode, branchcode = BranchCode });

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region RoomTypeMaster

        public async Task<IEnumerable<RoomTypeMasterModel>> GetRoomTypeMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select RoomId, RoomCode, RoomName, RoomDescription, RoomMode, IsRoomActive, TotalRooms, Allocated, UnAllocated, 
                RoomPlanType, PlanId, PlanName, ChannelManagerCode, DefaultTraiff, PAX, PAXRates, ExtraAdult, ExtraChild, Taxinclude, Package,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Room_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By RoomId; ";

            var roomtypelist = await connection.QueryAsync<RoomTypeMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return roomtypelist;
        }

        //public async Task<IEnumerable<RoomTariffMasterModel>> GetRoomTariffMasterList(string BranchCode, string PropertyCode)
        //{
        //    using var connection = await _factory.CreateConnection(DbNames.HMS);

        //    var selectquery = @"Select RoomTariffId, RoomCode, DefaultTraiff, PAX, PAXRates, ExtraAdult, ExtraChild, TariffStatus, 
        //                        Taxinclude, Package, UserCode, IPAddress, PropertyCode, BranchCode
        //                        From Tbl_Room_Tariff
        //                        Where PropertyCode = @propertyCode And BranchCode = @branchCode; ";

        //    var roomtariffist = await connection.QueryAsync<RoomTariffMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
        //    return roomtariffist;
        //}

        public async Task<int> CreateRoomTypeMasterDetail(RoomTypeMasterRequestModel roomtypemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                BEGIN TRY

                BEGIN TRANSACTION;

                INSERT INTO Tbl_Room_Master
                (
                    RoomCode, RoomName, RoomDescription, RoomMode, IsRoomActive, TotalRooms, RoomPlanType, PlanId, PlanName, ChannelManagerCode,
                    DefaultTraiff, PAX, PAXRates, ExtraAdult, ExtraChild, Taxinclude, Package,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @roomcode, @roomname, @roomdescription, @roommode, @isroomactive, @totalrooms, @roomplantype, @planid, @planname, @channelmanagercode, 
                    @defaulttraiff, @pax, @paxrates, @extraadult, @extrachild, @taxinclude, @package,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                
                DECLARE @RoomId INT;
                SET @RoomId = CAST(SCOPE_IDENTITY() AS INT);
                
                COMMIT TRANSACTION;

                SELECT @RoomId;

                END TRY

                BEGIN CATCH

                    IF @@TRANCOUNT > 0
                        ROLLBACK TRANSACTION;

                    THROW;

                END CATCH;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    roomcode = roomtypemaster.RoomCode?.Trim(),
                    roomname = roomtypemaster.RoomName,
                    roomdescription = roomtypemaster.RoomDescription,
                    roommode = roomtypemaster.RoomMode,
                    isroomactive = roomtypemaster.IsRoomActive,
                    totalrooms = roomtypemaster.TotalRooms,
                    roomplantype = roomtypemaster.RoomPlanType,
                    planid = roomtypemaster.PlanId,
                    planname = roomtypemaster.PlanName,
                    channelmanagercode = roomtypemaster.ChannelManagerCode,
                    defaulttraiff = roomtypemaster.DefaultTraiff,
                    pax = roomtypemaster.Pax,
                    paxrates = JsonSerializer.Serialize(roomtypemaster.PaxRates),
                    extraadult = roomtypemaster.ExtraAdult,
                    extrachild = roomtypemaster.ExtraChild,
                    taxinclude = roomtypemaster.Taxinclude,
                    package = roomtypemaster.Package,
                    usercode = roomtypemaster.UserCode,
                    ipaddress = roomtypemaster.IPAddress,
                    propertycode = roomtypemaster.PropertyCode,
                    branchcode = roomtypemaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateRoomTypeMasterDetail(RoomTypeMasterRequestModel roomtypemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                // =========================================
                // UPDATE USER MASTER
                // =========================================
                var updatefloorQuery = @"BEGIN TRY

                BEGIN TRANSACTION;

                UPDATE Tbl_Room_Master
                SET
                    RoomName = COALESCE(@roomname, roomname),
                    RoomDescription = COALESCE(@roomdescription, roomdescription),
                    RoomMode = COALESCE(@roommode, roommode),
                    IsRoomActive = COALESCE(@isroomactive, isroomactive),
                    TotalRooms = COALESCE(@totalrooms, totalrooms),
                    RoomPlanType = COALESCE(@roomplantype, roomplantype),
                    planid = COALESCE(@planid, planid),
                    PlanName = COALESCE(@planname, planname),
                    ChannelManagerCode = COALESCE(@channelmanagercode, channelmanagercode),
                    DefaultTraiff = COALESCE(@defaulttraiff, defaulttraiff),
                    Pax = COALESCE(@pax, pax),
                    PaxRates = COALESCE(@paxrates, paxrates),
                    ExtraAdult = COALESCE(@extraadult, extraadult),
                    ExtraChild = COALESCE(@extrachild, extrachild),
                    TaxInclude = COALESCE(@taxinclude, taxinclude),
                    Package = COALESCE(@package, package),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode;

                COMMIT TRANSACTION;

                SELECT 1 AS Success,'Room updated successfully' AS Message;

                END TRY

                BEGIN CATCH

                    IF @@TRANCOUNT > 0
                        ROLLBACK TRANSACTION;

                    THROW;

                END CATCH;";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        roomcode = roomtypemaster.RoomCode?.Trim(),
                        roomname = roomtypemaster.RoomName,
                        roomdescription = roomtypemaster.RoomDescription,
                        roommode = roomtypemaster.RoomMode,
                        isroomactive = roomtypemaster.IsRoomActive,
                        totalrooms = roomtypemaster.TotalRooms,
                        roomplantype = roomtypemaster.RoomPlanType,
                        planid = roomtypemaster.PlanId,
                        planname = roomtypemaster.PlanName,
                        channelmanagercode = roomtypemaster.ChannelManagerCode,
                        defaulttraiff = roomtypemaster.DefaultTraiff,
                        pax = roomtypemaster.Pax,
                        paxrates = JsonSerializer.Serialize(roomtypemaster.PaxRates),
                        extraadult = roomtypemaster.ExtraAdult,
                        extrachild = roomtypemaster.ExtraChild,
                        taxinclude = roomtypemaster.Taxinclude,
                        package = roomtypemaster.Package,
                        ipaddress = roomtypemaster.IPAddress,
                        propertycode = roomtypemaster.PropertyCode,
                        branchcode = roomtypemaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = roomtypemaster.UserCode,
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteRoomTypeMasterDetail(int RoomId, string RoomCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete RoomTariffMaster 
                //const string deletequery1 = @"DELETE FROM Tbl_Room_Tariff
                //WHERE RoomCode = @roomcode AND PropertyCode = @propertycode  AND BranchCode = @branchcode ";
                //await connection.ExecuteAsync(deletequery1, new { roomcode = RoomCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                // Delete RoomTypeMaster
                const string deletequery2 = @"DELETE FROM Tbl_Room_Master
                WHERE RoomId = @roomid AND RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery2, new { roomid = RoomId, roomcode = RoomCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region PlanMaster

        public async Task<IEnumerable<PlanMasterModel>> GetPlanMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select PlanId, PlanCode, PlanName, PlanDescription, IsPlanActive, IsDefaultPlan, FoodRate, TaxCode,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Plan_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By PlanId; ";

            var planlist = await connection.QueryAsync<PlanMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return planlist;
        }


        public async Task<int> CreatePlanMasterDetail(PlanMasterModel planmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // First, deactivate all other tax rules
                if (planmaster.IsDefaultPlan == true)
                {
                    var deactivateQuery = @"
                        UPDATE dbo.Tbl_Plan_Master SET IsDefaultPlan = 0 WHERE PropertyCode = @propertycode AND BranchCode = @branchcode AND PlanCode <> @plancode;";

                    await connection.ExecuteAsync(deactivateQuery,
                       new
                       {
                           propertycode = planmaster.PropertyCode,
                           branchcode = planmaster.BranchCode,
                           plancode = planmaster.PlanCode
                       },
                       transaction);
                }

                var insertquery = @"
                INSERT INTO Tbl_Plan_Master
                (
                    PlanCode, PlanName, PlanDescription, IsPlanActive, IsDefaultPlan, FoodRate, TaxCode,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @plancode, @planname, @plandescription, @isplanactive, @isdefaultplan, @foodrate, @taxcode,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS planId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    plancode = planmaster.PlanCode,
                    planname = planmaster.PlanName,
                    plandescription = planmaster.PlanDescription,
                    isplanactive = planmaster.IsPlanActive,
                    isdefaultplan = planmaster.IsDefaultPlan,
                    foodrate = planmaster.FoodRate,
                    taxcode = planmaster.TaxCode,
                    usercode = planmaster.UserCode,
                    ipaddress = planmaster.IPAddress,
                    propertycode = planmaster.PropertyCode,
                    branchcode = planmaster.BranchCode,
                }, transaction);

                transaction.Commit();
                return newId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> UpdatePlanMasterDetail(PlanMasterModel planmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First, deactivate all other tax rules
                if (planmaster.IsDefaultPlan == true)
                {
                    var deactivateQuery = @"
                        UPDATE dbo.Tbl_Plan_Master SET IsDefaultPlan = 0 WHERE PropertyCode = @propertycode AND BranchCode = @branchcode AND PlanId <> @planid;";

                    await connection.ExecuteAsync(deactivateQuery,
                       new
                       {
                           propertycode = planmaster.PropertyCode,
                           branchcode = planmaster.BranchCode,
                           planid = planmaster.PlanId
                       },
                       transaction);
                }

                var updatefloorQuery = @"
                UPDATE Tbl_Plan_Master
                SET
                    PlanName = COALESCE(@planname, planname),
                    PlanDescription = COALESCE(@plandescription, plandescription),
                    IsPlanActive = COALESCE(@isplanactive, isplanactive),
                    IsDefaultPlan = COALESCE(@isdefaultplan, isdefaultplan),
                    FoodRate = COALESCE(@foodrate, foodrate),
                    TaxCode = COALESCE(@taxcode, taxcode),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE PlanId = @planid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        planid = planmaster.PlanId,
                        planCode = planmaster.PlanCode,
                        planname = planmaster.PlanName,
                        plandescription = planmaster.PlanDescription,
                        isplanactive = planmaster.IsPlanActive,
                        isdefaultplan = planmaster.IsDefaultPlan,
                        foodrate = planmaster.FoodRate,
                        taxcode = planmaster.TaxCode,
                        ipaddress = planmaster.IPAddress,
                        propertycode = planmaster.PropertyCode,
                        branchcode = planmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = planmaster.UserCode,
                    },
                    transaction);

                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                transaction.Commit();
                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> DeletePlanMasterDetail(int PlanId, string PlanCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Plan_Master
                WHERE PlanId = @planid AND PlanCode = @plancode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { planid = PlanId, plancode = PlanCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region RoomDetailMaster

        public async Task<IEnumerable<RoomDetailMasterModel>> GetRoomDetailMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select RoomDetailId, RoomNo, RoomCode, RoomDetailDescription, IsRoomDetailActive, DefaultPax, Priority, FloorCode, 
                FloorCode, RoomStatus, CurrentCheckIn, GroupCode, CheckInBillNo, UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Room_Details
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By RoomDetailId; ";

            var planlist = await connection.QueryAsync<RoomDetailMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return planlist;
        }


        public async Task<int> CreateRoomDetailMasterDetail(RoomDetailMasterRequestModel detailmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;
            connection.Open();

            var checkQuery = "SELECT COUNT(1) FROM Tbl_Room_Details WHERE RoomNo = @roomno AND RoomCode = @roomcode AND FloorCode = @floorcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var roomExists = await connection.QuerySingleAsync<int>(checkQuery,
                new { roomno = detailmaster.RoomNo, roomcode = detailmaster.RoomCode?.Trim(), floorcode = detailmaster.FloorCode, propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode });

            if (roomExists > 0)
            {
                return -1;
            }

            var FloorQuery = "Select MaxRoom, Allocated, UnAllocated From Tbl_Floor_Master Where FloorCode = @floorcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var Floorvalue = await connection.QuerySingleAsync(FloorQuery, new { floorcode = detailmaster.FloorCode, propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode });

            if (Floorvalue.MaxRoom == Floorvalue.Allocated)
            {
                return -2;
            }

            var roomQuery = "Select TotalRooms, Allocated, UnAllocated From Tbl_Room_Master Where RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var roomvalue = await connection.QuerySingleAsync(roomQuery, new { roomcode = detailmaster.RoomCode?.Trim(), propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode });

            if (roomvalue.TotalRooms == roomvalue.Allocated)
            {
                return -3;
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Room_Details
                (
                    RoomNo, RoomCode, RoomDetailDescription, IsRoomDetailActive, DefaultPax, Priority, FloorCode, RoomStatus, 
                    CurrentCheckIn, GroupCode, CheckInBillNo, UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @roomno, @roomcode, @roomdetaildescription, @isroomdetailactive, @defaultpax, @priority, @floorcode, @roomstatus,
                    @currentcheckin, @groupcode, @checkinbillno, @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS planId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    roomno = detailmaster.RoomNo,
                    roomcode = detailmaster.RoomCode?.Trim(),
                    roomdetaildescription = detailmaster.RoomDetailDescription,
                    isroomdetailactive = detailmaster.IsRoomDetailActive,
                    defaultpax = detailmaster.DefaultPax,
                    priority = detailmaster.Priority,
                    floorcode = detailmaster.FloorCode,
                    roomstatus = detailmaster.RoomStatus,
                    currentcheckin = detailmaster.CurrentCheckIn,
                    groupcode = detailmaster.GroupCode,
                    checkinbillno = detailmaster.CheckInBillNo,
                    usercode = detailmaster.UserCode,
                    ipaddress = detailmaster.IPAddress,
                    propertycode = detailmaster.PropertyCode,
                    branchcode = detailmaster.BranchCode,
                }, transaction);

                if (detailmaster.Priority == 'P')
                {
                    int floorAllocated = Floorvalue.Allocated + 1;
                    int floorUnallocated = Floorvalue.MaxRoom - floorAllocated;

                    var UpdateFloorQuery = "Update Tbl_Floor_Master Set Allocated = @allocated, UnAllocated = @unallocated Where FloorCode = @floorcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                    await connection.ExecuteAsync(UpdateFloorQuery, new { allocated = floorAllocated, unallocated = floorUnallocated,  floorcode = detailmaster.FloorCode, propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode }, transaction);

                    int roomAllocated = roomvalue.Allocated + 1;
                    int roomUnallocated = roomvalue.TotalRooms - roomAllocated;

                    var UpdateroomQuery = "Update Tbl_Room_Master Set Allocated = @allocated, UnAllocated = @unallocated Where RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                    await connection.ExecuteAsync(UpdateroomQuery, new { allocated = roomAllocated, unallocated = roomUnallocated, roomcode = detailmaster.RoomCode?.Trim(), propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode }, transaction);
                }
                
                transaction.Commit();
                return newId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateRoomDetailMasterDetail(RoomDetailMasterRequestModel detailmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            connection.Open();

            var checkQuery = "SELECT * FROM Tbl_Room_Details WHERE RoomNo = @roomno AND RoomDetailId = @roomdetailid AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var roomExists = await connection.QuerySingleAsync<RoomDetailMasterModel>(checkQuery,
                new { roomno = detailmaster.RoomNo, roomdetailid = detailmaster.RoomDetailId, propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode });

            if (roomExists == null)
            {
                return -1;
            }

            var FloorQuery = "Select MaxRoom, Allocated, UnAllocated From Tbl_Floor_Master Where FloorCode = @floorcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var Floorvalue = await connection.QuerySingleAsync(FloorQuery, new { floorcode = detailmaster.FloorCode, propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode });

            if (Floorvalue.MaxRoom == Floorvalue.Allocated)
            {
                return -2;
            }

            var roomQuery = "Select TotalRooms, Allocated, UnAllocated From Tbl_Room_Master Where RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var roomvalue = await connection.QuerySingleAsync(roomQuery, new { roomcode = detailmaster.RoomCode?.Trim(), propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode });

            if (roomvalue.TotalRooms == roomvalue.Allocated)
            {
                return -3;
            }

            using var transaction = connection.BeginTransaction();
            try
            {

                // 1. Check whether another record already has the new RoomNo + RoomCode
                //var duplicateQuery = @" SELECT COUNT(1) FROM Tbl_Room_Details WHERE RoomNo = @roomno AND RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode AND NOT ( RoomNo = @oldroomno AND RoomCode = @oldroomcode );";

                var duplicateQuery = @" SELECT COUNT(1) FROM Tbl_Room_Details WHERE RoomNo = @roomno AND RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode AND RoomDetailId <> @roomdetailid;";

                var duplicateCount = await connection.QuerySingleAsync<int>(duplicateQuery,
                    new
                    {
                        roomdetailid = detailmaster.RoomDetailId,
                        roomno = detailmaster.RoomNo,
                        roomcode = detailmaster.RoomCode?.Trim(),
                        oldroomno = detailmaster.OldRoomNo,
                        oldroomcode = detailmaster.OldRoomCode?.Trim(),
                        propertycode = detailmaster.PropertyCode,
                        branchcode = detailmaster.BranchCode
                    }, transaction);

                if (duplicateCount > 0)
                {
                    // Duplicate RoomNo + RoomCode
                    return -4;
                }

                var updatefloorQuery = @"
                UPDATE Tbl_Room_Details
                SET
                    RoomCode = COALESCE(@roomcode, roomcode),
                    RoomDetailDescription = COALESCE(@roomdetaildescription, roomdetaildescription),
                    IsRoomDetailActive = COALESCE(@isroomdetailactive, isroomdetailactive),
                    DefaultPax = COALESCE(@defaultpax, defaultpax),
                    Priority = COALESCE(@priority, priority),
                    FloorCode = COALESCE(@floorcode, floorcode),
                    RoomStatus = COALESCE(@roomstatus, roomstatus),
                    CurrentCheckIn = COALESCE(@currentcheckin, currentcheckin),
                    GroupCode = COALESCE(@groupcode, groupcode),
                    CheckInBillNo = COALESCE(@checkinbillno, checkinbillno),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE RoomDetailId = @roomdetailid And RoomNo = @oldroomno AND RoomCode = @oldroomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        roomdetailid = detailmaster.RoomDetailId,
                        roomno = detailmaster.RoomNo,
                        roomcode = detailmaster.RoomCode?.Trim(),
                        roomdetaildescription = detailmaster.RoomDetailDescription,
                        isroomdetailactive = detailmaster.IsRoomDetailActive,
                        defaultpax = detailmaster.DefaultPax,
                        priority = detailmaster.Priority,
                        floorcode = detailmaster.FloorCode,
                        roomstatus = detailmaster.RoomStatus,
                        currentcheckin = detailmaster.CurrentCheckIn,
                        groupcode = detailmaster.GroupCode,
                        checkinbillno = detailmaster.CheckInBillNo,
                        ipaddress = detailmaster.IPAddress,
                        propertycode = detailmaster.PropertyCode,
                        branchcode = detailmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = detailmaster.UserCode,
                        oldroomno = detailmaster.OldRoomNo,
                        oldroomcode = detailmaster.OldRoomCode?.Trim(),
                    }, transaction);

                if (rowsAffected == 0)
                {
                    return 0;
                }
                else
                {
                    if (roomExists.Priority == 'P' && detailmaster.Priority != 'P')
                    {
                        int floorAllocated = Floorvalue.Allocated - 1;
                        int floorUnallocated = Floorvalue.UnAllocated + 1;

                        var UpdateFloorQuery = "Update Tbl_Floor_Master Set Allocated = @allocated, UnAllocated = @unallocated Where FloorCode = @floorcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                        await connection.ExecuteAsync(UpdateFloorQuery, new { allocated = floorAllocated, unallocated = floorUnallocated, floorcode = detailmaster.OldFloorCode, propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode }, transaction);

                        int roomAllocated = roomvalue.Allocated - 1;
                        int roomUnallocated = roomvalue.UnAllocated + 1;

                        var UpdateroomQuery = "Update Tbl_Room_Master Set Allocated = @allocated, UnAllocated = @unallocated Where RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                        await connection.ExecuteAsync(UpdateroomQuery, new { allocated = roomAllocated, unallocated = roomUnallocated, roomcode = detailmaster.OldRoomCode?.Trim(), propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode }, transaction);
                    }
                    else if (roomExists.Priority != 'P' && detailmaster.Priority == 'P')
                    {
                        int floorAllocated = Floorvalue.Allocated + 1;
                        int floorUnallocated = Floorvalue.MaxRoom - floorAllocated;

                        var UpdateFloorQuery = "Update Tbl_Floor_Master Set Allocated = @allocated, UnAllocated = @unallocated Where FloorCode = @floorcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                        await connection.ExecuteAsync(UpdateFloorQuery, new { allocated = floorAllocated, unallocated = floorUnallocated, floorcode = detailmaster.FloorCode, propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode }, transaction);

                        int roomAllocated = roomvalue.Allocated + 1;
                        int roomUnallocated = roomvalue.TotalRooms - roomAllocated;

                        var UpdateroomQuery = "Update Tbl_Room_Master Set Allocated = @allocated, UnAllocated = @unallocated Where RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                        await connection.ExecuteAsync(UpdateroomQuery, new { allocated = roomAllocated, unallocated = roomUnallocated, roomcode = detailmaster.RoomCode?.Trim(), propertycode = detailmaster.PropertyCode, branchcode = detailmaster.BranchCode }, transaction);
                    }
                }

                transaction.Commit();
                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> DeleteRoomDetailMasterDetail(int RoomDetailId, string RoomNo, string RoomCode, string FloorCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            connection.Open();

            var FloorQuery = "Select MaxRoom, Allocated, UnAllocated From Tbl_Floor_Master Where FloorCode = @floorcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var Floorvalue = await connection.QuerySingleAsync(FloorQuery, new { floorcode = FloorCode, propertycode = PropertyCode, branchcode = BranchCode });

            var roomQuery = "Select TotalRooms, Allocated, UnAllocated From Tbl_Room_Master Where RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var roomvalue = await connection.QuerySingleAsync(roomQuery, new { roomcode = RoomCode, propertycode = PropertyCode, branchcode = BranchCode });


            try
            {
                using var transaction = connection.BeginTransaction();

                const string deletequery = @"DELETE FROM Tbl_Room_Details WHERE RoomDetailId = @roomdetailid AND RoomNo = @roomno AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { roomdetailid = RoomDetailId, roomno = RoomNo, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                if (rowsAffected > 0)
                {
                    int floorAllocated = Floorvalue.Allocated - 1;
                    int floorUnallocated = Floorvalue.UnAllocated + 1;

                    var UpdateFloorQuery = "Update Tbl_Floor_Master Set Allocated = @allocated, UnAllocated = @unallocated Where FloorCode = @floorcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                    await connection.ExecuteAsync(UpdateFloorQuery, new { allocated = floorAllocated, unallocated = floorUnallocated, floorcode = FloorCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                    int roomAllocated = roomvalue.Allocated - 1;
                    int roomUnallocated = roomvalue.UnAllocated + 1;

                    var UpdateroomQuery = "Update Tbl_Room_Master Set Allocated = @allocated, UnAllocated = @unallocated Where RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                    await connection.ExecuteAsync(UpdateroomQuery, new { allocated = roomAllocated, unallocated = roomUnallocated, roomcode = RoomCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);
                }
                return rowsAffected;

            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region PurposeMaster

        public async Task<IEnumerable<PurposeMasterModel>> GetPurposeMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select PurposeId, PurposeCode, PurposeName, IsPurposeActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Purpose_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By PurposeId; ";

            var planlist = await connection.QueryAsync<PurposeMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return planlist;
        }

        public async Task<int> CreatePurposeMasterDetail(PurposeMasterModel purposemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Purpose_Master
                (
                    PurposeCode, PurposeName, IsPurposeActive, 
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @PurposeCode, @PurposeName, @IsPurposeActive, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS PurposeId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    purposecode = purposemaster.PurposeCode,
                    purposename = purposemaster.PurposeName,
                    ispurposeactive = purposemaster.IsPurposeActive,
                    usercode = purposemaster.UserCode,
                    ipaddress = purposemaster.IPAddress,
                    propertycode = purposemaster.PropertyCode,
                    branchcode = purposemaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdatePurposeMasterDetail(PurposeMasterModel purposemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Purpose_Master
                SET
                    PurposeName = COALESCE(@purposename, purposename),
                    IsPurposeActive = COALESCE(@ispurposeactive, ispurposeactive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE PurposeId = @purposeid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        purposeid = purposemaster.PurposeId,
                        purposecode = purposemaster.PurposeCode,
                        purposename = purposemaster.PurposeName,
                        ispurposeactive = purposemaster.IsPurposeActive,
                        ipaddress = purposemaster.IPAddress,
                        propertycode = purposemaster.PropertyCode,
                        branchcode = purposemaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = purposemaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeletePurposeMasterDetail(int PurposeId, string PurposeCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Purpose_Master
                WHERE PurposeId = @purposeid AND PurposeCode = @purposecode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { purposeid = PurposeId, purposecode = PurposeCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region PaymentMaster

        public async Task<IEnumerable<PaymentMainModeModel>> GetPaymentMainMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = "SELECT MainModeId, MainModeType, IsMainModeActive, UserCode, IPAddress, PropertyCode, BranchCode FROM Tbl_PaymentMainMode_Master " +
                " Where PropertyCode = @propertycode And BranchCode = @branchcode Order By MainModeId";
            var MainList = await connection.QueryAsync<PaymentMainModeModel>(selectquery, new { propertycode = PropertyCode , branchcode = BranchCode });
            return MainList;
        }

        public async Task<IEnumerable<PaymentSubModeModel>> GetPaymentSubMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = "SELECT SubModeId, SubModeType, IsSubModeActive, MainModeId, UserCode, IPAddress, PropertyCode, BranchCode " +
                " FROM Tbl_PaymentSubMode_Master " +
                "  Where PropertyCode = @propertycode And BranchCode = @branchcode Order By MainModeId ";
            var SubList = await connection.QueryAsync<PaymentSubModeModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return SubList;
        }

        public async Task<int> SaveMainandSubPaymentMode(PaymentModeRequestModel requestmodel)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                int MainModeId = requestmodel.MainModeId;
                int SubModeId = 0;

                //------------------------------------------------
                // INSERT PaymentMainMode IF NEW
                //------------------------------------------------

                if (!requestmodel.IsExistingMainMode)
                {
                    string mainMenuQuery = @"
                    INSERT INTO Tbl_PaymentMainMode_Master ( MainModeType, IsMainModeActive, UserCode, IPAddress, PropertyCode, BranchCode )
                    VALUES
                    ( @mainmodetype, @ismainmodeactive, @usercode, @ipaddress, @propertycode, @branchcode )

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    MainModeId = await connection.ExecuteScalarAsync<int>(mainMenuQuery, new
                    {
                        mainmodetype = requestmodel.MainModeType,
                        ismainmodeactive = requestmodel.IsMainModeActive,
                        usercode = requestmodel.UserCode,
                        ipaddress = requestmodel.IPAddress,
                        propertycode = requestmodel.PropertyCode,
                        branchcode = requestmodel.BranchCode
                    }, transaction);
                }

                //------------------------------------------------
                // INSERT PaymentSubMode
                //------------------------------------------------

                //if (requestmodel.SubModeType != null && requestmodel.SubModeType.ToLower() != "string")
                if (requestmodel.SubModeType != null && !string.Equals(requestmodel.SubModeType, "string", StringComparison.OrdinalIgnoreCase))
                {
                    string subMenuQuery = @"
                    INSERT INTO Tbl_PaymentSubMode_Master ( SubModeType, IsSubModeActive, MainModeId, UserCode, IPAddress, PropertyCode, BranchCode )
                    VALUES
                    ( @submodetype, @issubmodeactive, @mainmodeid, @usercode, @ipaddress, @propertycode, @branchcode )

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    SubModeId = await connection.ExecuteScalarAsync<int>(subMenuQuery,new 
                    { 
                        submodetype = requestmodel.SubModeType, 
                        issubmodeactive = requestmodel.IsSubModeActive,
                        mainmodeid = MainModeId,
                        usercode = requestmodel.UserCode,
                        ipaddress = requestmodel.IPAddress,
                        propertycode = requestmodel.PropertyCode,
                        branchcode = requestmodel.BranchCode
                    }, transaction);
                }

                transaction.Commit();

                return MainModeId > 0 ? MainModeId : SubModeId;

            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteMainPaymentModeDetail(int MainModeId, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            try
            {
                const string deletequery = @"DELETE FROM Tbl_PaymentMainMode_Master 
                WHERE MainModeId = @mainmodeid AND PropertyCode = @propertycode AND BranchCode = @branchcode;";

                var rowsAffected = await connection.ExecuteAsync(deletequery, new { mainmodeid = MainModeId, propertycode = PropertyCode, branchcode = BranchCode });

                return (rowsAffected) > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteSubPaymentModeDetail(int SubModeId, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            try
            {
                const string deletequery = @"DELETE FROM Tbl_PaymentSubMode_Master 
                WHERE SubModeId = @submodeid AND PropertyCode = @propertycode AND BranchCode = @branchcode;";

                var rowsAffected = await connection.ExecuteAsync(deletequery, new { submodeid = SubModeId, propertycode = PropertyCode, branchcode = BranchCode });

                return (rowsAffected) > 0;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region IdentityproofMaster

        public async Task<IEnumerable<IdentityproofMasterModel>> GetIdentityproofMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select IdentityproofId, IdentityproofCode, IdentityproofName, IsIdentityproofActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Identityproof_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By IdentityproofId; ";

            var prooflist = await connection.QueryAsync<IdentityproofMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return prooflist;
        }

        public async Task<int> CreateIdentityproofMasterDetail(IdentityproofMasterModel proofmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Identityproof_Master
                (
                    IdentityproofCode, IdentityproofName, IsIdentityproofActive, 
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @identityproofcode, @identityproofname, @isidentityproofactive, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS planId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    identityproofcode = proofmaster.IdentityproofCode,
                    identityproofname = proofmaster.IdentityproofName,
                    isidentityproofactive = proofmaster.IsIdentityproofActive,
                    usercode = proofmaster.UserCode,
                    ipaddress = proofmaster.IPAddress,
                    propertycode = proofmaster.PropertyCode,
                    branchcode = proofmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateIdentityproofMasterDetail(IdentityproofMasterModel proofmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Identityproof_Master
                SET
                    IdentityproofName = COALESCE(@identityproofname, identityproofname),
                    IsIdentityproofActive = COALESCE(@isidentityproofactive, isidentityproofactive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE IdentityproofId = @identityproofid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        identityproofid = proofmaster.IdentityproofId,
                        identityproofcode = proofmaster.IdentityproofCode,
                        identityproofname = proofmaster.IdentityproofName,
                        isidentityproofactive = proofmaster.IsIdentityproofActive,
                        ipaddress = proofmaster.IPAddress,
                        propertycode = proofmaster.PropertyCode,
                        branchcode = proofmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = proofmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteIdentityproofMasterDetail(int IdentityproofId, string IdentityproofCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Identityproof_Master
                WHERE IdentityproofId = @identityproofid AND IdentityproofCode = @identityproofcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { identityproofid = IdentityproofId, identityproofcode = IdentityproofCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region TaxMaster

        public async Task<IEnumerable<TaxMasterModel>> GetTaxMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select TaxId, TaxCode, TaxName, IsTaxActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Tax_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By TaxId; ";

            var Taxlist = await connection.QueryAsync<TaxMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Taxlist;
        }

        public async Task<int> CreateTaxMasterDetail(TaxMasterModel taxmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Tax_Master
                (
                    TaxCode, TaxName, IsTaxActive, 
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @taxcode, @taxname, @istaxactive, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS TaxId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    taxcode = taxmaster.TaxCode,
                    taxname = taxmaster.TaxName,
                    istaxactive = taxmaster.IsTaxActive,
                    usercode = taxmaster.UserCode,
                    ipaddress = taxmaster.IPAddress,
                    propertycode = taxmaster.PropertyCode,
                    branchcode = taxmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateTaxMasterDetail(TaxMasterModel taxmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Tax_Master
                SET
                    TaxName = COALESCE(@taxname, taxname),
                    IsTaxActive = COALESCE(@istaxactive, istaxactive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE TaxId = @taxid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        taxid = taxmaster.TaxId,
                        taxcode = taxmaster.TaxCode,
                        taxname = taxmaster.TaxName,
                        istaxactive = taxmaster.IsTaxActive,
                        ipaddress = taxmaster.IPAddress,
                        propertycode = taxmaster.PropertyCode,
                        branchcode = taxmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = taxmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteTaxMasterDetail(int TaxId, string TaxCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Tax_Master
                WHERE TaxId = @taxid AND TaxCode = @taxcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { taxid = TaxId, taxcode = TaxCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region TaxDescriptionMaster

        public async Task<IEnumerable<TaxDescriptionMasterModel>> GetTaxDescriptionMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select TaxDescriptionId, TaxDescriptionCode, TaxDescriptionName, TaxPercentage, IsTaxDescriptionActive,
                TaxCode, FromAmount, ToAmount, FromDate, ToDate, 
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_TaxDescription_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By IdentityproofId; ";

            var TaxDescriptionlist = await connection.QueryAsync<TaxDescriptionMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return TaxDescriptionlist;
        }

        public async Task<int> CreateTaxDescriptionMasterDetail(TaxDescriptionMasterModel descriptionmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_TaxDescription_Master
                (
                    TaxDescriptionCode, TaxDescriptionName, TaxPercentage, IsTaxDescriptionActive, TaxCode, FromAmount, ToAmount, FromDate, ToDate,  
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @taxdescriptioncode, @taxdescriptionname, @taxpercentage, @istaxdescriptionactive, @taxcode, @fromamount, @toamount, @fromdate, @todate, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS TaxDescriptionId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    taxdescriptioncode = descriptionmaster.TaxDescriptionCode,
                    taxdescriptionname = descriptionmaster.TaxDescriptionName,
                    taxpercentage = descriptionmaster.TaxPercentage,
                    istaxdescriptionactive = descriptionmaster.IsTaxDescriptionActive,
                    taxcode = descriptionmaster.TaxCode,
                    fromamount = descriptionmaster.FromAmount,
                    toamount = descriptionmaster.ToAmount,
                    fromdate = descriptionmaster.FromDate,
                    todate = descriptionmaster.ToDate,
                    usercode = descriptionmaster.UserCode,
                    ipaddress = descriptionmaster.IPAddress,
                    propertycode = descriptionmaster.PropertyCode,
                    branchcode = descriptionmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateTaxDescriptionMasterDetail(TaxDescriptionMasterModel descriptionmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_TaxDescription_Master
                SET
                    TaxDescriptionName = COALESCE(@taxdescriptionname, taxdescriptionname),
                    TaxPercentage = COALESCE(@taxpercentage, taxpercentage),
                    IsTaxDescriptionActive = COALESCE(@istaxdescriptionactive, istaxdescriptionactive),
                    TaxCode = COALESCE(@taxcode, taxcode),
                    FromAmount = COALESCE(@fromamount, fromamount),
                    ToAmount = COALESCE(@toamount, toamount),
                    FromDate = COALESCE(@fromdate, fromdate),
                    ToDate = COALESCE(@toamount, todate),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE TaxDescriptionId = @taxdescriptionid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        taxdescriptionid = descriptionmaster.TaxDescriptionId,
                        taxdescriptioncode = descriptionmaster.TaxDescriptionCode,
                        taxdescriptionname = descriptionmaster.TaxDescriptionName,
                        taxpercentage = descriptionmaster.TaxPercentage,
                        istaxdescriptionactive = descriptionmaster.IsTaxDescriptionActive,
                        taxcode = descriptionmaster.TaxCode,
                        fromamount = descriptionmaster.FromAmount,
                        toamount = descriptionmaster.ToAmount,
                        fromdate = descriptionmaster.FromDate,
                        todate = descriptionmaster.ToDate,
                        ipaddress = descriptionmaster.IPAddress,
                        propertycode = descriptionmaster.PropertyCode,
                        branchcode = descriptionmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = descriptionmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteTaxDescriptionMasterDetail(int TaxDescriptionId, string TaxDescriptionCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_TaxDescription_Master
                WHERE TaxDescriptionId = @taxdescriptionid AND TaxDescriptionCode = @taxdescriptioncode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { taxdescriptionid = TaxDescriptionId, taxdescriptioncode = TaxDescriptionCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region CompanyMaster

        public async Task<IEnumerable<CompanyMasterModel>> GetCompanyMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select  CompanyId, CompanyCode, CompanyName, CompanyType, CompanyContactPerson, CompanyMobileNo, 
              CompanyEmail, CompanyAddress, CompanyCity, CompanyState, CompanyNation, CompanyPincode, IsCompanyActive, CompanyGSTNo, 
              CompanyGSTType, CompanyGSTStateCode, CompanyOpeningBalance, UserCode, IPAddress, PropertyCode, BranchCode
              From Tbl_Company_Master
              Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By CompanyId; ";

            var Companylist = await connection.QueryAsync<CompanyMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Companylist;
        }

        public async Task<int> CreateCompanyMasterDetail(CompanyMasterModel companymaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Company_Master
                (
                   CompanyCode, CompanyName, CompanyType, CompanyContactPerson, CompanyMobileNo, CompanyEmail, CompanyAddress, CompanyCity, 
                   CompanyState, CompanyNation, CompanyPincode, IsCompanyActive, CompanyGSTNo, CompanyGSTType, CompanyGSTStateCode,
                   CompanyOpeningBalance, UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                   @companycode, @companyname, @companytype, @companycontactperson, @companymobileno, @companyemail, @companyaddress, @companycity, 
                   @companystate, @companynation, @companypincode, @iscompanyactive, @companygstno, @companygsttype, @companygststatecode,
                   @companyopeningbalance, @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS CompanyId;";
                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    companycode = companymaster.CompanyCode,
                    companyname = companymaster.CompanyName,
                    companytype = companymaster.CompanyType,
                    companycontactperson = companymaster.CompanyContactPerson,
                    companymobileno = companymaster.CompanyMobileNo,
                    companyemail = companymaster.CompanyEmail,
                    companyaddress = companymaster.CompanyAddress,
                    companycity = companymaster.CompanyCity,
                    companystate = companymaster.CompanyState,
                    companynation = companymaster.CompanyNation,
                    companypincode = companymaster.CompanyPincode,
                    iscompanyactive = companymaster.IsCompanyActive,
                    companygstno = companymaster.CompanyGSTNo,
                    companygsttype = companymaster.CompanyGSTType,
                    companygststatecode = companymaster.CompanyGSTStateCode,
                    companyopeningbalance = companymaster.CompanyOpeningBalance,
                    usercode = companymaster.UserCode,
                    ipaddress = companymaster.IPAddress,
                    propertycode = companymaster.PropertyCode,
                    branchcode = companymaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateCompanyMasterDetail(CompanyMasterModel companymaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Company_Master
                SET
                    CompanyName = COALESCE(@companyname, companyname),
                    CompanyType = COALESCE(@companytype, companytype),
                    CompanyContactPerson = COALESCE(@companycontactperson, companycontactperson),
                    CompanyMobileNo = COALESCE(@companymobileno, companymobileno),
                    CompanyEmail = COALESCE(@companyemail, companyemail),
                    CompanyAddress = COALESCE(@companyaddress, companyaddress),
                    CompanyCity = COALESCE(@companycity, companycity),
                    CompanyState = COALESCE(@companystate, companystate),
                    CompanyNation = COALESCE(@companynation, companynation),
                    CompanyPincode = COALESCE(@companypincode, companypincode),
                    IsCompanyActive = COALESCE(@iscompanyactive, iscompanyactive),
                    CompanyGSTNo = COALESCE(@companygstno, companygstno),
                    CompanyGSTType = COALESCE(@companygsttype, companygsttype),
                    CompanyGSTStateCode = COALESCE(@companygststatecode, companygststatecode),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE CompanyId = @companyid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        companyid = companymaster.CompanyId,
                        companycode = companymaster.CompanyCode,
                        companyname = companymaster.CompanyName,
                        companytype = companymaster.CompanyType,
                        companycontactperson = companymaster.CompanyContactPerson,
                        companymobileno = companymaster.CompanyMobileNo,
                        companyemail = companymaster.CompanyEmail,
                        companyaddress = companymaster.CompanyAddress,
                        companycity = companymaster.CompanyCity,
                        companystate = companymaster.CompanyState,
                        companynation = companymaster.CompanyNation,
                        companypincode = companymaster.CompanyPincode,
                        iscompanyactive = companymaster.IsCompanyActive,
                        companygstno = companymaster.CompanyGSTNo,
                        companygsttype = companymaster.CompanyGSTType,
                        companygststatecode = companymaster.CompanyGSTStateCode,
                        companyopeningbalance = companymaster.CompanyOpeningBalance,
                        ipaddress = companymaster.IPAddress,
                        propertycode = companymaster.PropertyCode,
                        branchcode = companymaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = companymaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteCompanyMasterDetail(int CompanyId, string CompanyCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Company_Master
                WHERE CompanyId = @companyid AND CompanyCode = @companycode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { companyid = CompanyId, companycode = CompanyCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region CompanyTraiffMaster

        public async Task<IEnumerable<CompanyTraiffMasterModel>> GetCompanyTraiffMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select CompanyTraiffId, CompanyCode, RoomCode, TraiffCategory, RoomPlanType, PlanId, PlanName, DefaultTraiff,
                PAX, PAXRates, ExtraAdult, ExtraChild, TaxInclude, ServiceTax, Package, UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Company_Traiff
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By CompanyTraiffId; ";

            var CompanyTraifflist = await connection.QueryAsync<CompanyTraiffMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return CompanyTraifflist;
        }

        public async Task<int> CreateCompanyTraiffMasterDetail(CompanyTraiffRequestModel cmptraiffmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;
            connection.Open();

            var checkQuery = "SELECT COUNT(1) FROM Tbl_Company_Traiff WHERE CompanyCode = @companycode AND RoomCode = @roomcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
            var roomExists = await connection.QuerySingleAsync<int>(checkQuery,
                new { companycode = cmptraiffmaster.CompanyCode, roomcode = cmptraiffmaster.RoomCode?.Trim(), propertycode = cmptraiffmaster.PropertyCode, branchcode = cmptraiffmaster.BranchCode });

            if (roomExists > 0)
            {
                return -1;
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Company_Traiff
                (
                    CompanyCode, RoomCode, TraiffCategory, RoomPlanType, PlanId, PlanName, DefaultTraiff,
                    PAX, PAXRates, ExtraAdult, ExtraChild, TaxInclude, ServiceTax, Package, 
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @companycode, @roomcode, @traiffcategory, @roomplantype, @planid, @planname, @defaulttraiff,
                    @pax, @paxrates, @extraadult, @extrachild, @taxinclude, @servicetax, @package, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS CompanyTraiffId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    companycode = cmptraiffmaster.CompanyCode,
                    roomcode = cmptraiffmaster.RoomCode,
                    traiffcategory = cmptraiffmaster.TraiffCategory,
                    roomplantype = cmptraiffmaster.RoomPlanType,
                    planid = cmptraiffmaster.PlanId,
                    planname = cmptraiffmaster.PlanName,
                    defaulttraiff = cmptraiffmaster.DefaultTraiff,
                    pax = cmptraiffmaster.Pax,
                    paxrates = JsonSerializer.Serialize(cmptraiffmaster.PaxRates),
                    extraadult = cmptraiffmaster.ExtraAdult,
                    extrachild = cmptraiffmaster.ExtraChild,
                    taxinclude = cmptraiffmaster.TaxInclude,
                    servicetax = cmptraiffmaster.ServiceTax,
                    package = cmptraiffmaster.Package,
                    usercode = cmptraiffmaster.UserCode,
                    ipaddress = cmptraiffmaster.IPAddress,
                    propertycode = cmptraiffmaster.PropertyCode,
                    branchcode = cmptraiffmaster.BranchCode,
                }, transaction);

                transaction.Commit();
                return newId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateCompanyTraiffMasterDetail(CompanyTraiffRequestModel cmptraiffmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Company_Traiff
                SET
                    CompanyCode = COALESCE(@companycode, CompanyCode),
                    RoomCode = COALESCE(@roomcode, RoomCode),
                    TraiffCategory = COALESCE(@traiffcategory, TraiffCategory),
                    RoomPlanType = COALESCE(@roomplantype, RoomPlanType),
                    PlanId = COALESCE(@planid, PlanId),
                    PlanName = COALESCE(@planname, PlanName),
                    DefaultTraiff = COALESCE(@defaulttraiff, DefaultTraiff),
                    PAX = COALESCE(@pax, PAX),
                    PAXRates = COALESCE(@paxrates, PAXRates),
                    ExtraAdult = COALESCE(@extraadult, ExtraAdult),
                    ExtraChild = COALESCE(@extrachild, ExtraChild),
                    TaxInclude = COALESCE(@taxinclude, TaxInclude),
                    ServiceTax = COALESCE(@servicetax, ServiceTax),
                    Package = COALESCE(@package, Package),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE CompanyTraiffId = @companytraiffid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        companytraiffid = cmptraiffmaster.CompanyTraiffId,
                        companycode = cmptraiffmaster.CompanyCode,
                        roomcode = cmptraiffmaster.RoomCode,
                        traiffcategory = cmptraiffmaster.TraiffCategory,
                        roomplantype = cmptraiffmaster.RoomPlanType,
                        planid = cmptraiffmaster.PlanId,
                        planname = cmptraiffmaster.PlanName,
                        defaulttraiff = cmptraiffmaster.DefaultTraiff,
                        pax = cmptraiffmaster.Pax,
                        paxrates = JsonSerializer.Serialize(cmptraiffmaster.PaxRates),
                        extraadult = cmptraiffmaster.ExtraAdult,
                        extrachild = cmptraiffmaster.ExtraChild,
                        taxinclude = cmptraiffmaster.TaxInclude,
                        servicetax = cmptraiffmaster.ServiceTax,
                        package = cmptraiffmaster.Package,
                        ipaddress = cmptraiffmaster.IPAddress,
                        propertycode = cmptraiffmaster.PropertyCode,
                        branchcode = cmptraiffmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = cmptraiffmaster.UserCode
                    }, transaction);

                if (rowsAffected == 0)
                {
                    return 0;
                }

                transaction.Commit();
                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> DeleteCompanyTraiffMasterDetail(int CompanyTraiffId, string RoomCode, string CompanyCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            connection.Open();

            try
            {
                using var transaction = connection.BeginTransaction();

                const string deletequery = @"DELETE FROM Tbl_Company_Traiff 
                WHERE CompanyTraiffId = @companytraiffid AND RoomCode = @roomcode AND CompanyCode = @companycode AND PropertyCode = @propertycode AND BranchCode = @branchcode";

                var rowsAffected = await connection.ExecuteAsync(deletequery, new { companytraiffid = CompanyTraiffId, roomcode = RoomCode, companycode = CompanyCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                transaction.Commit();
                return rowsAffected;

            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region RevenueMaster

        public async Task<IEnumerable<RevenueMasterModel>> GetRevenueMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery =@"Select RevenueId, RevenueCode, RevenueName, IsRevenueActive, TaxCode, SACCode, IsHouseKeeping, 
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Revenue_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By RevenueId; ";

            var Revenuelist = await connection.QueryAsync<RevenueMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Revenuelist;
        }

        public async Task<int> CreateRevenueMasterDetail(RevenueMasterModel revenuemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Revenue_Master
                (
                    RevenueCode, RevenueName, IsRevenueActive, TaxCode, SACCode, IsHouseKeeping,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @revenuecode, @revenuename, @isrevenueactive, @taxcode, @saccode, @ishousekeeping, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS RevenueId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    revenuecode = revenuemaster.RevenueCode,
                    revenuename = revenuemaster.RevenueName,
                    isrevenueactive = revenuemaster.IsRevenueActive,
                    taxcode = revenuemaster.TaxCode,
                    saccode = revenuemaster.SACCode,
                    ishousekeeping = revenuemaster.IsHouseKeeping,
                    usercode = revenuemaster.UserCode,
                    ipaddress = revenuemaster.IPAddress,
                    propertycode = revenuemaster.PropertyCode,
                    branchcode = revenuemaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateRevenueMasterDetail(RevenueMasterModel revenuemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Revenue_Master
                SET
                    RevenueName = COALESCE(@revenuename, revenuename),
                    IsRevenueActive = COALESCE(@isRevenueactive, isRevenueactive),
                    TaxCode = COALESCE(@taxcode, taxcode),
                    SACCode = COALESCE(@saccode, saccode),
                    IsHouseKeeping = COALESCE(@ishousekeeping, ishousekeeping),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE revenueid = @revenueid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        revenueid = revenuemaster.RevenueId,
                        revenuecode = revenuemaster.RevenueCode,
                        revenuename = revenuemaster.RevenueName,
                        isrevenueactive = revenuemaster.IsRevenueActive,
                        taxcode = revenuemaster.TaxCode,
                        saccode = revenuemaster.SACCode,
                        ishousekeeping = revenuemaster.IsHouseKeeping,
                        ipaddress = revenuemaster.IPAddress,
                        propertycode = revenuemaster.PropertyCode,
                        branchcode = revenuemaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = revenuemaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteRevenueMasterDetail(int RevenueId, string RevenueCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Revenue_Master
                WHERE RevenueId = @revenueid AND RevenueCode = @revenuecode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { revenueid = RevenueId, revenuecode = RevenueCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region StewardMaster

        public async Task<IEnumerable<StewardMasterModel>> GetStewardMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select StewardId, StewardCode, StewardName, StewardDOB, StewardAddress, StewardMobileNo, StewardEmail,
                StewardCity, StewardState, StewardNation, StewardPincode, IsStewardActive, 
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Steward_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By StewardId; ";

            var Stewardlist = await connection.QueryAsync<StewardMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Stewardlist;
        }

        public async Task<int> CreateStewardMasterDetail(StewardMasterModel stewardmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Steward_Master
                (
                    StewardCode, StewardName, StewardDOB, StewardAddress, StewardMobileNo, StewardEmail,
                    StewardCity, StewardState, StewardNation, StewardPincode, IsStewardActive, 
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @stewardcode, @stewardname, @stewarddob, @stewardaddress, @stewardmobileno, @stewardemail,
                    @stewardcity, @stewardstate, @stewardnation, @stewardpincode, @isstewardactive, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS RevenueId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    stewardcode = stewardmaster.StewardCode,
                    stewardname = stewardmaster.StewardName,
                    stewarddob = stewardmaster.StewardDOB,
                    stewardaddress = stewardmaster.StewardAddress,
                    stewardmobileno = stewardmaster.StewardMobileNo,
                    stewardemail = stewardmaster.StewardEmail,
                    stewardcity = stewardmaster.StewardCity,
                    stewardstate = stewardmaster.StewardState,
                    stewardnation = stewardmaster.StewardNation,
                    stewardpincode = stewardmaster.StewardPincode,
                    isstewardactive = stewardmaster.IsStewardActive,
                    usercode = stewardmaster.UserCode,
                    ipaddress = stewardmaster.IPAddress,
                    propertycode = stewardmaster.PropertyCode,
                    branchcode = stewardmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateStewardMasterDetail(StewardMasterModel stewardmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Steward_Master
                SET
                    StewardName = COALESCE(@stewardname, stewardname),
                    StewardDOB = COALESCE(@stewarddob, stewarddob),
                    StewardAddress = COALESCE(@stewardaddress, stewardaddress),
                    StewardMobileNo = COALESCE(@stewardmobileno, stewardmobileno),
                    StewardEmail = COALESCE(@stewardemail, stewardemail),
                    StewardCity = COALESCE(@stewardcity, stewardcity),
                    StewardState = COALESCE(@stewardstate, stewardstate),
                    StewardNation = COALESCE(@stewardnation, stewardnation),
                    StewardPincode = COALESCE(@stewardpincode, stewardpincode),
                    IsStewardActive = COALESCE(@isstewardactive, isstewardactive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE StewardId = @stewardid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        stewardid = stewardmaster.StewardId,
                        stewardcode = stewardmaster.StewardCode,
                        stewardname = stewardmaster.StewardName,
                        stewarddob = stewardmaster.StewardDOB,
                        stewardaddress = stewardmaster.StewardAddress,
                        stewardmobileno = stewardmaster.StewardMobileNo,
                        stewardemail = stewardmaster.StewardEmail,
                        stewardcity = stewardmaster.StewardCity,
                        stewardstate = stewardmaster.StewardState,
                        stewardnation = stewardmaster.StewardNation,
                        stewardpincode = stewardmaster.StewardPincode,
                        isstewardactive = stewardmaster.IsStewardActive,
                        ipaddress = stewardmaster.IPAddress,
                        propertycode = stewardmaster.PropertyCode,
                        branchcode = stewardmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = stewardmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteStewardMasterDetail(int StewardId, string StewardCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Steward_Master
                WHERE StewardId = @stewardid AND StewardCode = @stewardcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { stewardid = StewardId, stewardcode = StewardCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region ItemMaster

        public async Task<IEnumerable<ItemMasterModel>> GetItemMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select ItemId, ItemCode, ItemName, ItemRate, RevenueCode, RevenueName, IsItemActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Item_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By ItemId; ";

            var Itemlist = await connection.QueryAsync<ItemMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Itemlist;
        }

        public async Task<int> CreateItemMasterDetail(ItemMasterModel itemmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Item_Master
                (
                    ItemCode, ItemName, ItemRate, RevenueCode, RevenueName, IsItemActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @itemcode, @itemname, @itemrate, @revenuecode, @revenuename, @isitemactive, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS ItemId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    itemcode = itemmaster.ItemCode,
                    itemname = itemmaster.ItemName,
                    itemrate = itemmaster.ItemRate,
                    revenuecode = itemmaster.RevenueCode,
                    revenuename = itemmaster.RevenueName,
                    isitemactive = itemmaster.IsItemActive,
                    usercode = itemmaster.UserCode,
                    ipaddress = itemmaster.IPAddress,
                    propertycode = itemmaster.PropertyCode,
                    branchcode = itemmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateItemMasterDetail(ItemMasterModel itemmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Item_Master
                SET
                    ItemName = COALESCE(@itemname, itemname),
                    ItemRate = COALESCE(@itemrate, itemrate),
                    RevenueCode = COALESCE(@revenuecode, revenuecode),
                    RevenueName = COALESCE(@revenuename, revenuename),
                    IsItemActive = COALESCE(@isitemactive, isitemactive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE ItemId = @itemid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        itemid = itemmaster.ItemId,
                        itemcode = itemmaster.ItemCode,
                        itemname = itemmaster.ItemName,
                        itemrate = itemmaster.ItemRate,
                        revenuecode = itemmaster.RevenueCode,
                        revenuename = itemmaster.RevenueName,
                        isitemactive = itemmaster.IsItemActive,
                        ipaddress = itemmaster.IPAddress,
                        propertycode = itemmaster.PropertyCode,
                        branchcode = itemmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = itemmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteItemMasterDetail(int ItemId, string ItemCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Item_Master
                WHERE ItemId = @itemid AND ItemCode = @itemcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { itemid = ItemId, itemcode = ItemCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region SegmentMaster

        public async Task<IEnumerable<SegmentMasterModel>> GetSegmentMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select SegmentId, SegmentCode, SegmentName, SegmentDescription, IsSegmentActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Segment_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By SegmentId; ";

            var Segmentlist = await connection.QueryAsync<SegmentMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Segmentlist;
        }

        public async Task<int> CreateSegmentMasterDetail(SegmentMasterModel segmentmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Segment_Master
                (
                    SegmentCode, SegmentName, SegmentDescription, IsSegmentActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @segmentcode, @segmentname, @segmentdescription, @issegmentactive, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS SegmentId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    segmentcode = segmentmaster.SegmentCode,
                    segmentname = segmentmaster.SegmentName,
                    segmentdescription = segmentmaster.SegmentDescription,
                    issegmentactive = segmentmaster.IsSegmentActive,
                    usercode = segmentmaster.UserCode,
                    ipaddress = segmentmaster.IPAddress,
                    propertycode = segmentmaster.PropertyCode,
                    branchcode = segmentmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateSegmentMasterDetail(SegmentMasterModel segmentmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Segment_Master
                SET
                    SegmentName = COALESCE(@segmentname, segmentname),
                    SegmentDescription = COALESCE(@segmentdescription, segmentdescription),
                    IsSegmentActive = COALESCE(@issegmentactive, issegmentactive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE SegmentId = @segmentid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        segmentid = segmentmaster.SegmentId,
                        segmentcode = segmentmaster.SegmentCode,
                        segmentname = segmentmaster.SegmentName,
                        segmentdescription = segmentmaster.SegmentDescription,
                        issegmentactive = segmentmaster.IsSegmentActive,
                        ipaddress = segmentmaster.IPAddress,
                        propertycode = segmentmaster.PropertyCode,
                        branchcode = segmentmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = segmentmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteSegmentMasterDetail(int SegmentId, string SegmentCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Segment_Master
                WHERE SegmentId = @segmentid AND SegmentCode = @segmentcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { segmentid = SegmentId, segmentcode = SegmentCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region SubSegmentMaster

        public async Task<IEnumerable<SubSegmentMasterModel>> GetSubSegmentMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select SubSegmentId, SubSegmentCode, SubSegmentName, SegmentCode, IsSubSegmentActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_SubSegment_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By SubSegmentId; ";

            var SubSegmentlist = await connection.QueryAsync<SubSegmentMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return SubSegmentlist;
        }

        public async Task<int> CreateSubSegmentMasterDetail(SubSegmentMasterModel subsegmentmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_SubSegment_Master
                (
                    SubSegmentCode, SubSegmentName, SegmentCode, IsSubSegmentActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @subsegmentcode, @subsegmentname, @segmentcode, @issubsegmentactive, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS SubSegmentId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    subsegmentcode = subsegmentmaster.SubSegmentCode,
                    subsegmentname = subsegmentmaster.SubSegmentName,
                    segmentcode = subsegmentmaster.SegmentCode,
                    issubsegmentactive = subsegmentmaster.IsSubSegmentActive,
                    usercode = subsegmentmaster.UserCode,
                    ipaddress = subsegmentmaster.IPAddress,
                    propertycode = subsegmentmaster.PropertyCode,
                    branchcode = subsegmentmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateSubSegmentMasterDetail(SubSegmentMasterModel subsegmentmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_SubSegment_Master
                SET
                    SubSegmentName = COALESCE(@SubSegmentName, SubSegmentName),
                    SegmentCode = COALESCE(@SegmentCode, SegmentCode),
                    IsSubSegmentActive = COALESCE(@IsSubSegmentActive, IsSubSegmentActive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE SubSegmentId = @subsegmentid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        subsegmentid = subsegmentmaster.SubSegmentId,
                        subsegmentcode = subsegmentmaster.SubSegmentCode,
                        subsegmentname = subsegmentmaster.SubSegmentName,
                        segmentcode = subsegmentmaster.SegmentCode,
                        issubsegmentactive = subsegmentmaster.IsSubSegmentActive,
                        ipaddress = subsegmentmaster.IPAddress,
                        propertycode = subsegmentmaster.PropertyCode,
                        branchcode = subsegmentmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = subsegmentmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteSubSegmentMasterDetail(int SubSegmentId, string SubSegmentCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_SubSegment_Master
                WHERE SubSegmentId = @subsegmentid AND SubSegmentCode = @subsegmentcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { subsegmentid = SubSegmentId, subsegmentcode = SubSegmentCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region GuestMaster

        public async Task<IEnumerable<GuestMasterModel>> GetGuestMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select GuestId, GuestCode, GuestTitle, GuestName, GuestGender, GuestDOB, GuestDOA, GuestAddress, GuestCity, GuestState, GuestNation, 
                GuestPinCode, GuestMobileNo, GuestEmail, IdentityproofCode, IdentityproofName, GuestVehicleType, GuestVehicleNo, IsGuestActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Guest_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By GuestId; ";

            var Guestlist = await connection.QueryAsync<GuestMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Guestlist;
        }

        public async Task<int> CreateGuestMasterDetail(GuestMasterModel guestmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Guest_Master
                (
                    GuestCode, GuestTitle, GuestName, GuestGender, GuestDOB, GuestDOA, GuestAddress, GuestCity, GuestState, GuestNation, 
                    GuestPinCode, GuestMobileNo, GuestEmail, IdentityproofCode, IdentityproofName, GuestVehicleType, GuestVehicleNo, IsGuestActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @guestcode, @guesttitle, @guestname, @guestgender, @guestdob, @guestdoa, @guestaddress, @guestcity, @gueststate, @guestnation, 
                    @guestpincode, @guestmobileno, @guestemail, @identityproofcode, @identityproofname, @guestvehicletype, @guestvehicleno, @isguestactive,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS GuestId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    guestcode = guestmaster.GuestCode,
                    guesttitle = guestmaster.GuestTitle,
                    guestname = guestmaster.GuestName,
                    guestgender = guestmaster.GuestGender,
                    guestdob = guestmaster.GuestDOB,
                    guestdoa = guestmaster.GuestDOA,
                    guestaddress = guestmaster.GuestAddress,
                    guestcity = guestmaster.GuestCity,
                    gueststate = guestmaster.GuestState,
                    guestnation = guestmaster.GuestNation,
                    guestpincode = guestmaster.GuestPinCode,
                    guestmobileno = guestmaster.GuestMobileNo,
                    guestemail = guestmaster.GuestEmail,
                    identityproofcode = guestmaster.IdentityproofCode,
                    identityproofname = guestmaster.IdentityproofName,
                    guestvehicletype = guestmaster.GuestVehicleType,
                    guestvehicleno = guestmaster.GuestVehicleNo,
                    isguestactive = guestmaster.IsGuestActive,
                    usercode = guestmaster.UserCode,
                    ipaddress = guestmaster.IPAddress,
                    propertycode = guestmaster.PropertyCode,
                    branchcode = guestmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateGuestMasterDetail(GuestMasterModel guestmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Guest_Master
                SET
                    GuestTitle = COALESCE(@guesttitle, GuestTitle),
                    GuestName = COALESCE(@guestname, GuestName),
                    GuestGender = COALESCE(@guestgender, GuestGender),
                    GuestDOB = COALESCE(@guestdob, GuestDOB),
                    GuestDOA = COALESCE(@guestdoa, GuestDOA),
                    GuestAddress = COALESCE(@guestaddress, GuestAddress),
                    GuestCity = COALESCE(@guestcity, GuestCity),
                    GuestState = COALESCE(@gueststate, GuestState),
                    GuestNation = COALESCE(@guestnation, GuestNation),
                    GuestPinCode = COALESCE(@guestpincode, GuestPinCode),
                    GuestMobileNo = COALESCE(@guestmobileno, GuestMobileNo),
                    GuestEmail = COALESCE(@guestemail, GuestEmail),
                    IdentityproofCode = COALESCE(@identityproofcode, IdentityproofCode),
                    IdentityproofName = COALESCE(@identityproofname, IdentityproofName),
                    GuestVehicleType = COALESCE(@guestvehicletype, GuestVehicleType),
                    GuestVehicleNo = COALESCE(@guestvehicleno, GuestVehicleNo),
                    IsGuestActive = COALESCE(@isguestactive, IsGuestActive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE GuestId = @guestid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        guestid = guestmaster.GuestId,
                        guestcode = guestmaster.GuestCode,
                        guesttitle = guestmaster.GuestTitle,
                        guestname = guestmaster.GuestName,
                        guestgender = guestmaster.GuestGender,
                        guestdob = guestmaster.GuestDOB,
                        guestdoa = guestmaster.GuestDOA,
                        guestaddress = guestmaster.GuestAddress,
                        guestcity = guestmaster.GuestCity,
                        gueststate = guestmaster.GuestState,
                        guestnation = guestmaster.GuestNation,
                        guestpincode = guestmaster.GuestPinCode,
                        guestmobileno = guestmaster.GuestMobileNo,
                        guestemail = guestmaster.GuestEmail,
                        identityproofcode = guestmaster.IdentityproofCode,
                        identityproofname = guestmaster.IdentityproofName,
                        guestvehicletype = guestmaster.GuestVehicleType,
                        guestvehicleno = guestmaster.GuestVehicleNo,
                        isguestactive = guestmaster.IsGuestActive,
                        ipaddress = guestmaster.IPAddress,
                        propertycode = guestmaster.PropertyCode,
                        branchcode = guestmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = guestmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteGuestMasterDetail(int GuestId, string GuestCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Guest_Master
                WHERE GuestId = @guestid AND GuestCode = @guestcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { guestid = GuestId, guestcode = GuestCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region FormCMaster

        public async Task<IEnumerable<FormCMasterModel>> GetFormCMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select FormCId, FormCCode, GuestCode, CompanyCode, CheckinNo, 
                IndiaAddress, IndiaCity, IndiaState, IndiaPinCode, IndiaCountry,
                PermanentAddress, PermanentCity, PermanentState, PermanentPinCode, PermanentCountry,
                PassportNo, PassportIssueCity, PassportIssueCountry, PassportIssueDate, PassportExpiryDate,
                VisaNo, VisaIssueCity, VisaIssueCountry, VisaIssueDate, VisaExpiryDate, VisaType,
                ArrivalCity, ArrivalCountry, ArrivalDate, HotelArrivalDate, HotelArrivalTime, NoofStay, VisitPurpose,
                DepartureDate, NextDestinationPlace, NextDestinationCity, NextDestinationState, NextDestinationCountry,
                MobileNo, Email,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_FormC_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By FormCId; ";

            var formlist = await connection.QueryAsync<FormCMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return formlist;
        }

        public async Task<int> CreateFormCMasterDetail(FormCRequestModel formcmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_FormC_Master
                (
                    FormCCode, GuestCode, CompanyCode, CheckinNo, 
                    IndiaAddress, IndiaCity, IndiaState, IndiaPinCode, IndiaCountry,
                    PermanentAddress, PermanentCity, PermanentState, PermanentPinCode, PermanentCountry,
                    PassportNo, PassportIssueCity, PassportIssueCountry, PassportIssueDate, PassportExpiryDate,
                    VisaNo, VisaIssueCity, VisaIssueCountry, VisaIssueDate, VisaExpiryDate, VisaType,
                    ArrivalCity, ArrivalCountry, ArrivalDate, HotelArrivalDate, HotelArrivalTime, NoofStay, VisitPurpose,
                    DepartureDate, NextDestinationPlace, NextDestinationCity, NextDestinationState, NextDestinationCountry,
                    MobileNo, Email,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @formccode, @guestcode, @companycode, @checkinno, 
                    @indiaaddress, @indiacity, @indiastate, @indiapincode, @indiacountry,
                    @permanentaddress, @permanentcity, @permanentstate, @permanentpincode, @permanentcountry,
                    @passportno, @passportissuecity, @passportissuecountry, @passportissuedate, @passportexpirydate,
                    @visano, @visaissuecity, @visaissuecountry, @visaissuedate, @visaexpirydate, @visatype,
                    @arrivalcity, @arrivalcountry, @arrivaldate, @hotelarrivaldate, @hotelarrivaltime, @noofstay, @visitpurpose,
                    @departuredate, @nextdestinationplace, @nextdestinationcity, @nextdestinationstate, @nextdestinationcountry,
                    @mobileno, @email,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS FormCId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    formccode = formcmaster.FormCCode,
                    guestcode = formcmaster.GuestCode,
                    companycode = formcmaster.CompanyCode,
                    checkinno = formcmaster.CheckinNo,

                    indiaaddress = formcmaster.IndiaDetails.IndiaAddress,
                    indiacity = formcmaster.IndiaDetails.IndiaCity,
                    indiastate = formcmaster.IndiaDetails.IndiaState,
                    indiapincode = formcmaster.IndiaDetails.IndiaPinCode,
                    indiacountry = formcmaster.IndiaDetails.IndiaCountry,

                    permanentaddress = formcmaster.PermanentDetails.PermanentAddress,
                    permanentcity = formcmaster.PermanentDetails.PermanentCity,
                    permanentstate = formcmaster.PermanentDetails.PermanentState,
                    permanentpincode = formcmaster.PermanentDetails.PermanentPinCode,
                    permanentcountry = formcmaster.PermanentDetails.PermanentCountry,

                    passportno = formcmaster.PassportDetails.PassportNo,
                    passportissuecity = formcmaster.PassportDetails.PassportIssueCity,
                    passportissuecountry = formcmaster.PassportDetails.PassportIssueCountry,
                    passportissuedate = formcmaster.PassportDetails.PassportIssueDate,
                    passportexpirydate = formcmaster.PassportDetails.PassportExpiryDate,

                    visano = formcmaster.VisaDetails.VisaNo,
                    visaissuecity = formcmaster.VisaDetails.VisaIssueCity,
                    visaissuecountry = formcmaster.VisaDetails.VisaIssueCountry,
                    visaissuedate = formcmaster.VisaDetails.VisaIssueDate,
                    visaexpirydate = formcmaster.VisaDetails.VisaExpiryDate,
                    visatype = formcmaster.VisaDetails.VisaType,

                    arrivalcity = formcmaster.ArrivalDetails.ArrivalCity,
                    arrivalcountry = formcmaster.ArrivalDetails.ArrivalCountry,
                    arrivaldate = formcmaster.ArrivalDetails.ArrivalDate,
                    hotelarrivaldate = formcmaster.ArrivalDetails.HotelArrivalDate,
                    hotelarrivaltime = formcmaster.ArrivalDetails.HotelArrivalTime,
                    noofstay = formcmaster.ArrivalDetails.NoofStay,
                    visitpurpose = formcmaster.ArrivalDetails.VisitPurpose,
                    departuredate = formcmaster.ArrivalDetails.DepartureDate,

                    nextdestinationplace = formcmaster.NextDestinationDetails.NextDestinationPlace,
                    nextdestinationcity = formcmaster.NextDestinationDetails.NextDestinationCity,
                    nextdestinationstate = formcmaster.NextDestinationDetails.NextDestinationState,
                    nextdestinationcountry = formcmaster.NextDestinationDetails.NextDestinationCountry,

                    mobileno = formcmaster.ContactDetails.MobileNo,
                    email = formcmaster.ContactDetails.Email,

                    usercode = formcmaster.UserCode,
                    ipaddress = formcmaster.IPAddress,
                    propertycode = formcmaster.PropertyCode,
                    branchcode = formcmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateFormCMasterDetail(FormCRequestModel formcmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_FormC_Master
                SET
                    CompanyCode = COALESCE(@companycode, CompanyCode),
                    CheckinNo = COALESCE(@checkinno, CheckinNo),

                    IndiaAddress = COALESCE(@indiaaddress, IndiaAddress),
                    IndiaCity = COALESCE(@indiacity, IndiaCity),
                    IndiaState = COALESCE(@indiastate, IndiaState),
                    IndiaPinCode = COALESCE(@indiapincode, IndiaPinCode),
                    Indiacountry = COALESCE(@indiacountry, IndiaCountry),

                    PermanentAddress = COALESCE(@permanentaddress, PermanentAddress),
                    PermanentCity = COALESCE(@permanentcity, PermanentCity),
                    PermanentState = COALESCE(@permanentstate, PermanentState),
                    PermanentPinCode = COALESCE(@permanentpincode, PermanentPinCode),
                    Permanentcountry = COALESCE(@Permanentcountry, PermanentCountry),

                    PassportNo = COALESCE(@passportno, PassportNo),
                    PassportIssueCity = COALESCE(@passportissuecity, PassportIssueCity),
                    PassportIssueCountry = COALESCE(@passportissuecountry, PassportIssueCountry),
                    PassportIssueDate = COALESCE(@passportissuedate, PassportIssueDate),
                    PassportExpiryDate = COALESCE(@passportexpirydate, PassportExpiryDate),

                    VisaNo = COALESCE(@visano, VisaNo),
                    VisaIssueCity = COALESCE(@visaissuecity, VisaIssueCity),
                    VisaIssueCountry = COALESCE(@visaissuecountry, VisaIssueCountry),
                    VisaIssueDate = COALESCE(@visaissuedate, VisaIssueDate),
                    VisaExpiryDate = COALESCE(@visaexpirydate, VisaExpiryDate),
                    VisaType = COALESCE(@visatype, VisaType),

                    ArrivalCity = COALESCE(@arrivalcity, ArrivalCity),
                    ArrivalCountry = COALESCE(@arrivalcountry, ArrivalCountry),
                    ArrivalDate = COALESCE(@arrivaldate, ArrivalDate),
                    HotelArrivalDate = COALESCE(@hotelarrivaldate, HotelArrivalDate),
                    HotelArrivalTime = COALESCE(@hotelarrivaltime, HotelArrivalTime),
                    NoofStay = COALESCE(@noofstay, NoofStay),
                    VisitPurpose = COALESCE(@visitpurpose, VisitPurpose),
                    DepartureDate = COALESCE(@departuredate, DepartureDate),

                    NextDestinationPlace = COALESCE(@nextdestinationplace, NextDestinationPlace),
                    NextDestinationCity = COALESCE(@nextdestinationcity, NextDestinationCity),
                    NextDestinationState = COALESCE(@nextdestinationstate, NextDestinationState),
                    NextDestinationcountry = COALESCE(@nextdestinationcountry, NextDestinationCountry),

                    MobileNo = COALESCE(@mobileno, MobileNo),
                    Email = COALESCE(@email, Email),

                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE FormCId = @formcid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        formcid = formcmaster.FormCId,
                        formccode = formcmaster.FormCCode,
                        guestcode = formcmaster.GuestCode,
                        companycode = formcmaster.CompanyCode,
                        checkinno = formcmaster.CheckinNo,

                        indiaaddress = formcmaster.IndiaDetails.IndiaAddress,
                        indiacity = formcmaster.IndiaDetails.IndiaCity,
                        indiastate = formcmaster.IndiaDetails.IndiaState,
                        indiapincode = formcmaster.IndiaDetails.IndiaPinCode,
                        indiacountry = formcmaster.IndiaDetails.IndiaCountry,

                        permanentaddress = formcmaster.PermanentDetails.PermanentAddress,
                        permanentcity = formcmaster.PermanentDetails.PermanentCity,
                        permanentstate = formcmaster.PermanentDetails.PermanentState,
                        permanentpincode = formcmaster.PermanentDetails.PermanentPinCode,
                        permanentcountry = formcmaster.PermanentDetails.PermanentCountry,

                        passportno = formcmaster.PassportDetails.PassportNo,
                        passportissuecity = formcmaster.PassportDetails.PassportIssueCity,
                        passportissuecountry = formcmaster.PassportDetails.PassportIssueCountry,
                        passportissuedate = formcmaster.PassportDetails.PassportIssueDate,
                        passportexpirydate = formcmaster.PassportDetails.PassportExpiryDate,

                        visano = formcmaster.VisaDetails.VisaNo,
                        visaissuecity = formcmaster.VisaDetails.VisaIssueCity,
                        visaissuecountry = formcmaster.VisaDetails.VisaIssueCountry,
                        visaissuedate = formcmaster.VisaDetails.VisaIssueDate,
                        visaexpirydate = formcmaster.VisaDetails.VisaExpiryDate,
                        visatype = formcmaster.VisaDetails.VisaType,

                        arrivalcity = formcmaster.ArrivalDetails.ArrivalCity,
                        arrivalcountry = formcmaster.ArrivalDetails.ArrivalCountry,
                        arrivaldate = formcmaster.ArrivalDetails.ArrivalDate,
                        hotelarrivaldate = formcmaster.ArrivalDetails.HotelArrivalDate,
                        hotelarrivaltime = formcmaster.ArrivalDetails.HotelArrivalTime,
                        noofstay = formcmaster.ArrivalDetails.NoofStay,
                        visitpurpose = formcmaster.ArrivalDetails.VisitPurpose,
                        departuredate = formcmaster.ArrivalDetails.DepartureDate,

                        nextdestinationplace = formcmaster.NextDestinationDetails.NextDestinationPlace,
                        nextdestinationcity = formcmaster.NextDestinationDetails.NextDestinationCity,
                        nextdestinationstate = formcmaster.NextDestinationDetails.NextDestinationState,
                        nextdestinationcountry = formcmaster.NextDestinationDetails.NextDestinationCountry,

                        mobileno = formcmaster.ContactDetails.MobileNo,
                        email = formcmaster.ContactDetails.Email,

                        ipaddress = formcmaster.IPAddress,
                        propertycode = formcmaster.PropertyCode,
                        branchcode = formcmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = formcmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteFormCMasterDetail(int FormCId, string GuestCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_FormC_Master
                WHERE FormCId = @formcid AND GuestCode = @guestcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { formcid = FormCId, guestcode = GuestCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region LedgerMaster

        public async Task<IEnumerable<LedgerMasterModel>> GetLedgerMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select LedgerId, LedgerCode, LedgerName, LedgerOpeningBalance, IsLedgerActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Ledger_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By LedgerId; ";

            var Ledgerlist = await connection.QueryAsync<LedgerMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Ledgerlist;
        }

        public async Task<int> CreateLedgerMasterDetail(LedgerMasterModel Ledgermaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Ledger_Master
                (
                    LedgerCode, LedgerName, LedgerOpeningBalance, IsLedgerActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @ledgercode, @ledgername, @ledgeropeningbalance, @isledgeractive, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS LedgerId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    ledgercode = Ledgermaster.LedgerCode,
                    ledgername = Ledgermaster.LedgerName,
                    ledgeropeningbalance = Ledgermaster.LedgerOpeningBalance,
                    isledgeractive = Ledgermaster.IsLedgerActive,
                    usercode = Ledgermaster.UserCode,
                    ipaddress = Ledgermaster.IPAddress,
                    propertycode = Ledgermaster.PropertyCode,
                    branchcode = Ledgermaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateLedgerMasterDetail(LedgerMasterModel Ledgermaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Ledger_Master
                SET
                    LedgerName = COALESCE(@ledgername, LedgerName),
                    LedgerOpeningBalance = COALESCE(@ledgeropeningbalance, LedgerOpeningBalance),
                    IsLedgerActive = COALESCE(@isledgeractive, IsLedgerActive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE LedgerId = @ledgerid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        ledgerid = Ledgermaster.LedgerId,
                        ledgercode = Ledgermaster.LedgerCode,
                        ledgername = Ledgermaster.LedgerName,
                        ledgeropeningbalance = Ledgermaster.LedgerOpeningBalance,
                        isledgeractive = Ledgermaster.IsLedgerActive,
                        ipaddress = Ledgermaster.IPAddress,
                        propertycode = Ledgermaster.PropertyCode,
                        branchcode = Ledgermaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = Ledgermaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteLedgerMasterDetail(int LedgerId, string LedgerCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Ledger_Master
                WHERE LedgerId = @ledgerid AND LedgerCode = @ledgercode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { ledgerid = LedgerId, ledgercode = LedgerCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region TaxRuleMaster

        public async Task<IEnumerable<TaxRulesMasterModel>> GetTaxRulesMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select TaxRuleId, TaxRuleCode, Traiff, TraiffOperation, Type1, Operation1, Type2, IsTaxRuleActive, IsSeprateExtraBedTax,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_TaxRule_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By TaxRuleId; ";

            var TaxRulelist = await connection.QueryAsync<TaxRulesMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return TaxRulelist;
        }

        public async Task<int> CreateTaxRulesMasterDetail(TaxRulesMasterModel taxRuleMaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First, deactivate all other tax rules
                if (taxRuleMaster.IsTaxRuleActive == true)
                {
                    var deactivateQuery = @"
                        UPDATE dbo.Tbl_TaxRule_Master SET IsTaxRuleActive = 0 WHERE PropertyCode = @propertycode AND BranchCode = @branchcode AND TaxRuleCode <> @taxrulecode;";

                    await connection.ExecuteAsync(deactivateQuery,
                       new
                       {
                           propertycode = taxRuleMaster.PropertyCode,
                           branchcode = taxRuleMaster.BranchCode,
                           taxrulecode = taxRuleMaster.TaxRuleCode
                       },
                       transaction);
                }

                var insertquery = @"
                INSERT INTO Tbl_TaxRule_Master
                (
                    TaxRuleCode, Traiff, TraiffOperation, Type1, Operation1, Type2, IsTaxRuleActive, IsSeprateExtraBedTax,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @taxrulecode, @traiff, @traiffoperation, @type1, @operation1, @type2, @istaxruleactive, @isseprateextrabedtax,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS TaxRuleId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    taxrulecode = taxRuleMaster.TaxRuleCode,
                    traiff = taxRuleMaster.Traiff,
                    traiffoperation = taxRuleMaster.TraiffOperation,
                    type1 = taxRuleMaster.Type1,
                    operation1 = taxRuleMaster.Operation1,
                    type2 = taxRuleMaster.Type2,
                    istaxruleactive = taxRuleMaster.IsTaxRuleActive,
                    isseprateextrabedtax = taxRuleMaster.IsSeprateExtraBedTax,
                    usercode = taxRuleMaster.UserCode,
                    ipaddress = taxRuleMaster.IPAddress,
                    propertycode = taxRuleMaster.PropertyCode,
                    branchcode = taxRuleMaster.BranchCode,
                },transaction);

                transaction.Commit();
                return newId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateTaxRulesMasterDetail(TaxRulesMasterModel taxRuleMaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First, deactivate all other tax rules
                if (taxRuleMaster.IsTaxRuleActive == true)
                {
                    var deactivateQuery = @"
                        UPDATE dbo.Tbl_TaxRule_Master SET IsTaxRuleActive = 0 WHERE PropertyCode = @propertycode AND BranchCode = @branchcode AND TaxRuleId <> @taxruleid;";

                     await connection.ExecuteAsync(deactivateQuery,
                        new
                        {
                            propertycode = taxRuleMaster.PropertyCode,
                            branchcode = taxRuleMaster.BranchCode,
                            taxruleid = taxRuleMaster.TaxRuleId
                        },
                        transaction);
                }

                // Update selected tax rule
                var updateQuery = @"
                UPDATE dbo.Tbl_TaxRule_Master
                SET
                    Traiff = COALESCE(@traiff, Traiff),
                    TraiffOperation = COALESCE(@traiffoperation, TraiffOperation),
                    Type1 = COALESCE(@type1, Type1),
                    Operation1 = COALESCE(@operation1, Operation1),
                    Type2 = COALESCE(@type2, Type2),
                    IsTaxRuleActive = COALESCE(@istaxruleactive, IsTaxRuleActive),
                    IsSeprateExtraBedTax = COALESCE(@isseprateextrabedtax, IsSeprateExtraBedTax),
                    IPAddress = COALESCE(@ipaddress, IPAddress),
                    ModifiedDate = @modifieddate,
                    ModifiedBy = @modifiedby
                WHERE TaxRuleId = @taxruleid AND PropertyCode = @propertycode AND BranchCode = @branchcode;";

                var rowsAffected = await connection.ExecuteAsync(
                    updateQuery,
                    new
                    {
                        taxruleid = taxRuleMaster.TaxRuleId,
                        traiff = taxRuleMaster.Traiff,
                        traiffoperation = taxRuleMaster.TraiffOperation,
                        type1 = taxRuleMaster.Type1,
                        operation1 = taxRuleMaster.Operation1,
                        type2 = taxRuleMaster.Type2,
                        istaxruleactive = taxRuleMaster.IsTaxRuleActive,
                        isseprateextrabedtax = taxRuleMaster.IsSeprateExtraBedTax,
                        ipaddress = taxRuleMaster.IPAddress,
                        propertycode = taxRuleMaster.PropertyCode,
                        branchcode = taxRuleMaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = taxRuleMaster.UserCode
                    },
                    transaction);

                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                transaction.Commit();

                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> DeleteTaxRulesMasterDetail(int TaxRuleId, string TaxRuleCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_TaxRule_Master
                WHERE TaxRuleId = @taxruleid AND TaxRuleCode = @taxrulecode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { taxruleid = TaxRuleId, taxrulecode = TaxRuleCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region DriverMaster

        public async Task<IEnumerable<DriverMasterModel>> GetDriverMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select DriverId, DriverCode, DriverName, DriverMobileNo, DriverVechileNo, IsDriverActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Driver_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By DriverId; ";

            var Driverlist = await connection.QueryAsync<DriverMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return Driverlist;
        }

        public async Task<int> CreateDriverMasterDetail(DriverMasterModel drivermaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Driver_Master
                (
                    DriverCode, DriverName, DriverMobileNo, DriverVechileNo, IsDriverActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @drivercode, @drivername, @drivermobileno, @drivervechileno, @isdriveractive,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS DriverId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    drivercode = drivermaster.DriverCode,
                    drivername = drivermaster.DriverName,
                    drivermobileno = drivermaster.DriverMobileNo,
                    drivervechileno = drivermaster.DriverVechileNo,
                    isdriveractive = drivermaster.IsDriverActive,
                    usercode = drivermaster.UserCode,
                    ipaddress = drivermaster.IPAddress,
                    propertycode = drivermaster.PropertyCode,
                    branchcode = drivermaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateDriverMasterDetail(DriverMasterModel drivermaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Driver_Master
                SET
                    DriverName = COALESCE(@drivername, DriverName),
                    DriverMobileNo = COALESCE(@drivermobileno, DriverMobileNo),
                    DriverVechileNo = COALESCE(@drivervechileno, DriverVechileNo),
                    IsDriverActive = COALESCE(@isdriveractive, IsDriverActive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE DriverId = @driverid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        driverid = drivermaster.DriverId,
                        drivercode = drivermaster.DriverCode,
                        drivername = drivermaster.DriverName,
                        drivermobileno = drivermaster.DriverMobileNo,
                        drivervechileno = drivermaster.DriverVechileNo,
                        isdriveractive = drivermaster.IsDriverActive,
                        ipaddress = drivermaster.IPAddress,
                        propertycode = drivermaster.PropertyCode,
                        branchcode = drivermaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = drivermaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteDriverMasterDetail(int DriverId, string DriverCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_Driver_Master
                WHERE DriverId = @driverid AND DriverCode = @drivercode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { driverid = DriverId, drivercode = DriverCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region BookingSourceMaster

        public async Task<IEnumerable<BookingSourceMasterModel>> GetBookingSourceMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select BookingSourceId, BookingSourceCode, BookingSourceName, IsBookingSourceActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_BookingSource_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By BookingSourceId; ";

            var BookingSourcelist = await connection.QueryAsync<BookingSourceMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return BookingSourcelist;
        }

        public async Task<int> CreateBookingSourceMasterDetail(BookingSourceMasterModel bookingsourcemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_BookingSource_Master
                (
                    BookingSourceCode, BookingSourceName, IsBookingSourceActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @bookingsourcecode, @bookingsourcename, @isbookingsourceactive,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS BookingSourceId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    bookingsourcecode = bookingsourcemaster.BookingSourceCode,
                    bookingSourceName = bookingsourcemaster.BookingSourceName,
                    isbookingsourceactive = bookingsourcemaster.IsBookingSourceActive,
                    usercode = bookingsourcemaster.UserCode,
                    ipaddress = bookingsourcemaster.IPAddress,
                    propertycode = bookingsourcemaster.PropertyCode,
                    branchcode = bookingsourcemaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateBookingSourceMasterDetail(BookingSourceMasterModel bookingsourcemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_BookingSource_Master
                SET
                    BookingSourceName = COALESCE(@bookingsourcename, BookingSourceName),
                    IsBookingSourceActive = COALESCE(@isbookingsourceactive, IsBookingSourceActive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE BookingSourceId = @bookingsourceid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        bookingsourceid = bookingsourcemaster.BookingSourceId,
                        bookingsourcecode = bookingsourcemaster.BookingSourceCode,
                        bookingSourceName = bookingsourcemaster.BookingSourceName,
                        isbookingsourceactive = bookingsourcemaster.IsBookingSourceActive,
                        ipaddress = bookingsourcemaster.IPAddress,
                        propertycode = bookingsourcemaster.PropertyCode,
                        branchcode = bookingsourcemaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = bookingsourcemaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteBookingSourceMasterDetail(int BookingSourceId, string BookingSourceCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_BookingSource_Master
                WHERE BookingSourceId = @bookingsourceid AND BookingSourceCode = @bookingsourcecode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { BookingSourceid = BookingSourceId, bookingsourcecode = BookingSourceCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region InstructionDepartmentMaster

        public async Task<IEnumerable<InstructionDepartmentMasterModel>> GetInstructionDepartmentMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select InstructionDepartmentId, InstructionDepartmentCode, InstructionDepartmentName, IsInstructionDepartmentActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_InstructionDepartment_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By InstructionDepartmentId; ";

            var InstructionDepartmentlist = await connection.QueryAsync<InstructionDepartmentMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return InstructionDepartmentlist;
        }

        public async Task<int> CreateInstructionDepartmentMasterDetail(InstructionDepartmentMasterModel instructiondeptmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_InstructionDepartment_Master
                (
                    InstructionDepartmentCode, InstructionDepartmentName, IsInstructionDepartmentActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @instructiondepartmentcode, @instructiondepartmentname, @isinstructiondepartmentactive,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS InstructionDepartmentId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    instructiondepartmentcode = instructiondeptmaster.InstructionDepartmentCode,
                    instructiondepartmentname = instructiondeptmaster.InstructionDepartmentName,
                    isinstructiondepartmentactive = instructiondeptmaster.IsInstructionDepartmentActive,
                    usercode = instructiondeptmaster.UserCode,
                    ipaddress = instructiondeptmaster.IPAddress,
                    propertycode = instructiondeptmaster.PropertyCode,
                    branchcode = instructiondeptmaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateInstructionDepartmentMasterDetail(InstructionDepartmentMasterModel instructiondeptmaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_InstructionDepartment_Master
                SET
                    InstructionDepartmentName = COALESCE(@instructiondepartmentname, InstructionDepartmentName),
                    IsInstructionDepartmentActive = COALESCE(@isinstructiondepartmentactive, IsInstructionDepartmentActive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE InstructionDepartmentId = @instructiondepartmentid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        instructiondepartmentid = instructiondeptmaster.InstructionDepartmentId,
                        instructiondepartmentcode = instructiondeptmaster.InstructionDepartmentCode,
                        instructiondepartmentname = instructiondeptmaster.InstructionDepartmentName,
                        isinstructiondepartmentactive = instructiondeptmaster.IsInstructionDepartmentActive,
                        ipaddress = instructiondeptmaster.IPAddress,
                        propertycode = instructiondeptmaster.PropertyCode,
                        branchcode = instructiondeptmaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = instructiondeptmaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteInstructionDepartmentMasterDetail(int InstructionDepartmentId, string InstructionDepartmentCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_InstructionDepartment_Master
                WHERE InstructionDepartmentId = @instructiondepartmentid AND InstructionDepartmentCode = @instructiondepartmentcode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { instructiondepartmentid = InstructionDepartmentId, instructiondepartmentcode = InstructionDepartmentCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region SalesExecutiveMaster

        public async Task<IEnumerable<SalesExecutiveMasterModel>> GetSalesExecutiveMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select SalesExecutiveId, SalesExecutiveCode, SalesExecutiveName, IsSalesExecutiveActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_SalesExecutive_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By SalesExecutiveId; ";

            var SalesExecutivelist = await connection.QueryAsync<SalesExecutiveMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return SalesExecutivelist;
        }

        public async Task<int> CreateSalesExecutiveMasterDetail(SalesExecutiveMasterModel salesexecutivemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_SalesExecutive_Master
                (
                    SalesExecutiveCode, SalesExecutiveName, IsSalesExecutiveActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @salesexecutivecode, @salesexecutivename, @issalesexecutiveactive,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS SalesExecutiveId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    salesexecutivecode = salesexecutivemaster.SalesExecutiveCode,
                    salesexecutivename = salesexecutivemaster.SalesExecutiveName,
                    issalesexecutiveactive = salesexecutivemaster.IsSalesExecutiveActive,
                    usercode = salesexecutivemaster.UserCode,
                    ipaddress = salesexecutivemaster.IPAddress,
                    propertycode = salesexecutivemaster.PropertyCode,
                    branchcode = salesexecutivemaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateSalesExecutiveMasterDetail(SalesExecutiveMasterModel salesexecutivemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_SalesExecutive_Master
                SET
                    SalesExecutiveName = COALESCE(@salesexecutivename, SalesExecutiveName),
                    IsSalesExecutiveActive = COALESCE(@issalesexecutiveactive, IsSalesExecutiveActive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE SalesExecutiveId = @SalesExecutiveid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        salesexecutiveid = salesexecutivemaster.SalesExecutiveId,
                        salesexecutivecode = salesexecutivemaster.SalesExecutiveCode,
                        salesexecutivename = salesexecutivemaster.SalesExecutiveName,
                        issalesexecutiveactive = salesexecutivemaster.IsSalesExecutiveActive,
                        usercode = salesexecutivemaster.UserCode,
                        ipaddress = salesexecutivemaster.IPAddress,
                        propertycode = salesexecutivemaster.PropertyCode,
                        branchcode = salesexecutivemaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = salesexecutivemaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteSalesExecutiveMasterDetail(int SalesExecutiveId, string SalesExecutiveCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_SalesExecutive_Master
                WHERE SalesExecutiveId = @salesexecutiveid AND SalesExecutiveCode = @salesexecutivecode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { salesexecutiveid = SalesExecutiveId, salesexecutivecode = SalesExecutiveCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region ReservationPayModeMaster

        public async Task<IEnumerable<ReservationPayModeMasterModel>> GetReservationPayModeMasterList(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select ReservationPayModeId, ReservationPayModeCode, ReservationPayModeName, IsReservationPayModeActive,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_ReservationPayMode_Master
                Where PropertyCode = @propertyCode And BranchCode = @branchCode Order By ReservationPayModeId; ";

            var ReservationPayModelist = await connection.QueryAsync<ReservationPayModeMasterModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return ReservationPayModelist;
        }

        public async Task<int> CreateReservationPayModeMasterDetail(ReservationPayModeMasterModel resermodemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_ReservationPayMode_Master
                (
                    ReservationPayModeCode, ReservationPayModeName, IsReservationPayModeActive,
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES
                (
                    @reservationpaymodecode, @reservationpaymodename, @isreservationpaymodeactive,
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS ReservationPayModeId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    reservationpaymodecode = resermodemaster.ReservationPayModeCode,
                    reservationpaymodename = resermodemaster.ReservationPayModeName,
                    isreservationpaymodeactive = resermodemaster.IsReservationPayModeActive,
                    usercode = resermodemaster.UserCode,
                    ipaddress = resermodemaster.IPAddress,
                    propertycode = resermodemaster.PropertyCode,
                    branchcode = resermodemaster.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateReservationPayModeMasterDetail(ReservationPayModeMasterModel resermodemaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_ReservationPayMode_Master
                SET
                    ReservationPayModeName = COALESCE(@reservationpaymodename, ReservationPayModeName),
                    IsReservationPayModeActive = COALESCE(@isreservationpaymodeactive, IsReservationPayModeActive),
                    IPAddress = COALESCE(@ipaddress, ipaddress),
                    ModifiedDate = COALESCE(@modifieddate, modifieddate),
                    ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE ReservationPayModeId = @reservationpaymodeid AND PropertyCode = @propertycode AND BranchCode = @branchcode; ";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        reservationpaymodeid = resermodemaster.ReservationPayModeId,
                        reservationpaymodecode = resermodemaster.ReservationPayModeCode,
                        reservationpaymodename = resermodemaster.ReservationPayModeName,
                        isreservationpaymodeactive = resermodemaster.IsReservationPayModeActive,
                        ipaddress = resermodemaster.IPAddress,
                        propertycode = resermodemaster.PropertyCode,
                        branchcode = resermodemaster.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = resermodemaster.UserCode
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteReservationPayModeMasterDetail(int ReservationPayModeId, string ReservationPayModeCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery = @"DELETE FROM Tbl_ReservationPayMode_Master
                WHERE ReservationPayModeId = @reservationpaymodeid AND ReservationPayModeCode = @reservationpaymodecode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery, new { reservationpaymodeid = ReservationPayModeId, reservationpaymodecode = ReservationPayModeCode, propertycode = PropertyCode, branchcode = BranchCode });
                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Common Method

        //public async Task<string> GetNextMasterCode( string tableName, string codeColumn, string prefix, string propertyCode, string branchCode)
        //{
        //    using var connection = await _factory.CreateConnection(DbNames.HMS);
            
        //    var sql = $""" SELECT MAX( TRY_CONVERT( INT, SUBSTRING( [{codeColumn}], @PrefixLength + 1, LEN([{codeColumn}]) ) ) ) FROM [{tableName}] WHERE PropertyCode = @PropertyCode AND BranchCode = @BranchCode AND [{codeColumn}] LIKE @Prefix + '%' """;

        //    var result = await connection.ExecuteScalarAsync<int?>(
        //        sql,
        //        new
        //        {
        //            PrefixLength = prefix.Length,
        //            Prefix = prefix,
        //            PropertyCode = propertyCode,
        //            BranchCode = branchCode
        //        });

        //    var nextNumber = (result ?? 0) + 1;

        //    return $"{prefix}{nextNumber:D3}";
        //}

        #endregion
    }
}
