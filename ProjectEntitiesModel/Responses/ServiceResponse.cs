using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectEntitiesModel.Responses
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public int StatusCode { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "Success")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public static ServiceResult<T> Created(T data, string message = "Created Successfully")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = StatusCodes.Status201Created
            };
        }

        public static ServiceResult<T> Fail(T? data, string message, int statusCode = StatusCodes.Status400BadRequest)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ServiceResult<T> NotFound(string message = "Record not found")
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                StatusCode = StatusCodes.Status200OK
            };
        }

        //public static ServiceResult<T> BadRequest(string message = "Request failed")
        //{
        //    return new ServiceResult<T>
        //    {
        //        Success = false,
        //        Message = message,
        //        Data = data
        //    };
        //}
    }
}
