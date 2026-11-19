using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;
using FrontOfficeApplication.ProjectServiceLayer.GST;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.TaxCalculation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectAPI.Controllers.TaxCalculation
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxCalculationController : BaseController
    {
        private readonly TaxCalculation_Service _taxCalculation;

        public TaxCalculationController(TaxCalculation_Service taxCalculation)
        {
            _taxCalculation = taxCalculation;
        }

        //[Authorize]
        //[HttpPost("CalculateAndCategorizeTaxes")]
        //public async Task<IActionResult> CalculateAndCategorizeTaxes([FromBody] TaxCalculationPayloadDto request)
        //{
        //    try
        //    {
        //        var BranchCode = User.FindFirst("Branch_code")?.Value;

        //        if (string.IsNullOrWhiteSpace(BranchCode))
        //            return Unauthorized(new { message = "Branch code is missing from the user claims." });

        //        var result = await _taxCalculation.CalculateAndCategorizeTaxes(request);

        //        if (result == null)
        //            return NotFoundResponse("No TaxCalculation Details Found");

        //        return SuccessResponse(result, "TaxCalculation verifed successful.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return FailResponse(ex.Message);
        //    }
        //}

        [Authorize]
        [HttpPost("CalculateDynamicTaxes")]
        public async Task<IActionResult> CalculateDynamicTaxes([FromBody] TaxCalculationRequestDto request)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _taxCalculation.CalculateDynamicTaxes(request);

                if (result == null)
                    return NotFoundResponse("No TaxCalculation Details Found");

                return SuccessResponse(result, "TaxCalculation verifed successful.");
            }
            catch (Exception ex)
            {
                return FailResponse(ex.Message);
            }
        }
    }
}
