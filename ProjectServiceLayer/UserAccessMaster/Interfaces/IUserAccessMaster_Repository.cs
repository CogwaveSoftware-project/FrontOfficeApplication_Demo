using FrontOfficeApplication.ProjectEntitiesModel.UserAccessMaster;

namespace FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.Interfaces
{
    public interface IUserAccessMaster_Repository
    {
        #region UserAccessMaster

        Task<IEnumerable<UserMaster>> GetBranchesByMasterUser(LoginRequest request);

        Task<UserMaster> GetUserDetails(LoginBranchRequest request);

        #endregion

        #region UserAccessMaster

        Task<IEnumerable<UserMaster>> GetUserMasterList(string branchcode, string propertycode);

        Task<int> CreateUserMaster(UserMaster userAccessMaster);

        Task<int> UpdateUserMaster(UserMaster userAccessMaster);

        Task<int> DeleteUserMaster(int UserCode, string PropertyCode, string BranchCode);

        Task<int> ChangeUserPassword(int UserCode, string PropertyCode, string BranchCode, string NewUserPassword);

        #endregion

        #region Branch, Product And Role Master

        Task<IEnumerable<BranchMaster>> GetBranchesList();

        Task<IEnumerable<PropertyMaster>> GetPropertiesList();

        Task<IEnumerable<RoleMaster>> GetRoleMasterList();

        #endregion


        #region UserPermissionAccessMaster

        Task<IEnumerable<MainMenuModel>> GetMainMenuList();

        Task<IEnumerable<SubMenuModel>> GetSubMenuList();

        Task<int> SaveMenuWithSubMenu(MenuWithSubMenuModel model);

        Task<bool> DeleteMainMenuDetail(int MainMenuId);

        Task<bool> DeleteSubMenuDetail(int SubMenuId);

        Task<IEnumerable<UserPermissionAccessMaster>> GetUserPermissionAccessList(string branchcode, string propertycode, int usercode, int roleId);

        Task<int> SaveUserPermissions(List<UserPermissionAccessMaster> permissions);

        Task<IEnumerable<AccessRightsMaster>> GetAccessRightsList();

        Task<IEnumerable<OperationRightsMaster>> GetOperationRightsList();

        Task<int> SaveAccessOROperationRights(RightsMasterRequest requestmodel);

        Task<bool> DeleteAccessRightsDetail(int AccessRightId);

        Task<bool> DeleteOperationRightsDetail(int OperationRightId);

        Task<IEnumerable<UserAccessRightsMaster>> GetUserAccessRightsList(string branchcode, string propertycode, int usercode, int roleId);

        Task<int> InsertUserAccessRightsMaster(List<UserAccessRightsMaster> useraccessright);

        Task<IEnumerable<UserOperationRightsMaster>> GetUserOperationRightsList(string branchcode, string propertycode, int usercode, int roleId);

        Task<int> InsertUserOperationRightsMaster(List<UserOperationRightsMaster> useroperationright);


        #endregion
    }
}
