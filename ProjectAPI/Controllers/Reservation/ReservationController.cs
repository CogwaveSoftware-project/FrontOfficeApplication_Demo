using FrontOfficeApplication.ProjectEntitiesModel.Reservation;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace FrontOfficeApplication.ProjectAPI.Controllers.Reservation
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController(IReservation_Service reservationService) : BaseController
    {
        public readonly IReservation_Service _reservationService = reservationService;

        //public ReservationController(IReservation_Service reservationService)
        //{
        //    _reservationService = reservationService;
        //}

        #region ReservationTransaction

        [HttpGet("GetChartAvailability")]
        public IActionResult GetChartAvailability([FromQuery] int NoDays, [FromQuery] DateTime checkInDate, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = _reservationService.GetChartAvailability(NoDays, checkInDate, PropertyCode, BranchCode);

            if (result == null)
                return NotFoundResponse($"No ChartAvailability found");

            return SuccessResponse(result, "ChartAvailability Fetched Successfully");
        }

        [HttpGet("GetRoomAvailability")]
        public async Task<IActionResult> GetRoomAvailability( [FromQuery] int noDays, [FromQuery] DateTime checkInDate, [FromQuery]string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetRoomAvailability( noDays, checkInDate, PropertyCode, BranchCode);

            if (result == null || !result.Any())
                return NotFoundResponse($"No RoomAvailability found");

            return SuccessResponse(result, "RoomAvailability Fetched Successfully");
        }

        [HttpGet("GetTaxDetails")]
        public async Task<IActionResult> GetTaxDetails( [FromQuery]string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetTaxDetails( PropertyCode, BranchCode);

            if (result == null || !result.Any())
                return NotFoundResponse($"No TaxDetails found");

            return SuccessResponse(result, "TaxDetails Fetched Successfully");
        }

        [HttpGet("GetTaxRulesDetails")]
        public async Task<IActionResult> GetTaxRulesDetails( [FromQuery]string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetTaxRulesDetails( PropertyCode, BranchCode);

            if (result == null)
                return NotFoundResponse($"No TaxRulesDetails found");

            return SuccessResponse(result, "TaxRulesDetails Fetched Successfully");
        }

        [HttpGet("GetPlanDetails")]
        public async Task<IActionResult> GetPlanDetails( [FromQuery]string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetPlanDetails( PropertyCode, BranchCode);

            if (result == null || !result.Any())
                return NotFoundResponse($"No PlanDetails found");

            return SuccessResponse(result, "PlanDetails Fetched Successfully");
        }

        [HttpGet("GetRoomTraiffDetails")]
        public async Task<IActionResult> GetRoomTraiffDetails( [FromQuery]string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetRoomTraiffDetails( PropertyCode, BranchCode);

            if (result == null || !result.Any())
                return NotFoundResponse($"No RoomTraiffDetails found");

            return SuccessResponse(result, "RoomTraiffDetails Fetched Successfully");
        }

        [HttpGet("GetCompanyTraiffDetails")]
        public async Task<IActionResult> GetCompanyTraiffDetails( [FromQuery]string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetCompanyTraiffDetails( PropertyCode, BranchCode);

            if (result == null || !result.Any())
                return NotFoundResponse($"No CompanyTraiff found");

            return SuccessResponse(result, "CompanyTraiff Fetched Successfully");
        }

        [HttpGet("GetCombinedReservationDetails")]
        public async Task<IActionResult> GetCombinedReservationDetails( [FromQuery]string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetCombinedReservationDetails( PropertyCode, BranchCode);

            if (result == null)
                return NotFoundResponse($"No CombinedReservationDetails found");

            return SuccessResponse(result, "CombinedReservationDetails Fetched Successfully");
        }


        //[AllowAnonymous]
        //[Authorize]
        //[HttpGet("GetReservationPayModeMasterList")]
        //public async Task<IActionResult> GetReservationPayModeMasterList([FromQuery] string PropertyCode)
        //{
        //    try
        //    {
        //        var BranchCode = User.FindFirst("Branch_code")?.Value;

        //        if (string.IsNullOrWhiteSpace(BranchCode))
        //            return Unauthorized(new { message = "Branch code is missing from the user claims." });

        //        var result = await _reservationService.GetReservationPayModeMasterList(BranchCode, PropertyCode);

        //        if (result == null || !result.Any())
        //            return NotFoundResponse($"No ReservationPayMode Masters found");

        //        return SuccessResponse(result, "ReservationPayMode Master Fetched Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateReservationDetail")]
        public async Task<IActionResult> CreateReservationDetail([FromBody] ReservationRequestModel ReservationDetail)
        {
            if (ReservationDetail == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _reservationService.CreateReservationDetail(ReservationDetail);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("BookingCancellationDetail")]
        public async Task<IActionResult> BookingCancellationDetail([FromBody] BookingCancellationModel requestDetail)
        {
            if (requestDetail == null)
                return FailResponse("Invalid data");

            var success = await _reservationService.BookingCancellationDetail(requestDetail);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        [HttpGet("GetGuestSearchList")]
        public async Task<IActionResult> GetGuestSearchList([FromQuery] string SearchingName, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetGuestSearchList(SearchingName, PropertyCode, BranchCode);

            if (result == null || !result.Any())
                return NotFoundResponse($"No Guest Name found");

            return SuccessResponse(result, "GuestDetails Fetched Successfully");
        }

        [HttpGet("GetReservationCompleteDetails")]
        public async Task<IActionResult> GetReservationCompleteDetails([FromQuery] string GuestCode, [FromQuery] string ReservationNo, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _reservationService.GetReservationCompleteDetails(GuestCode, ReservationNo, PropertyCode, BranchCode);

            if (result == null)
                return NotFoundResponse($"No ReservationComplete Details found");

            return SuccessResponse(result, "ReservationComplete Details Fetched Successfully");
        }
        #endregion
    }
}
