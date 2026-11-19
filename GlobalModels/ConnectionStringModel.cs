namespace FrontOfficeApplication.GlobalModels
{
    public static class DbNames
    {
        public const string POS = "POS";
        public const string HMS = "HMS";
    }

    public class ClientDatabasemodel
    {
        public string ServerName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
