namespace FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.DTOs
{
    public class CheckInSaveResult
    {
        public int InsertId { get; set; } = 0;
        public string ActionType { get; set; } = string.Empty;
        public int StatusCode { get; set; } = 0;
        public string Message { get; set; } = string.Empty;
    }

}
