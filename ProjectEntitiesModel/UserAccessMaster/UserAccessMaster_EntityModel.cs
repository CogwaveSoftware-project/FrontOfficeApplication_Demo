using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace FrontOfficeApplication.ProjectEntitiesModel.UserAccessMaster
{
    #region UserLoginAccess

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginBranchRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public int RoleId { get; set; } = int.MinValue;
    }

    public class UserMaster 
    {
        public int UserId { get; set; } = 0;
        public int UserCode { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
        public DateTime UserDOB { get; set; } = DateTime.MinValue;
        public string UserAddress { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string UserMail { get; set; } = string.Empty;
        public bool UserStatus { get; set; } = true; 
        public string DiscountPer { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; } = 0;
        public int GracePeriod { get; set; } = 0;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty; 
        public string BranchCode { get; set; } = string.Empty;
        public int RoleId { get; set; } = 0;
    }

    #endregion

    #region Branch, Product And Role Master

    public class BranchMaster 
    {
        public int BranchId { get; set; } 
        public string BranchCode { get; set; } = string.Empty; 
        public string BranchName { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string Phone_number { get; set; } = string.Empty;
        public string Mobile_number { get; set; } = string.Empty;
        public string Fax_number { get; set; } = string.Empty;
        public string Email_id { get; set; } = string.Empty;
        public string Tin_no { get; set; } = string.Empty;
        public string Licence_number { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
    }

    public class PropertyMaster 
    {
        public int PropertyId { get; set; } = 0;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public DateTime StartYear { get; set; } = DateTime.MinValue;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string Phone_number { get; set; } = string.Empty;
        public string Mobile_number { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Owner_Number { get; set; } = string.Empty;
        public string Fax_number { get; set; } = string.Empty;
        public string Email_id { get; set; } = string.Empty;
        public string Tin_no { get; set; } = string.Empty;
        public string Licence_number { get; set; } = string.Empty;
    }

    public class RoleMaster
    {
        public int RoleId { get; set; } = 0;
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    
    }
    #endregion

    #region UserPermissionAccessMaster

    public class MainMenuModel
    {
        public int MainMenuId { get; set; } = 0;
        public string MainMenuName { get; set; } = string.Empty;
        public bool MainMenuRequired { get; set; } = false;
    }

    public class SubMenuModel
    {
        public int SubMenuId { get; set; } = 0;
        public string SubMenuName { get; set; } = string.Empty;
        public bool SubMenuRequired { get; set; } = false;
        public int MainMenuId { get; set; } = 0;
        public int ParentSubMenuId { get; set; } = 0;
    }

    public class MenuWithSubMenuModel
    {
        public int MainMenuId { get; set; } = 0;
        public string MainMenuName { get; set; } = string.Empty;
        public bool MainMenuRequired { get; set; } = false;
        public int SubMenuId { get; set; } = 0;
        public string SubMenuName { get; set; } = string.Empty;
        public bool SubMenuRequired { get; set; } = false;
        public int ParentSubMenuId { get; set; } = 0;

        public bool IsExistingMainMenu { get; set; } = false;
    }

    public class UserPermissionAccessMaster
    {
        public int UserCode { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; } = 0;
        public string RoleName { get; set; } = string.Empty;
        public int MainMenuId { get; set; } = 0;
        public string MainMenuName { get; set; } = string.Empty;
        public bool MainMenuRequired { get; set; } = false;
        public int SubMenuId { get; set; } = 0;
        public string SubMenuName { get; set; } = string.Empty;
        public bool SubMenuRequired { get; set; } = false;
        public bool IsPermission { get; set; } = false;
        public bool CanView { get; set; } = false;
        public bool CanAdd { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanPrint { get; set; } = false;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class AccessRightsMaster
    {
        public int AccessRightId { get; set; } = 0;
        public string AccessRightCode { get; set; } = string.Empty;
        public string AccessRightName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
    }

    public class OperationRightsMaster
    {
        public int OperationRightId { get; set; } = 0;
        public string OperationCode { get; set; } = string.Empty;
        public string OperationName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
    }

    public class RightsMasterRequest
    {
        public string RightsCode { get; set; } = string.Empty;
        public string RightsName { get; set; } = string.Empty;
        public bool IsPermission { get; set; } = false;

        public string AccessType { get; set; } = string.Empty;

    }

    public class UserAccessRightsMaster
    {
        public int UserCode { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; } = 0;
        public string RoleName { get; set; } = string.Empty;
        public int AccessRightId { get; set; } = 0;
        public string AccessRightName { get; set; } = string.Empty;
        public bool IsPermission { get; set; } = false;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class UserOperationRightsMaster
    {
        public int UserCode { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; } = 0;
        public string RoleName { get; set; } = string.Empty;
        public int OperationRightId { get; set; } = 0;
        public string OperationName { get; set; } = string.Empty;
        public bool IsPermission { get; set; } = false;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

}
