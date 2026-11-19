namespace FrontOfficeApplication.ProjectInfrastructure.DataBase
{
    public interface ITenantContext
    {
        string BranchCode { get; }
        string UserCode { get; }
        string UserName { get; }
    }
}
