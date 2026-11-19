using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectEntitiesModel.UserAccessMaster;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectAPI.Controllers.UserAccessMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccessMasterController : BaseController
    {
        private readonly IUserAccessMaster_Service _userAccessService;

        public UserAccessMasterController(IUserAccessMaster_Service userAccessMasterService)
        {
            _userAccessService = userAccessMasterService;
        }

        #region UserLoginAccess

        [AllowAnonymous]
        [HttpGet("GetBranchesByMasterUser")]
        public async Task<IActionResult> GetBranchesByMasterUser([FromQuery] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return FailResponse( "Username and password are required.");
                }

                var result = await _userAccessService.GetBranchesByMasterUser(request);

                if (result == null || !result.Any())
                    return UnauthorizedResponse("Invalid username or password.");

                return SuccessResponse(result, "Login successful. Please select a branch.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("BtnSubmitLogin")]
        public async Task<IActionResult> BtnSubmitLogin([FromBody] LoginBranchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return FailResponse( "Username and password are required.");
                }

                var result = await _userAccessService.GetLoginDetails(request);

                return ServiceResponse(result);

                //if (result == null)
                //    return UnauthorizedResponse("Invalid username or password.");

                //return SuccessResponse(result, "Login successful. Please select a branch.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region UserAccessMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetUserMasterList")]
        public async Task<IActionResult> GetUserMasterList(string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _userAccessService.GetUserMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No user masters found");

                return SuccessResponse(result, "Users Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateUserMaster")]
        public async Task<IActionResult> CreateUserMaster([FromBody] UserMaster useraccessmaster)
        {
            if (useraccessmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            if (string.IsNullOrWhiteSpace(useraccessmaster.UserName))
                return FailResponse("User Name is required");

            var result = await _userAccessService.CreateUserMaster(useraccessmaster);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateUserMaster")]
        public async Task<IActionResult> UpdateUserMaster([FromBody] UserMaster useraccessmaster)
        {
            if (useraccessmaster == null || useraccessmaster.UserCode <= 0)
                return FailResponse("Invalid data");

            var success = await _userAccessService.UpdateUserMaster(useraccessmaster);

            if (!success.Success)
                return NotFoundResponse("User Details Master not found");

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteUserMaster")]
        public async Task<IActionResult> DeleteUserMaster([FromQuery] int UserCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (UserCode <= 0)
                return FailResponse("Invalid UserCode");

            var result = await _userAccessService.DeleteUserMaster(UserCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("ChangeUserPassword")]
        public async Task<IActionResult> ChangeUserPassword([FromQuery] int UserCode, [FromQuery] string PropertyCode, [FromQuery] string NewUserPassword)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (UserCode <= 0)
                return FailResponse("Invalid UserCode");

            var result = await _userAccessService.ChangeUserPassword(UserCode, PropertyCode, BranchCode, NewUserPassword);

            return ServiceResponse(result);
        }

        #endregion

        #region Branch, Product And Role Master

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetBranchesList")]
        public async Task<IActionResult> GetBranchesList()
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });
                
                var result = await _userAccessService.GetBranchesList();

                if (result == null || !result.Any())
                    return NotFoundResponse($"No branches found");

                return SuccessResponse(result, "Branches Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetPropertiesList")]
        public async Task<IActionResult> GetPropertiesList()
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _userAccessService.GetPropertiesList();

                if (result == null || !result.Any())
                    return NotFoundResponse($"No properties found ");

                return SuccessResponse(result, "Properties Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetRoleMasterList")]
        public async Task<IActionResult> GetRoleMasterList()
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _userAccessService.GetRoleMasterList();


                if (result == null || !result.Any())
                    return NotFoundResponse($"No roles found ");

                return SuccessResponse(result, "Role Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region UserPermissionAccessMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetMainMenuList")]
        public async Task<IActionResult> GetMainMenuList()
        {

            var result = await _userAccessService.GetMainMenuList();

            if (result == null || !result.Any())
                return NotFoundResponse($"No Main Menu Details Found");

            return SuccessResponse(result, "Main Menu Details Response Fetched Successfully");
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetSubMenuList")]
        public async Task<IActionResult> GetSubMenuList()
        {
            var result = await _userAccessService.GetSubMenuList();

            if (result == null || !result.Any())
                return NotFoundResponse($"No Sub Menu Details Found");

            return SuccessResponse(result, "Sub Menu Details Response Fetched Successfully");
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("SaveMenuWithSubMenu")]
        public async Task<IActionResult> SaveMenuWithSubMenu([FromBody] MenuWithSubMenuModel requestmodel)
        {
            if (requestmodel == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _userAccessService.SaveMenuWithSubMenu(requestmodel);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteMainMenuDetail")]
        public async Task<IActionResult> DeleteMainMenuDetail([FromQuery] int MainMenuId)
        {
            if (MainMenuId <= 0)
                return FailResponse("Invalid id");

            var result = await _userAccessService.DeleteMainMenuDetail(MainMenuId);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteSubMenuDetail")]
        public async Task<IActionResult> DeleteSubMenuDetail([FromQuery] int SubMenuId)
        {
            if (SubMenuId <= 0)
                return FailResponse("Invalid id");

            var result = await _userAccessService.DeleteSubMenuDetail(SubMenuId);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetUserPermissionAccessList")]
        public async Task<IActionResult> GetUserPermissionAccessList([FromQuery] string PropertyCode, [FromQuery] int UserCode, [FromQuery] int RoleId)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (string.IsNullOrWhiteSpace(PropertyCode))
                return BadRequest("Property code is required");

            var result = await _userAccessService.GetUserPermissionAccessList(BranchCode, PropertyCode, UserCode, RoleId);

            if (result == null)
                return NotFoundResponse("No User Permission Access Details Found");

            return SuccessResponse(result, "User Permission Access Details fetched successfully");
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("InsertUserPermissionAccessMaster")]
        public async Task<IActionResult> InsertUserPermissionAccessMaster([FromBody] List<UserPermissionAccessMaster> userpermission)
        {
            if (userpermission == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            if (userpermission.Any(up => string.IsNullOrWhiteSpace(up.UserName)))
                return FailResponse("User Name is required");

            var result = await _userAccessService.CreateUserPermissionAccessMaster(userpermission);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetAccessRightsList")]
        public async Task<IActionResult> GetAccessRightsList()
        {

            var result = await _userAccessService.GetAccessRightsList();

            if (result == null || !result.Any())
                return NotFoundResponse($"No AccessRights Details Found");

            return SuccessResponse(result, "AccessRights Details Response Fetched Successfully");
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetOperationRightsList")]
        public async Task<IActionResult> GetOperationRightsList()
        {
            var result = await _userAccessService.GetOperationRightsList();

            if (result == null || !result.Any())
                return NotFoundResponse($"No OperationRights Details Found");

            return SuccessResponse(result, "OperationRights Details Response Fetched Successfully");
        }


        //[AllowAnonymous]
        [Authorize]
        [HttpPost("SaveAccessOROperationRights")]
        public async Task<IActionResult> SaveAccessOROperationRights([FromBody] RightsMasterRequest requestmodel)
        {
            if (requestmodel.RightsCode == null && requestmodel.RightsName == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _userAccessService.SaveAccessOROperationRights(requestmodel);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteAccessRightsDetail")]
        public async Task<IActionResult> DeleteAccessRightsDetail([FromQuery] int AccessRightId)
        {
            if (AccessRightId <= 0)
                return FailResponse("Invalid id");

            var result = await _userAccessService.DeleteAccessRightsDetail(AccessRightId);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteOperationRightsDetail")]
        public async Task<IActionResult> DeleteOperationRightsDetail([FromQuery] int OperationRightId)
        {
            if (OperationRightId <= 0)
                return FailResponse("Invalid id");

            var result = await _userAccessService.DeleteOperationRightsDetail(OperationRightId);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetUserAccessRightsList")]
        public async Task<IActionResult> GetUserAccessRightsList([FromQuery] string PropertyCode, [FromQuery] int UserCode, [FromQuery] int RoleId)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (string.IsNullOrWhiteSpace(PropertyCode))
                return BadRequest("Property code is required");

            var result = await _userAccessService.GetUserAccessRightsList(BranchCode, PropertyCode, UserCode, RoleId);

            if (result == null)
                return NotFoundResponse("No UserAccessRights Details Found");

            return SuccessResponse(result, "UserAccessRights Details fetched successfully");
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("InsertUserAccessRightsMaster")]
        public async Task<IActionResult> InsertUserAccessRightsMaster([FromBody] List<UserAccessRightsMaster> useraccessright)
        {
            if (useraccessright == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            if (useraccessright.Any(up => string.IsNullOrWhiteSpace(up.UserName)))
                return FailResponse("User Name is required");

            var result = await _userAccessService.InsertUserAccessRightsMaster(useraccessright);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetUserOperationRightsList")]
        public async Task<IActionResult> GetUserOperationRightsList([FromQuery] string PropertyCode, [FromQuery] int UserCode, [FromQuery] int RoleId)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (string.IsNullOrWhiteSpace(PropertyCode))
                return BadRequest("Property code is required");

            var result = await _userAccessService.GetUserOperationRightsList(BranchCode, PropertyCode, UserCode, RoleId);

            if (result == null)
                return NotFoundResponse("No UserOperationRights Details Found");

            return SuccessResponse(result, "UserOperationRights Details fetched successfully");
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("InsertUserOperationRightsMaster")]
        public async Task<IActionResult> InsertUserOperationRightsMaster([FromBody] List<UserOperationRightsMaster> useroperationright)
        {
            if (useroperationright == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            if (useroperationright.Any(up => string.IsNullOrWhiteSpace(up.UserName)))
                return FailResponse("User Name is required");

            var result = await _userAccessService.InsertUserOperationRightsMaster(useroperationright);

            return ServiceResponse(result);
        }

        #endregion
    }
}
