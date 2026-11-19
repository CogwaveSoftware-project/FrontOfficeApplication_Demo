using FrontOfficeApplication.ProjectEntitiesModel.UserAccessMaster;

namespace FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.DTOs
{
    public class BranchDTO
    {
        public int RoleId { get; set; } = 0;
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
    }

    public class LoginBranchDTO
    {
        public UserMaster UserDetails { get; set; } = new();
        public string JWTToken { get; set; } = string.Empty;
        public string SerialKey { get; set; } = string.Empty;
        public bool SerialKeyExpired { get; set; } = false;
    }

    public class MainMenuGroupModel
    {
        public int MainMenuId { get; set; } = 0;

        public List<SubMenuModel> SubMenus { get; set; } = new();
    }

    public class UserPermissionResponse
    {
        public int UserCode { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; } = 0;
        public string RoleName { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

        public List<MenuResponse> Menus { get; set; } = new();
    }

    public class MenuResponse
    {
        public int MainMenuId { get; set; } = 0;
        public string MainMenuName { get; set; } = string.Empty;
        public bool MainMenuRequired { get; set; } = false;
        public List<SubMenuResponse> SubMenus { get; set; } = new();
    }

    public class SubMenuResponse
    {
        public int SubMenuId { get; set; }
        public string SubMenuName { get; set; } = string.Empty;
        public bool SubMenuRequired { get; set; } = false;
        public bool IsPermission { get; set; }
        public bool CanView { get; set; } = false;
        public bool CanAdd { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanPrint { get; set; } = false;
    }

    public class UserAccessRightsResponse
    {
        public int UserCode { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; } = 0;
        public string RoleName { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public List<AccessRightsResponse> AccessRights { get; set; } = new();

    }

    public class AccessRightsResponse
    {
        public int AccessRightId { get; set; } = 0;
        public string AccessRightName { get; set; } = string.Empty;
        public bool IsPermission { get; set; } = false;
    }

    public class UserOperationRightsResponse 
    {
        public int UserCode { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; } = 0;
        public string RoleName { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

        public List<OperationRightsResponse> OperationRights { get; set; } = new();
    }

    public class OperationRightsResponse
    {
        public int OperationRightId { get; set; } = 0;
        public string OperationName { get; set; } = string.Empty;
        public bool IsPermission { get; set; } = false;
    }

}
