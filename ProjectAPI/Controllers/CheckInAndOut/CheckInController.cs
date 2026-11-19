using FrontOfficeApplication.ProjectEntitiesModel.CheckInAndOut;
using FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectAPI.Controllers.CheckInAndOut
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckInController(ICheckIn_Service checkInService) : BaseController
    {
        private readonly ICheckIn_Service _checkInService = checkInService;

        [Authorize]
        [HttpPost("CreateCheckInDetail")]
        public async Task<IActionResult> CreateCheckInDetail([FromBody] CheckInRequestModel RequestDetail)
        {
            if (RequestDetail == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _checkInService.CreateCheckInDetail(RequestDetail);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }
    }
}
