using FrontOfficeApplication.ProjectServiceLayer.GST;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectAPI.Controllers.GST
{
    [Route("api/[controller]")]
    [ApiController]
    public class GSTController : BaseController
    {
        private readonly EInvoiceGST _eInvoiceGST;

        public GSTController(EInvoiceGST eInvoiceGST)
        {
            _eInvoiceGST = eInvoiceGST;
        }

        [Authorize]
        [HttpGet("GSTVerification")]
        public async Task<IActionResult> GSTVerification([FromQuery] string GSTIn, [FromQuery] string PropertyCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GSTIn))
                    return FailResponse("GSTIN is required");

                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _eInvoiceGST.GSTVerification(GSTIn, PropertyCode, BranchCode);

                if (result == null)
                    return NotFoundResponse("No GST Details Found");

                return SuccessResponse(result, "GST verification successful.");
            }
            catch (Exception ex)
            {
                return FailResponse(ex.Message);
            }
        }
    }
}