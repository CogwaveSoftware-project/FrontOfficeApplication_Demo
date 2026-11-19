using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        #region Success Responses

        protected IActionResult SuccessResponse<T>(T data, string message = "Success")
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            });
        }

        protected IActionResult Success(string message = "Success")
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data = null
            });
        }

        protected IActionResult CreatedResponse<T>(T data, string message = "Created successfully")
        {
            return StatusCode(StatusCodes.Status201Created,
                new ApiResponse<T>
                {
                    Success = true,
                    Message = message,
                    Data = data
                });
        }

        #endregion

        #region Failure Responses

        protected IActionResult FailResponse(string message = "Request failed")
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null
            });
        }

        protected IActionResult NotFoundResponse(string message = "Record not found")
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null
            });
        }

        protected IActionResult UnauthorizedResponse(string message = "Unauthorized")
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null
            });
        }

        protected IActionResult ForbiddenResponse(string message = "Access denied")
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
        }

        protected IActionResult InternalServerError(string message = "Internal server error")
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
        }

        #endregion

        #region Services Responses
        protected IActionResult ServiceResponse<T>(ServiceResult<T> result)
        {
            var response = new ApiResponse<T>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data
            };

            return StatusCode(result.StatusCode, response);
        }
        
        #endregion
    }
}
