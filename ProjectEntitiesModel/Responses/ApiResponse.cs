namespace FrontOfficeApplication.ProjectEntitiesModel.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }
    }
}
