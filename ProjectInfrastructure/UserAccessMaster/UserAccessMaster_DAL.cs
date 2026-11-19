using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.UserAccessMaster;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Data;

namespace FrontOfficeApplication.ProjectInfrastructure.UserAccessMaster
{
    public class UserAccessMaster_DAL : IUserAccessMaster_Repository
    {
        private readonly IDbConnectionFactory _factory;

        public UserAccessMaster_DAL(IDbConnectionFactory connectionFactory)
        {
            _factory = connectionFactory;
        }

        #region UserLoginAccess

        public async Task<IEnumerable<UserMaster>> GetBranchesByMasterUser(LoginRequest request)
        {
            using var connection = _factory.CreateMasterConnection();

            var selectquery = @"SELECT UserCode, UserName, PropertyName, PropertyCode, BranchName, BranchCode, RoleId
                FROM Tbl_User_Master Where UserStatus = 1 And LOWER(UserName) = @username And UserPassword = @password Order By UserCode; ";

            return await connection.QueryAsync<UserMaster>(selectquery, new { username = request.Username.ToLower(), password = request.Password });
        }

        public async Task<UserMaster> GetUserDetails(LoginBranchRequest request)
        {
            using var connection = await _factory.CreateConnection( request.BranchCode, DbNames.HMS);

            var selectquery = @"SELECT UserCode, UserName, UserDOB, UserAddress, UserMobile, UserMail, UserStatus, DiscountPer,
                DiscountValue, GracePeriod, PropertyName, PropertyCode, BranchName, BranchCode, RoleId
                FROM Tbl_User_Master Where UserStatus = 1 And LOWER(UserName) = @username And UserPassword = @password And BranchCode = @branchCode And RoleId = @roleid Order By UserCode; ";

             var userdetails = await connection.QueryFirstOrDefaultAsync<UserMaster>(selectquery, new { username = request.Username.ToLower(), password = request.Password, branchCode = request.BranchCode, roleid = request.RoleId });

            return userdetails!;
        }

        #endregion

        #region UserAccessMaster

        public async Task<IEnumerable<UserMaster>> GetUserMasterList(string branchcode, string propertycode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"SELECT  UserCode, UserName, UserDOB, UserAddress, UserMobile, UserMail, UserStatus, DiscountPer,
                DiscountValue, GracePeriod, PropertyName, PropertyCode, BranchName, BranchCode, RoleId
                FROM Tbl_User_Master Where UserStatus = 1 And PropertyCode = @propertyCode And BranchCode = @branchCode Order By UserCode ;";

            var userAccessList = await connection.QueryAsync<UserMaster>(selectquery, new { PropertyCode = propertycode, BranchCode = branchcode });
            return userAccessList;
        }


        public async Task<int> CreateUserMaster(UserMaster userAccessMaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            int newId = 0;

            var checkQuery = "SELECT COUNT(1) FROM Tbl_User_Master WHERE LOWER(UserName) = @username And UserCode = @usercode And RoleId = @roleid And BranchCode = @branchcode";
            var userExists = await connection.QuerySingleAsync<int>(checkQuery,
                new { username = userAccessMaster.UserName.ToLower(), usercode = userAccessMaster.UserCode, roleid = userAccessMaster.RoleId, branchcode = userAccessMaster.BranchCode });

            if (userExists > 0)
            {
                return newId = -1; // Indicate that the username already exists
            }

            using var transaction = connection.BeginTransaction();

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_User_Master (UserCode, UserName, UserPassword, UserDOB, UserAddress, UserMobile, UserMail, UserStatus, DiscountPer,
                DiscountValue, GracePeriod, PropertyName, PropertyCode, BranchName, BranchCode, RoleId)
                VALUES (@UserCode, @UserName, @UserPassword, @UserDOB, @UserAddress, @UserMobile, @UserMail, @UserStatus, @DiscountPer,
                @DiscountValue, @GracePeriod, @PropertyName, @PropertyCode, @BranchName, @BranchCode, @RoleId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                newId = await connection.QuerySingleAsync<int>(insertquery, userAccessMaster, transaction);

                // =========================================
                // COPY DEFAULT ROLE PERMISSIONS
                // =========================================

                var insertPermissionQuery = @" INSERT INTO Tbl_UserPermission_Master
                    ( UserCode, UserName, RoleId, MainMenuId, SubMenuId, IsPermission, CanView, CanAdd, CanEdit, CanDelete, CanPrint, PropertyCode, BranchCode )
                    SELECT 
                    @usercode, @username, rp.RoleId, rp.MainMenuId, rp.SubMenuId, rp.IsPermission, rp.CanView, rp.CanAdd, rp.CanEdit, rp.CanDelete, rp.CanPrint, 
                    @propertycode, @branchcode FROM Tbl_RolePermission_Master rp
                    INNER JOIN Tbl_SubIndex_Master sm ON sm.SubMenuId = rp.SubMenuId
                    INNER JOIN Tbl_Role_Master rm ON rm.RoleId = rp.RoleId
                    WHERE rp.RoleId = @roleid";

                await connection.ExecuteAsync(insertPermissionQuery, new { username = userAccessMaster.UserName, usercode = userAccessMaster.UserCode, roleid = userAccessMaster.RoleId, branchcode = userAccessMaster.BranchCode, propertycode = userAccessMaster.PropertyCode }, transaction);

                var insertOperationPermissionQuery = @" INSERT INTO Tbl_UserOperationRights_Master
                    ( UserCode, UserName, RoleId, OperationRightId, IsPermission, PropertyCode, BranchCode)
                    SELECT 
                    @usercode, @username, RoleId, OperationRightId, IsPermission, @propertycode, @branchcode
                    FROM Tbl_RoleOperationRights_Master
                    WHERE RoleId = @roleid";

                await connection.ExecuteAsync(insertOperationPermissionQuery, new { username = userAccessMaster.UserName, usercode = userAccessMaster.UserCode, roleid = userAccessMaster.RoleId, branchcode = userAccessMaster.BranchCode, propertycode = userAccessMaster.PropertyCode }, transaction);

                var insertAccessPermissionQuery = @" INSERT INTO Tbl_UserAccessRights_Master
                    ( UserCode, UserName, RoleId, AccessRightId, IsPermission, PropertyCode, BranchCode )
                    SELECT 
                    @usercode, @username, RoleId, AccessRightId, IsPermission, @propertycode, @branchcode 
                    FROM Tbl_RoleAccessRights_Master 
                    WHERE RoleId = @roleid";

                await connection.ExecuteAsync(insertAccessPermissionQuery, new { username = userAccessMaster.UserName, usercode = userAccessMaster.UserCode, roleid = userAccessMaster.RoleId, branchcode = userAccessMaster.BranchCode, propertycode = userAccessMaster.PropertyCode }, transaction);

                transaction.Commit();
                newId = userAccessMaster.UserCode;
                return newId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateUserMaster(UserMaster userAccessMaster)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            // First check whether another user already uses this combination
            //var checkQuery = @" SELECT COUNT(1) FROM Tbl_User_Master WHERE LOWER(UserName) = LOWER(@username) AND UserCode = @usercode AND RoleId = @roleid AND BranchCode = @branchcode";

            //var userExists = await connection.QuerySingleAsync<int>( checkQuery, new { username = userAccessMaster.UserName, usercode = userAccessMaster.UserCode, roleid = userAccessMaster.RoleId, branchcode = userAccessMaster.BranchCode });

            //if (userExists > 0)
            //{
            //    return -1;
            //}

            using var transaction = connection.BeginTransaction();

            try
            {
                // =========================================
                // UPDATE USER MASTER
                // =========================================

                var updateUserQuery = @"
                UPDATE Tbl_User_Master
                SET
                UserName = COALESCE(@username, UserName),
                UserPassword = COALESCE(@userpassword, UserPassword),
                UserDOB = COALESCE(@userdob, UserDOB),
                UserAddress = COALESCE(@useraddress, UserAddress),
                UserMobile = COALESCE(@usermobile, UserMobile),
                UserMail = COALESCE(@usermail, UserMail),
                UserStatus = COALESCE(@userstatus, UserStatus),
                DiscountPer = COALESCE(@discountper, DiscountPer),
                DiscountValue = COALESCE(@discountvalue, DiscountValue),
                GracePeriod = COALESCE(@graceperiod, GracePeriod),
                PropertyName = COALESCE(@propertyname, PropertyName),
                PropertyCode = COALESCE(@propertycode, PropertyCode),
                BranchName = COALESCE(@branchname, BranchName),
                BranchCode = COALESCE(@branchcode, BranchCode),
                RoleId = COALESCE(@roleid, RoleId)
                WHERE UserCode = @usercode;";

                var rowsAffected = await connection.ExecuteAsync(
                    updateUserQuery,
                    new
                    {
                        usercode = userAccessMaster.UserCode,
                        username = userAccessMaster.UserName,
                        userpassword = userAccessMaster.UserPassword,
                        userdob = userAccessMaster.UserDOB,
                        useraddress = userAccessMaster.UserAddress,
                        usermobile = userAccessMaster.UserMobile,
                        usermail = userAccessMaster.UserMail,
                        userstatus = userAccessMaster.UserStatus,
                        discountper = userAccessMaster.DiscountPer,
                        discountvalue = userAccessMaster.DiscountValue,
                        graceperiod = userAccessMaster.GracePeriod,
                        propertyname = userAccessMaster.PropertyName,
                        propertycode = userAccessMaster.PropertyCode,
                        branchname = userAccessMaster.BranchName,
                        branchcode = userAccessMaster.BranchCode,
                        roleid = userAccessMaster.RoleId
                    },
                    transaction);

                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                // =========================================
                // DELETE OLD ROLE PERMISSIONS
                // =========================================

                var deletePermissionQuery = @"
                DELETE FROM Tbl_UserPermission_Master WHERE UserCode = @usercode  AND PropertyCode = @propertycode AND BranchCode = @branchcode;";

                await connection.ExecuteAsync( deletePermissionQuery, new { usercode = userAccessMaster.UserCode, propertycode = userAccessMaster.PropertyCode, branchcode = userAccessMaster.BranchCode }, transaction);

                // =========================================
                // INSERT ROLE PERMISSIONS AGAIN
                // =========================================

                var insertPermissionQuery = @"
                INSERT INTO Tbl_UserPermission_Master
                (
                    UserCode, UserName, RoleId, MainMenuId, SubMenuId, IsPermission, CanView, CanAdd, CanEdit, CanDelete, CanPrint, PropertyCode, BranchCode
                )
                SELECT
                @UserCode, @UserName, rp.RoleId, rp.MainMenuId, rp.SubMenuId, rp.IsPermission, rp.CanView, rp.CanAdd, rp.CanEdit, rp.CanDelete, rp.CanPrint, @PropertyCode, @BranchCode FROM Tbl_RolePermission_Master rp
                INNER JOIN Tbl_SubIndex_Master sm ON sm.SubMenuId = rp.SubMenuId
                INNER JOIN Tbl_Role_Master rm ON rm.RoleId = rp.RoleId
                WHERE rp.RoleId = @RoleId;";

                await connection.ExecuteAsync( insertPermissionQuery, userAccessMaster, transaction);

                // =========================================
                // DELETE OLD OPERATION RIGHTS
                // =========================================

                var deleteOperationPermissionQuery = @"
                DELETE FROM Tbl_UserOperationRights_Master WHERE UserCode = @usercode AND PropertyCode = @propertycode AND BranchCode = @branchcode;";

                await connection.ExecuteAsync( deleteOperationPermissionQuery, new { usercode = userAccessMaster.UserCode, propertycode = userAccessMaster.PropertyCode, branchcode = userAccessMaster.BranchCode }, transaction);

                // =========================================
                // INSERT OPERATION RIGHTS AGAIN
                // =========================================

                var insertOperationPermissionQuery = @"
                INSERT INTO Tbl_UserOperationRights_Master
                (
                    UserCode, UserName, RoleId, OperationRightId, IsPermission, PropertyCode, BranchCode
                )
                SELECT
                @UserCode, @UserName, RoleId, OperationRightId, IsPermission, @PropertyCode, @BranchCode
                FROM Tbl_RoleOperationRights_Master
                WHERE RoleId = @RoleId;";

                await connection.ExecuteAsync( insertOperationPermissionQuery, userAccessMaster, transaction);

                // =========================================
                // DELETE OLD ACCESS RIGHTS
                // =========================================

                var deleteAccessPermissionQuery = @"
                DELETE FROM Tbl_UserAccessRights_Master WHERE UserCode = @UserCode AND PropertyCode = @PropertyCode AND BranchCode = @BranchCode;";

                await connection.ExecuteAsync( deleteAccessPermissionQuery, new { usercode = userAccessMaster.UserCode, propertycode = userAccessMaster.PropertyCode, branchcode = userAccessMaster.BranchCode }, transaction);

                // =========================================
                // INSERT ACCESS RIGHTS AGAIN
                // =========================================

                var insertAccessPermissionQuery = @"
                INSERT INTO Tbl_UserAccessRights_Master
                (
                    UserCode, UserName, RoleId, AccessRightId, IsPermission, PropertyCode, BranchCode
                )
                SELECT
                @UserCode, @UserName, RoleId, AccessRightId, IsPermission, @PropertyCode, @BranchCode
                FROM Tbl_RoleAccessRights_Master
                WHERE RoleId = @RoleId;";

                await connection.ExecuteAsync( insertAccessPermissionQuery, userAccessMaster, transaction);

                // =========================================
                // COMMIT
                // =========================================

                transaction.Commit();

                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> DeleteUserMaster(int UserCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Get user details first
                const string checkUserQuery = @"SELECT UserCode, PropertyCode, BranchCode FROM Tbl_User_Master WHERE UserCode = @usercode And PropertyCode = @propertycode And BranchCode = @branchcode";
                var user = await connection.QuerySingleOrDefaultAsync<UserMaster>(checkUserQuery, new { usercode = UserCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                if (user == null)
                {
                    transaction.Rollback();
                    return 0;
                }

                // Delete User Permissions
                const string deletequery1 = @"DELETE FROM Tbl_UserPermission_Master
                WHERE UserCode = @usercode AND PropertyCode = @propertycode  AND BranchCode = @branchcode ";
                await connection.ExecuteAsync(deletequery1, new { usercode = UserCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                // Delete Operation Rights
                const string deletequery2 = @"DELETE FROM Tbl_UserOperationRights_Master
                WHERE UserCode = @usercode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                await connection.ExecuteAsync(deletequery2, new { usercode = UserCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                // Delete Access Rights
                const string deletequery3 = @"DELETE FROM Tbl_UserAccessRights_Master
                WHERE UserCode = @usercode AND PropertyCode = @propertycode AND BranchCode = @branchcode";
                await connection.ExecuteAsync(deletequery3, new { usercode = UserCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                // Finally delete User
                const string deletequery4 = @"DELETE FROM Tbl_User_Master WHERE UserCode = @usercode And PropertyCode = @propertycode And BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync( deletequery4, new { usercode = UserCode, propertycode = PropertyCode, branchcode = BranchCode }, transaction);

                transaction.Commit();

                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> ChangeUserPassword(int UserCode, string PropertyCode, string BranchCode, string NewUserPassword)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                // Get user details first
                var updateUserQuery = @"
                UPDATE Tbl_User_Master
                SET
                UserPassword = COALESCE(@userpassword, UserPassword)
                WHERE UserCode = @usercode And PropertyCode = @propertycode And BranchCode = @branchcode;";

                var rowsAffected = await connection.ExecuteAsync(
                    updateUserQuery,
                    new
                    {
                        usercode = UserCode,
                        propertycode = PropertyCode,
                        branchcode = BranchCode,
                        userpassword = NewUserPassword
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

        #endregion

        #region Branch, Product And Role Master

        public async Task<IEnumerable<BranchMaster>> GetBranchesList()
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select BranchId, BranchCode, BranchName, Address1, Address2, Phone_number, Mobile_number, 
                Fax_number, Email_id, Tin_no, Licence_number, PropertyCode From Tbl_Branch_Master Order By BranchId";

            return await connection.QueryAsync<BranchMaster>(selectquery);
        }

        public async Task<IEnumerable<PropertyMaster>> GetPropertiesList()
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select PropertyId, PropertyName, PropertyCode, StartYear, Address1, Address2, Phone_number, Mobile_number,
	            OwnerName, Owner_Number, Fax_number, Email_id, Tin_no, Licence_number From Tbl_Branch_Master Order By PropertyId";

            return await connection.QueryAsync<PropertyMaster>(selectquery);
        }

        public async Task<IEnumerable<RoleMaster>> GetRoleMasterList()
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select RoleId, RoleName, Description, Status From Tbl_Role_Master Order By RoleId";
            var roleList = await connection.QueryAsync<RoleMaster>(selectquery);
            return roleList;
        }

        #endregion

        #region UserPermissionAccessMaster

        public async Task<IEnumerable<MainMenuModel>> GetMainMenuList()
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = "Select MainMenuId, MainMenuName, MainMenuRequired From Tbl_MenuIndex_Master Order By MainMenuId";
            var MenuList = await connection.QueryAsync<MainMenuModel>(selectquery);
            return MenuList;
        }

        public async Task<IEnumerable<SubMenuModel>> GetSubMenuList()
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = "SELECT SubMenuId, SubMenuName, SubMenuRequired, MainMenuId, ParentSubMenuId FROM Tbl_SubIndex_Master Order By MainMenuId";
            var MenuList = await connection.QueryAsync<SubMenuModel>(selectquery);
            return MenuList;
        }

        public async Task<int> SaveMenuWithSubMenu(MenuWithSubMenuModel model)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                int mainMenuId = model.MainMenuId;
                int subMenuId = 0;

                //------------------------------------------------
                // INSERT MAIN MENU IF NEW
                //------------------------------------------------

                if (!model.IsExistingMainMenu)
                {
                    string mainMenuQuery = @"
                    INSERT INTO Tbl_MenuIndex_Master ( MainMenuName, MainMenuRequired)
                    VALUES
                    ( @MainMenuName, @MainMenuRequired )

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    mainMenuId = await connection.ExecuteScalarAsync<int>(mainMenuQuery, model, transaction);
                }

                //------------------------------------------------
                // INSERT SUBMENU
                //------------------------------------------------

                if (model.ParentSubMenuId > 0)
                {
                    string subMenuQuery = @"
                    INSERT INTO Tbl_SubIndex_Master ( MainMenuId, SubMenuName, SubMenuRequired, ParentSubMenuId )
                    VALUES
                    ( @MainMenuId, @SubMenuName, @SubMenuRequired, @ParentSubMenuId )

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        subMenuId = await connection.ExecuteScalarAsync<int>(subMenuQuery,
                            new
                            {
                                MainMenuId = mainMenuId,
                                model.SubMenuName,
                                model.SubMenuRequired,
                                model.ParentSubMenuId,
                            },
                            transaction);
                }
                else
                {
                    if(model.SubMenuName != null && model.SubMenuName.ToLower() != "string")
                    {
                        string subMenuQuery = @"
                    INSERT INTO Tbl_SubIndex_Master ( MainMenuId, SubMenuName, SubMenuRequired )
                    VALUES
                    ( @MainMenuId, @SubMenuName, @SubMenuRequired )

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        subMenuId = await connection.ExecuteScalarAsync<int>(subMenuQuery,
                            new
                            {
                                MainMenuId = mainMenuId,
                                model.SubMenuName,
                                model.SubMenuRequired,
                            },
                            transaction);
                    }
                }

                //------------------------------------------------
                // SYNC ROLE PERMISSIONS
                //------------------------------------------------

                await connection.ExecuteAsync("SP_SyncRolePermissionMaster", transaction: transaction, commandType: CommandType.StoredProcedure);

                transaction.Commit();

                //return (mainMenuId + subMenuId);
                return mainMenuId > 0 ? mainMenuId : subMenuId;

            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteMainMenuDetail(int MainMenuId)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                const string deletequery1 = @"DELETE FROM Tbl_UserPermission_Master WHERE MainMenuId = @mainmenuid;";
                
                var rowsAffected1 = await connection.ExecuteAsync(deletequery1, new { mainmenuid = MainMenuId }, transaction);

                const string deletequery2 = @"DELETE FROM Tbl_RolePermission_Master WHERE MainMenuId = @mainmenuid;";
                
                var rowsAffected2 = await connection.ExecuteAsync(deletequery2, new { mainmenuid = MainMenuId }, transaction);

                const string deletequery3 = @"DELETE FROM Tbl_SubIndex_Master WHERE MainMenuId = @mainmenuid;";

                var rowsAffected3 = await connection.ExecuteAsync(deletequery3, new { mainmenuid = MainMenuId }, transaction);

                const string deletequery4 = @"DELETE FROM Tbl_MenuIndex_Master WHERE MainMenuId = @mainmenuid;";
                
                var rowsAffected4 = await connection.ExecuteAsync(deletequery4, new { mainmenuid = MainMenuId }, transaction);

                transaction.Commit();

                return (rowsAffected4) > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteSubMenuDetail(int SubMenuId)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                const string deletequery1 = @"DELETE FROM Tbl_UserPermission_Master WHERE SubMenuId = @submenuid;";

                var rowsAffected1 = await connection.ExecuteAsync(deletequery1, new { submenuid = SubMenuId }, transaction);

                const string deletequery2 = @"DELETE FROM Tbl_RolePermission_Master WHERE SubMenuId = @submenuid;";

                var rowsAffected2 = await connection.ExecuteAsync(deletequery2, new { submenuid = SubMenuId }, transaction);

                const string deletequery3 = @"DELETE FROM Tbl_SubIndex_Master WHERE SubMenuId = @submenuid;";

                var rowsAffected3 = await connection.ExecuteAsync(deletequery3, new { submenuid = SubMenuId }, transaction);

                transaction.Commit();

                return (rowsAffected3) > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<UserPermissionAccessMaster>> GetUserPermissionAccessList(string branchcode, string propertycode, int usercode, int roleId)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var query = @"

                --------------------------------------------------------
                -- EXISTING USER PERMISSIONS
                --------------------------------------------------------

                SELECT UP.UserCode, UP.UserName, UP.RoleId, RM.RoleName, UP.MainMenuId, MM.MainMenuName, MM.MainMenuRequired, UP.SubMenuId, 
                SM.SubMenuName, SM.SubMenuRequired, UP.IsPermission, UP.CanView, UP.CanAdd, UP.CanEdit, UP.CanDelete, UP.CanPrint, UP.PropertyCode, UP.BranchCode
                FROM Tbl_UserPermission_Master UP
                INNER JOIN Tbl_MenuIndex_Master MM ON MM.MainMenuId = UP.MainMenuId
                INNER JOIN Tbl_SubIndex_Master SM ON SM.SubMenuId = UP.SubMenuId
                INNER JOIN Tbl_Role_Master RM ON RM.RoleId = UP.RoleId
                WHERE UP.BranchCode = @BranchCode AND UP.PropertyCode = @PropertyCode AND UP.UserCode = @UserCode AND UP.RoleId = @RoleId

                UNION
                --------------------------------------------------------
                -- NEW ROLE PERMISSIONS NOT IN USER TABLE
                --------------------------------------------------------

                SELECT @UserCode AS UserCode, '' AS UserName, RP.RoleId, RM.RoleName, RP.MainMenuId, MM.MainMenuName, MM.MainMenuRequired, RP.SubMenuId, 
                SM.SubMenuName, SM.SubMenuRequired, RP.IsPermission, RP.CanView, RP.CanAdd, RP.CanEdit, RP.CanDelete, RP.CanPrint, @PropertyCode, @BranchCode
                FROM Tbl_RolePermission_Master RP
                INNER JOIN Tbl_MenuIndex_Master MM ON mm.MainMenuId = RP.MainMenuId
                INNER JOIN Tbl_SubIndex_Master SM ON sm.SubMenuId = RP.SubMenuId
                INNER JOIN Tbl_Role_Master RM ON rm.RoleId = RP.RoleId
                WHERE RP.RoleId = @RoleId 
                
                AND NOT EXISTS
                (
                SELECT 1 FROM Tbl_UserPermission_Master UP WHERE UP.UserCode = @UserCode AND UP.MainMenuId = RP.MainMenuId AND UP.SubMenuId = RP.SubMenuId
                ) ORDER BY MainMenuId, SubMenuId ";

            var result = await connection.QueryAsync<UserPermissionAccessMaster>(query, new { BranchCode = branchcode, PropertyCode = propertycode, UserCode = usercode, RoleId = roleId });

            return result;
        }

        public async Task<int> SaveUserPermissions(List<UserPermissionAccessMaster> permissions)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                string insertquery = @"
                MERGE Tbl_UserPermission_Master AS TARGET

                USING
                (
	                SELECT
	                @UserCode AS UserCode,
                    @UserName AS UserName,
                    @RoleId AS RoleId,
                    @MainMenuId AS MainMenuId,
                    @SubMenuId AS SubMenuId,
                    @IsPermission AS IsPermission,
	                @CanView AS CanView,
	                @CanAdd AS CanAdd, 
	                @CanEdit AS CanEdit,
	                @CanDelete AS CanDelete,
	                @CanPrint AS CanPrint,
	                @PropertyCode AS PropertyCode,
	                @BranchCode AS BranchCode
        
                ) AS SOURCE

                ON TARGET.UserCode = SOURCE.UserCode
                AND TARGET.MainMenuId = SOURCE.MainMenuId
                AND TARGET.SubMenuId = SOURCE.SubMenuId
                AND TARGET.BranchCode = SOURCE.BranchCode
                WHEN MATCHED THEN
                UPDATE SET
                TARGET.IsPermission = SOURCE.IsPermission,
                TARGET.CanView = SOURCE.CanView,
                TARGET.CanAdd = SOURCE.CanAdd,
                TARGET.CanEdit = SOURCE.CanEdit,
                TARGET.CanDelete = SOURCE.CanDelete,
                TARGET.CanPrint = SOURCE.CanPrint

                WHEN NOT MATCHED THEN

                INSERT
                (
	                UserCode, UserName, RoleId, MainMenuId, SubMenuId, IsPermission, 
	                CanView, CanAdd, CanEdit, CanDelete, CanPrint, PropertyCode, BranchCode
                )
                VALUES
                (
	                SOURCE.UserCode,
                    SOURCE.UserName,
                    SOURCE.RoleId,
                    SOURCE.MainMenuId,
                    SOURCE.SubMenuId,
                    SOURCE.IsPermission,
                    SOURCE.CanView,
                    SOURCE.CanAdd,
                    SOURCE.CanEdit,
                    SOURCE.CanDelete,
                    SOURCE.CanPrint,
                    SOURCE.PropertyCode,
                    SOURCE.BranchCode        
                );";

                int affectedRows = await connection.ExecuteAsync( insertquery, permissions, transaction );

                transaction.Commit();

                return affectedRows;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<AccessRightsMaster>> GetAccessRightsList()
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = "Select AccessRightId, AccessRightCode, AccessRightName, IsActive From Tbl_AccessRights_Master Order By AccessRightId ";

            var AccessRightsList = await connection.QueryAsync<AccessRightsMaster>(selectquery);

            return AccessRightsList;
        }

        public async Task<IEnumerable<OperationRightsMaster>> GetOperationRightsList()
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = "SELECT OperationRightId, OperationCode, OperationName, IsActive FROM Tbl_OperationRights_Master Order By OperationRightId";

            var OperationRightsList = await connection.QueryAsync<OperationRightsMaster>(selectquery);

            return OperationRightsList;
        }

        public async Task<int> SaveAccessOROperationRights(RightsMasterRequest requestmodel)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                int AccessId = 0;
                int opertaionId = 0;

                //------------------------------------------------
                // INSERT Access Rights
                //------------------------------------------------

                if (requestmodel.AccessType.ToLower() == "accessrights")
                {
                    string accessquery = @"
                    INSERT INTO Tbl_AccessRights_Master ( AccessRightCode, AccessRightName, IsActive)
                    VALUES
                    ( @accessrightcode, @accessrightname, @isactive)

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    AccessId = await connection.ExecuteScalarAsync<int>(accessquery, new
                    {
                        accessrightcode = requestmodel.RightsCode,
                        accessrightname = requestmodel.RightsName,
                        isactive = requestmodel.IsPermission
                    }, transaction);

                    //string roleaccessquery = @"
                    //INSERT INTO Tbl_RoleAccessRights_Master ( RoleId, AccessRightId, IsPermission)
                    //VALUES
                    //( @RoleId, @AccessRightId, @ispermission) ";

                    //await connection.ExecuteAsync(roleaccessquery, new { RoleId = 1,
                    //    AccessRightId = AccessId,
                    //    ispermission = requestmodel.IsPermission
                    //}, transaction);

                    string roleAccessQuery = @"
                    INSERT INTO Tbl_RoleAccessRights_Master ( RoleId, AccessRightId, IsPermission )
                    SELECT RoleId, @AccessRightId, CASE WHEN RoleId IN (1, 2) THEN 1 ELSE 0 END FROM Tbl_Role_Master WHERE Status = 'active';";

                    await connection.ExecuteAsync(
                        roleAccessQuery,
                        new
                        {
                            AccessRightId = AccessId
                        },
                        transaction);
                }

                //------------------------------------------------
                // INSERT Operation Rights
                //------------------------------------------------

                if (requestmodel.AccessType.ToLower() == "operationrights")
                {
                    string opertaionquery = @"
                    INSERT INTO Tbl_OperationRights_Master ( OperationCode, OperationName, IsActive )
                    VALUES
                    ( @operationcode, @operationname, @isactive )

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    opertaionId = await connection.ExecuteScalarAsync<int>(opertaionquery, new
                    {
                        operationcode = requestmodel.RightsCode,
                        operationname = requestmodel.RightsName,
                        isactive = requestmodel.IsPermission
                    }, transaction);

                    //string roleopertaionquery = @"
                    //INSERT INTO Tbl_RoleOperationRights_Master ( RoleId, OperationRightId, IsPermission)
                    //VALUES
                    //( @RoleId, @OperationRightId, @ispermission) ";

                    //await connection.ExecuteAsync(roleopertaionquery, new { RoleId = 1,
                    //    OperationRightId = opertaionId,
                    //    ispermission = requestmodel.IsPermission
                    //}, transaction);

                    string roleOperationQuery = @" 
                    INSERT INTO Tbl_RoleOperationRights_Master ( RoleId, OperationRightId, IsPermission )
                    SELECT RoleId, @OperationRightId, CASE WHEN RoleId IN (1, 2) THEN 1 ELSE 0 END FROM Tbl_Role_Master WHERE Status = 'active';";

                    await connection.ExecuteAsync(
                        roleOperationQuery,
                        new
                        {
                            OperationRightId = opertaionId
                        },
                        transaction);
                }

                transaction.Commit();

                return AccessId > 0 ? AccessId : opertaionId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAccessRightsDetail(int AccessRightId)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                const string deletequery1 = @"DELETE FROM Tbl_UserAccessRights_Master WHERE AccessRightId = @accessrightid;";

                var rowsAffected1 = await connection.ExecuteAsync(deletequery1, new { accessrightid = AccessRightId }, transaction);

                const string deletequery2 = @"DELETE FROM Tbl_RoleAccessRights_Master WHERE AccessRightId = @accessrightid;";

                var rowsAffected2 = await connection.ExecuteAsync(deletequery2, new { accessrightid = AccessRightId }, transaction);

                const string deletequery3 = @"DELETE FROM Tbl_AccessRights_Master WHERE AccessRightId = @accessrightid;";

                var rowsAffected3 = await connection.ExecuteAsync(deletequery3, new { accessrightid = AccessRightId }, transaction);

                transaction.Commit();

                return (rowsAffected3) > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteOperationRightsDetail(int OperationRightId)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                const string deletequery1 = @"DELETE FROM Tbl_UserOperationRights_Master WHERE OperationRightId = @operationrightid;";

                var rowsAffected1 = await connection.ExecuteAsync(deletequery1, new { operationrightid = OperationRightId }, transaction);

                const string deletequery2 = @"DELETE FROM Tbl_RoleOperationRights_Master WHERE OperationRightId = @operationrightid;";

                var rowsAffected2 = await connection.ExecuteAsync(deletequery2, new { operationrightid = OperationRightId }, transaction);

                const string deletequery3 = @"DELETE FROM Tbl_OperationRights_Master WHERE OperationRightId = @operationrightid;";

                var rowsAffected3 = await connection.ExecuteAsync(deletequery3, new { operationrightid = OperationRightId }, transaction);

                transaction.Commit();

                return (rowsAffected3) > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<UserAccessRightsMaster>> GetUserAccessRightsList(string branchcode, string propertycode, int usercode, int roleId)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var query = @"

                --------------------------------------------------------
                -- EXISTING USER PERMISSIONS
                --------------------------------------------------------

                SELECT UAR.UserCode, UAR.UserName, UAR.RoleId, RM.RoleName, UAR.AccessRightId,  AR.AccessRightName, UAR.IsPermission, UAR.PropertyCode, UAR.BranchCode
                FROM Tbl_UserAccessRights_Master UAR
                INNER JOIN Tbl_AccessRights_Master AR ON AR.AccessRightId = UAR.AccessRightId
                INNER JOIN Tbl_Role_Master RM ON RM.RoleId = UAR.RoleId
                WHERE UAR.BranchCode = @BranchCode AND UAR.PropertyCode = @PropertyCode AND UAR.UserCode = @UserCode AND UAR.RoleId = @RoleId

                UNION
                --------------------------------------------------------
                -- NEW ROLE PERMISSIONS NOT IN USER TABLE
                --------------------------------------------------------

                SELECT @UserCode AS UserCode, '' AS UserName, RAM.RoleId, RM.RoleName, RAM.AccessRightId,  AR.AccessRightName, RAM.IsPermission, @PropertyCode, @BranchCode
                FROM Tbl_RoleAccessRights_Master RAM
                INNER JOIN Tbl_AccessRights_Master AR ON AR.AccessRightId = RAM.AccessRightId
                INNER JOIN Tbl_Role_Master RM ON RM.RoleId = RAM.RoleId
                WHERE RAM.RoleId = @RoleId 

                AND NOT EXISTS
                (
                SELECT 1 FROM Tbl_UserAccessRights_Master UAR WHERE UAR.UserCode = @UserCode AND UAR.AccessRightId =  AR.AccessRightId  AND UAR.RoleId = RM.RoleId
                ) 
                ORDER BY AccessRightId "; 

            var result = await connection.QueryAsync<UserAccessRightsMaster>(query, new { BranchCode = branchcode, PropertyCode = propertycode, UserCode = usercode, RoleId = roleId });

            return result;
        }

        public async Task<int> InsertUserAccessRightsMaster(List<UserAccessRightsMaster> useraccessright)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                string insertquery = @"
                MERGE Tbl_UserAccessRights_Master AS TARGET

                USING
                (
	                SELECT
	                @UserCode AS UserCode,
                    @UserName AS UserName,
                    @RoleId AS RoleId,
                    @AccessRightId AS AccessRightId,
                    @IsPermission AS IsPermission,
	                @PropertyCode AS PropertyCode,
	                @BranchCode AS BranchCode
        
                ) AS SOURCE

                ON TARGET.UserCode = SOURCE.UserCode
                AND TARGET.AccessRightId = SOURCE.AccessRightId
                AND TARGET.BranchCode = SOURCE.BranchCode
                WHEN MATCHED THEN
                UPDATE SET
                TARGET.IsPermission = SOURCE.IsPermission

                WHEN NOT MATCHED THEN

                INSERT
                (
	                UserCode, UserName, RoleId, AccessRightId, IsPermission, PropertyCode, BranchCode
                )
                VALUES
                (
	                SOURCE.UserCode,
                    SOURCE.UserName,
                    SOURCE.RoleId,
                    SOURCE.AccessRightId,
                    SOURCE.IsPermission,
                    SOURCE.PropertyCode,
                    SOURCE.BranchCode        
                ); ";

                int affectedRows = await connection.ExecuteAsync(insertquery, useraccessright, transaction);

                transaction.Commit();

                return affectedRows;
            }
            catch
            {
                transaction.Rollback();
                throw ;
            }
        }

        public async Task<IEnumerable<UserOperationRightsMaster>> GetUserOperationRightsList(string branchcode, string propertycode, int usercode, int roleId)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var query = @"

                --------------------------------------------------------
                -- EXISTING USER PERMISSIONS
                --------------------------------------------------------

                SELECT UOR.UserCode, UOR.UserName, UOR.RoleId, RM.RoleName, UOR.OperationRightId,  ORM.OperationName, UOR.IsPermission, UOR.PropertyCode, UOR.BranchCode
                FROM Tbl_UserOperationRights_Master UOR
                INNER JOIN Tbl_OperationRights_Master ORM ON ORM.OperationRightId = UOR.OperationRightId
                INNER JOIN Tbl_Role_Master RM ON RM.RoleId = UOR.RoleId
                WHERE UOR.BranchCode = @BranchCode AND UOR.PropertyCode = @PropertyCode AND UOR.UserCode = @UserCode AND UOR.RoleId = @RoleId

                UNION
                --------------------------------------------------------
                -- NEW ROLE PERMISSIONS NOT IN USER TABLE
                --------------------------------------------------------

                SELECT @UserCode AS UserCode, '' AS UserName, ROM.RoleId, RM.RoleName, ROM.OperationRightId,  ORM.OperationName, ROM.IsPermission, @PropertyCode, @BranchCode
                FROM Tbl_RoleOperationRights_Master ROM
                INNER JOIN Tbl_OperationRights_Master ORM ON ORM.OperationRightId = ROM.OperationRightId
                INNER JOIN Tbl_Role_Master RM ON RM.RoleId = ROM.RoleId
                WHERE ROM.RoleId = @RoleId

                AND NOT EXISTS
                (
                SELECT 1 FROM Tbl_UserOperationRights_Master UOR WHERE UOR.UserCode = @UserCode AND UOR.OperationRightId = ORM.OperationRightId  AND UOR.RoleId = RM.RoleId
                )
                ORDER BY OperationRightId ";

            var result = await connection.QueryAsync<UserOperationRightsMaster>(query, new { BranchCode = branchcode, PropertyCode = propertycode, UserCode = usercode, RoleId = roleId });

            return result;
        }

        public async Task<int> InsertUserOperationRightsMaster(List<UserOperationRightsMaster> useroperationright)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                string insertquery = @"
                MERGE Tbl_UserOperationRights_Master AS TARGET

                USING
                (
	                SELECT
	                @UserCode AS UserCode,
                    @UserName AS UserName,
                    @RoleId AS RoleId,
                    @OperationRightId AS OperationRightId,
                    @IsPermission AS IsPermission,
	                @PropertyCode AS PropertyCode,
	                @BranchCode AS BranchCode
        
                ) AS SOURCE

                ON TARGET.UserCode = SOURCE.UserCode
                AND TARGET.OperationRightId = SOURCE.OperationRightId
                AND TARGET.BranchCode = SOURCE.BranchCode
                WHEN MATCHED THEN
                UPDATE SET
                TARGET.IsPermission = SOURCE.IsPermission

                WHEN NOT MATCHED THEN

                INSERT
                (
	                UserCode, UserName, RoleId, OperationRightId, IsPermission, PropertyCode, BranchCode
                )
                VALUES
                (
	                SOURCE.UserCode,
                    SOURCE.UserName,
                    SOURCE.RoleId,
                    SOURCE.OperationRightId,
                    SOURCE.IsPermission,
                    SOURCE.PropertyCode,
                    SOURCE.BranchCode        
                ); ";

                int affectedRows = await connection.ExecuteAsync(insertquery, useroperationright, transaction);

                transaction.Commit();

                return affectedRows;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion
    }
}
