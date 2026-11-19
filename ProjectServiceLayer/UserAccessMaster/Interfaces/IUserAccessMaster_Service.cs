using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectEntitiesModel.UserAccessMaster;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.Interfaces
{
    public interface IUserAccessMaster_Service
    {

        #region UserLoginAccess

        Task<IEnumerable<BranchDTO>> GetBranchesByMasterUser(LoginRequest request);

        Task<ServiceResult<LoginBranchDTO>> GetLoginDetails(LoginBranchRequest request);

        #endregion


        #region UserAccessMaster

        Task<IEnumerable<UserMaster>> GetUserMasterList(string branchcode, string propertycode);

        Task<ServiceResult<int>> CreateUserMaster(UserMaster userAccessMaster);

        Task<ServiceResult<int>> UpdateUserMaster(UserMaster userAccessMaster);

        Task<ServiceResult<int>> DeleteUserMaster(int UserCode, string PropertyCode, string BranchCode);

        Task<ServiceResult<int>> ChangeUserPassword(int UserCode, string PropertyCode, string BranchCode, string NewUserPassword);

        #endregion



        #region Branch, Product And Role Master

        Task<IEnumerable<BranchMaster>> GetBranchesList();

        Task<IEnumerable<PropertyMaster>> GetPropertiesList();

        Task<IEnumerable<RoleMaster>> GetRoleMasterList();

        #endregion

        #region UserPermissionAccessMaster

        Task<IEnumerable<MainMenuModel>> GetMainMenuList();

        Task<IEnumerable<MainMenuGroupModel>> GetSubMenuList();

        Task<ServiceResult<int>> SaveMenuWithSubMenu(MenuWithSubMenuModel model);

        Task<ServiceResult<bool>> DeleteMainMenuDetail(int MainMenuId);

        Task<ServiceResult<bool>> DeleteSubMenuDetail(int SubMenuId);

        Task<UserPermissionResponse> GetUserPermissionAccessList(string branchcode, string propertycode, int usercode, int roleId);

        Task<ServiceResult<int>> CreateUserPermissionAccessMaster(List<UserPermissionAccessMaster> userPermission);

        Task<IEnumerable<AccessRightsMaster>> GetAccessRightsList();

        Task<IEnumerable<OperationRightsMaster>> GetOperationRightsList();

        Task<ServiceResult<int>> SaveAccessOROperationRights(RightsMasterRequest requestmodel);

        Task<ServiceResult<bool>> DeleteAccessRightsDetail(int AccessRightId);

        Task<ServiceResult<bool>> DeleteOperationRightsDetail(int OperationRightId);

        Task<UserAccessRightsResponse> GetUserAccessRightsList(string branchcode, string propertycode, int usercode, int roleId);

        Task<ServiceResult<int>> InsertUserAccessRightsMaster(List<UserAccessRightsMaster> useraccessright);

        Task<UserOperationRightsResponse> GetUserOperationRightsList(string branchcode, string propertycode, int usercode, int roleId);

        Task<ServiceResult<int>> InsertUserOperationRightsMaster(List<UserOperationRightsMaster> useroperationright);

        #endregion

    }
}
