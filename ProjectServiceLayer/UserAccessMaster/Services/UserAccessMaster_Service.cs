using FrontOfficeApplication.Helper.Security;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectEntitiesModel.UserAccessMaster;
using FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Services;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.Interfaces;
using System.Globalization;

namespace FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.Services
{
    public class UserAccessMaster_Service : IUserAccessMaster_Service
    {
        private readonly IUserAccessMaster_Repository _useraccessrepository;
        private readonly ILogger<UserAccessMaster_Service> _logger;
        private readonly IDbConnectionFactory _factory;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ProductLicence_Service _productLicenceService;
        private readonly CommonMethod _commonMethod;


        public UserAccessMaster_Service(IUserAccessMaster_Repository userAccessMasterRepository, ILogger<UserAccessMaster_Service> logger, IDbConnectionFactory factory, JwtTokenService jwtTokenService, ProductLicence_Service productLicenceService, CommonMethod commonMethod)
        {
            _useraccessrepository = userAccessMasterRepository;
            _logger = logger;
            _factory = factory;
            _jwtTokenService = jwtTokenService;
            _productLicenceService = productLicenceService;
            _commonMethod = commonMethod;
        }

        #region UserLoginAccess

        public async Task<IEnumerable<BranchDTO>> GetBranchesByMasterUser(LoginRequest request)
        {
            var branches = await _useraccessrepository.GetBranchesByMasterUser(request);

            return branches.Select(x => new BranchDTO
            {
                BranchCode = x.BranchCode,
                BranchName = x.BranchName,
                RoleId = x.RoleId,
                PropertyName = x.PropertyName,
                PropertyCode = x.PropertyCode
            });
        }

        public async Task<ServiceResult<LoginBranchDTO>> GetLoginDetails(LoginBranchRequest request)
        {
            DateTime currentdate = _commonMethod.ConvertUtcToIst();
            string returnMessage = string.Empty;
            bool productValid = false;
            bool SerialKeyExpired = false;
            string SerialKey = string.Empty;

            var userBranch = await _useraccessrepository.GetUserDetails(request);

            if (userBranch == null)
            {
                return new ServiceResult<LoginBranchDTO>
                {
                    Success = false,
                    Message = "User is not authorized for this branch."
                };
            }

            var ProductLicence = await _productLicenceService.GetProductLicenceKey(request.BranchCode);
            var token = _jwtTokenService.GenerateToken(userBranch.UserName, userBranch.BranchCode, userBranch.UserCode.ToString(), userBranch.RoleId.ToString());

            var loginResponse = new LoginBranchDTO
            {
                UserDetails = userBranch,
                JWTToken = token,
                SerialKey = SerialKey,
                SerialKeyExpired = SerialKeyExpired
            };

            if (ProductLicence == null)
            {
                SerialKey = DateTime.Now.ToString("yyyy-MMdd-HHmm-ssff");

                return new ServiceResult<LoginBranchDTO>
                {
                    Success = true,
                    Message = "Product licence not found",
                    Data = new LoginBranchDTO
                    {
                        UserDetails = userBranch,
                        SerialKey = SerialKey,
                        SerialKeyExpired = true
                    }
                };
            }

            int daysRemaining = (ProductLicence.ValidDate.Date - currentdate.Date).Days;
            string decryptSerialkey = await _productLicenceService.Decrypt(ProductLicence.Encryptedserialkey);
            string decodedSerialkey = await _productLicenceService.DecodePass(ProductLicence.SerialKey);
            DateTime decryptValidDate = DateTime.Parse(await _productLicenceService.Decrypt(ProductLicence.EncryptedToDate));
            DateTime decryptIntimationDate = DateTime.Parse(await _productLicenceService.Decrypt(ProductLicence.EncryptedIntimationDate));

            if (decryptSerialkey != decodedSerialkey)
            {
                return new ServiceResult<LoginBranchDTO>
                {
                    Success = false,
                    Message = "Invalid serial key"
                };
            }
            else if (decryptValidDate.Date != ProductLicence.ValidDate.Date)
            {
                return new ServiceResult<LoginBranchDTO>
                {
                    Success = false,
                    Message = "Invalid valid date"
                };
            }
            else if (decryptIntimationDate.Date != ProductLicence.IntimationDate.Date)
            {
                return new ServiceResult<LoginBranchDTO>
                {
                    Success = false,
                    Message = "Invalid intimation date"
                };
            }

            if (currentdate.Date > decryptValidDate.Date)
            {
                productValid = true;
                SerialKey = DateTime.Now.ToString("yyyy-MMdd-HHmm-ssff");
                //SerialKeyExpired = true;
                returnMessage = "Your Period Has Been Expiried, Please Contact Software Vender";
            }
            else if (currentdate.Date > decryptIntimationDate.Date)
            {
                if (daysRemaining <= ProductLicence.IndimationDays)
                {
                    productValid = true;
                    returnMessage = $"Your license will expire in {daysRemaining} day(s). Please contact the software vendor.";
                }
            }

            if (!productValid && daysRemaining > 0)
            {
                return new ServiceResult<LoginBranchDTO>
                {
                    Success = true,
                    Message = "Login successfully",
                    Data = loginResponse
                };
            }
            else if (productValid && daysRemaining >= 0)
            {
                return new ServiceResult<LoginBranchDTO>
                {
                    Success = true,
                    Message = returnMessage,
                    Data = loginResponse
                };
            }
            else
            {
                return new ServiceResult<LoginBranchDTO>
                {
                    Success = true,
                    Message = returnMessage,
                    Data = new LoginBranchDTO
                    {
                        UserDetails = userBranch,
                        SerialKey = SerialKey,
                        SerialKeyExpired = true
                    }
                };
            }
        }

        #endregion


        #region UserAccessMaster

        public async Task<IEnumerable<UserMaster>> GetUserMasterList(string branchcode, string propertycode)
        {
            var userMasters = await _useraccessrepository.GetUserMasterList(branchcode, propertycode);

            return userMasters;
        }

        public async Task<ServiceResult<int>> CreateUserMaster(UserMaster userAccessMaster)
        {

            var result = await _useraccessrepository.CreateUserMaster(userAccessMaster);

            if (result > 0)
            {
                return new ServiceResult<int>
                {
                    Success = true,
                    Message = "User Detail inserted successfully",
                    Data = result
                };
            }
            else if (result == -1)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "Username already exists. Please don't use Same Username.",
                    Data = result
                };
            }
            else
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "Failed to insert User Detail",
                    Data = result
                };
            }
        }

        public async Task<ServiceResult<int>> UpdateUserMaster(UserMaster userAccessMaster)
        {
            var UpdateUser = await _useraccessrepository.UpdateUserMaster(userAccessMaster);
            if (UpdateUser > 0)
            {
                return new ServiceResult<int>
                {
                    Success = true,
                    Message = "User Detail updated successfully",
                    Data = UpdateUser
                };
            }
            else
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "User Detail not found",
                    Data = UpdateUser
                };
            }
        }

        public async Task<ServiceResult<int>> DeleteUserMaster(int UserCode, string PropertyCode, string BranchCode)
        {
            var result = await _useraccessrepository.DeleteUserMaster(UserCode, PropertyCode, BranchCode);
            if (result <= 0)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "User Detail not found",
                    Data = result
                };
            }
            else
            {
                return new ServiceResult<int>
                {
                    Success = true,
                    Message = "User Detail deleted successfully",
                    Data = result
                };
            }
        }

        public async Task<ServiceResult<int>> ChangeUserPassword(int UserCode, string PropertyCode, string BranchCode, string NewUserPassword)
        {
            var result = await _useraccessrepository.ChangeUserPassword(UserCode, PropertyCode, BranchCode, NewUserPassword);
            if (result <= 0)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "User Detail not found",
                    Data = result
                };
            }
            else
            {
                return new ServiceResult<int>
                {
                    Success = true,
                    Message = "User Password Changed successfully",
                    Data = result
                };
            }
        }

        #endregion

        #region Branch, Product And Role Master

        public async Task<IEnumerable<BranchMaster>> GetBranchesList()
        {
            var branches = await _useraccessrepository.GetBranchesList();

            return branches;
        }

        public async Task<IEnumerable<PropertyMaster>> GetPropertiesList()
        {
            var properties = await _useraccessrepository.GetPropertiesList();

            return properties;
        }

        public async Task<IEnumerable<RoleMaster>> GetRoleMasterList()
        {
            var roles = await _useraccessrepository.GetRoleMasterList();

            return roles;
        }

        #endregion

        #region UserPermissionAccessMaster

        public async Task<IEnumerable<MainMenuModel>> GetMainMenuList()
        {
            var MainMenuList = await _useraccessrepository.GetMainMenuList();

            return MainMenuList;
        }

        public async Task<IEnumerable<MainMenuGroupModel>> GetSubMenuList()
        {
            var subMenuList = await _useraccessrepository.GetSubMenuList();

            var groupedList = subMenuList
                .GroupBy(x => x.MainMenuId)
                .Select(g => new MainMenuGroupModel
                {
                    MainMenuId = g.Key,
                    SubMenus = g.ToList()
                })
                .ToList();

            return groupedList;
        }

        public async Task<ServiceResult<int>> SaveMenuWithSubMenu(MenuWithSubMenuModel model)
        {
            var Details = await _useraccessrepository.SaveMenuWithSubMenu(model);
            if (Details > 0)
            {
                return ServiceResult<int>.Created(Details, $"User Permission Access inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(Details, $"Failed to User Permission Access");
            }
        }

        public async Task<ServiceResult<bool>> DeleteMainMenuDetail(int MainMenuId)
        {
            var success = await _useraccessrepository.DeleteMainMenuDetail(MainMenuId);
            if (!success)
            {
                return ServiceResult<bool>.NotFound($"Main Menu not found");
            }
            else
            {
                return ServiceResult<bool>.Ok(success, $"Main Menu Removed successfully");
            }
        }

        public async Task<ServiceResult<bool>> DeleteSubMenuDetail(int SubMenuId)
        {
            var success = await _useraccessrepository.DeleteSubMenuDetail(SubMenuId);
            if (!success)
            {
                return ServiceResult<bool>.NotFound($"Sub Menu not found");
            }
            else
            {
                return ServiceResult<bool>.Ok(success, $"Sub Menu Removed successfully");
            }
        }

        public async Task<UserPermissionResponse> GetUserPermissionAccessList(string branchcode, string propertycode, int usercode, int roleId)
        {
            var data = await _useraccessrepository.GetUserPermissionAccessList(branchcode, propertycode, usercode, roleId);

            var first = data.FirstOrDefault();

            if (first == null)
                return new UserPermissionResponse();

            var result = new UserPermissionResponse
            {
                UserCode = first.UserCode,
                UserName = first.UserName,
                RoleId = first.RoleId,
                RoleName = first.RoleName,
                BranchCode = first.BranchCode,
                PropertyCode = first.PropertyCode,

                Menus = data.GroupBy(x => new { x.MainMenuId, x.MainMenuName, x.MainMenuRequired })
                .Select(menuGroup => new MenuResponse
                {
                    MainMenuId = menuGroup.Key.MainMenuId,
                    MainMenuName = menuGroup.Key.MainMenuName,
                    MainMenuRequired = menuGroup.Key.MainMenuRequired,

                    SubMenus = menuGroup
                    .Select(sub => new SubMenuResponse
                    {
                        SubMenuId = sub.SubMenuId,
                        SubMenuName = sub.SubMenuName,
                        SubMenuRequired = sub.SubMenuRequired,
                        IsPermission = sub.IsPermission,
                        CanView = sub.CanView,
                        CanAdd = sub.CanAdd,
                        CanEdit = sub.CanEdit,
                        CanDelete = sub.CanDelete,
                        CanPrint = sub.CanPrint
                    }).ToList()
                }).ToList()
            };

            return result;
        }

        public async Task<ServiceResult<int>> CreateUserPermissionAccessMaster(List<UserPermissionAccessMaster> userPermission)
        {
            var result = await _useraccessrepository.SaveUserPermissions(userPermission);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"User Permission Access inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to User Permission Access");
            }
        }

        public async Task<IEnumerable<AccessRightsMaster>> GetAccessRightsList()
        {
            var AccessRightsList = await _useraccessrepository.GetAccessRightsList();

            return AccessRightsList;
        }

        public async Task<IEnumerable<OperationRightsMaster>> GetOperationRightsList()
        {
            var OperationRightsList = await _useraccessrepository.GetOperationRightsList();

            return OperationRightsList;
        }

        public async Task<ServiceResult<int>> SaveAccessOROperationRights(RightsMasterRequest requestmodel)
        {
            string responsemessage = string.Empty;

            var Details = await _useraccessrepository.SaveAccessOROperationRights(requestmodel);

            if(requestmodel.AccessType.Equals("AccessRights", StringComparison.OrdinalIgnoreCase))
            {
                responsemessage = "Access Rights inserted successfully";
            }
            else
            {
                responsemessage = "Operation Rights inserted successfully";
            }

            if (Details > 0)
            {
                return ServiceResult<int>.Created(Details, responsemessage);
            }
            else
            {
                return ServiceResult<int>.Fail(Details, $"Failed to Insert Access");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAccessRightsDetail(int AccessRightId)

        {
            var success = await _useraccessrepository.DeleteAccessRightsDetail(AccessRightId);
            if (!success)
            {
                return ServiceResult<bool>.NotFound($"AccessRights not found");
            }
            else
            {
                return ServiceResult<bool>.Ok(success, $"AccessRights Removed successfully");
            }
        }

        public async Task<ServiceResult<bool>> DeleteOperationRightsDetail(int OperationRightId)
        {
            var success = await _useraccessrepository.DeleteOperationRightsDetail(OperationRightId);
            if (!success)
            {
                return ServiceResult<bool>.NotFound($"OperationRights not found");
            }
            else
            {
                return ServiceResult<bool>.Ok(success, $"OperationRights Removed successfully");
            }
        }

        public async Task<UserAccessRightsResponse> GetUserAccessRightsList(string branchcode, string propertycode, int usercode, int roleId)
        {
            var data = await _useraccessrepository.GetUserAccessRightsList(branchcode, propertycode, usercode, roleId);

            var first = data.FirstOrDefault();

            if (first == null)
                return new UserAccessRightsResponse();

            var result = new UserAccessRightsResponse
            {
                UserCode = first.UserCode,
                UserName = first.UserName,
                RoleId = first.RoleId,
                RoleName = first.RoleName,
                BranchCode = first.BranchCode,
                PropertyCode = first.PropertyCode,

                AccessRights = data.Select(x => new AccessRightsResponse
                {
                    AccessRightId = x.AccessRightId,
                    AccessRightName = x.AccessRightName,
                    IsPermission = x.IsPermission
                }).ToList()
            };

            return result;
        }

        public async Task<ServiceResult<int>> InsertUserAccessRightsMaster(List<UserAccessRightsMaster> useraccessright)
        {
            var result = await _useraccessrepository.InsertUserAccessRightsMaster(useraccessright);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"User Access Rights inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to User Access Rights");
            }
        }

        public async Task<UserOperationRightsResponse> GetUserOperationRightsList(string branchcode, string propertycode, int usercode, int roleId)
        {
            var data = await _useraccessrepository.GetUserOperationRightsList(branchcode, propertycode, usercode, roleId);


            var first = data.FirstOrDefault();

            if (first == null)
                return new UserOperationRightsResponse();

            var result = new UserOperationRightsResponse
            {
                UserCode = first.UserCode,
                UserName = first.UserName,
                RoleId = first.RoleId,
                RoleName = first.RoleName,
                BranchCode = first.BranchCode,
                PropertyCode = first.PropertyCode,

                OperationRights = data.Select(x => new OperationRightsResponse
                {
                    OperationRightId = x.OperationRightId,
                    OperationName = x.OperationName,
                    IsPermission = x.IsPermission
                }).ToList()
            };

            return result;
        }

        public async Task<ServiceResult<int>> InsertUserOperationRightsMaster(List<UserOperationRightsMaster> useroperationright)
        {
            var result = await _useraccessrepository.InsertUserOperationRightsMaster(useroperationright);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"User Operation Rights inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to User Operation Rights");
            }
        }

        #endregion


        #region Common Methods

        public DateTime ConvertUtcToIst()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Ensure input is treated as UTC
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            }

            TimeZoneInfo istZone;

            // Handle Windows & Linux
            try
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
        }

        #endregion

    }
}
