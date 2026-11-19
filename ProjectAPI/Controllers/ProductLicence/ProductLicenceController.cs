using FrontOfficeApplication.ProjectEntitiesModel.ProductLicence;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectAPI.Controllers.ProductLicence
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductLicenceController : BaseController
    {
        public readonly IProductLicence_Service _productLicenceService;

        public ProductLicenceController(IProductLicence_Service productLicenceService)
        {
            _productLicenceService = productLicenceService;
        }

        [AllowAnonymous]
        [HttpGet("GetProductLicenceKey")]
        public async Task<IActionResult> GetProductLicenceKey([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return FailResponse("Branch code is required");

            var data = await _productLicenceService.GetProductLicenceKey(branchcode);

            return SuccessResponse(data, "Product licence key fetched successfully");
        }

        [AllowAnonymous]
        [HttpPost("SaveProductLicenceKey")]
        public async Task<IActionResult> SaveProductLicenceKey([FromBody] InsertProductLicenceModel model)
        {
            if (model == null)
                return FailResponse("Invalid request data");

            var result = await _productLicenceService.SaveProductLicenceKey(model);

            return ServiceResponse(result);
        }

        [HttpPost("decode")]
        public async Task<IActionResult> Decode([FromQuery] string EncodedProductkey)
        {
            if (EncodedProductkey == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Encoded product key is required."
                });
            }

            string decodedKey = await _productLicenceService.DecodePass(EncodedProductkey);

            return Ok(new
            {
                success = true,
                message = "Product key decoded successfully",
                decodedKey = decodedKey
            });
        }
    }
}
