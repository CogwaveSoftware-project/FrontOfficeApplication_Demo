using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectServiceLayer.Master.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.Master.Interfaces;
using FrontOfficeApplication.ProjectServiceLayer.Master.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectAPI.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : BaseController
    {
        private readonly IMaster_Service _masterService;

        public MasterController (IMaster_Service masterService)
        {
            _masterService = masterService;
        }

        #region FloorMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetFloorMasterList")]
        public async Task<IActionResult> GetFloorMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetFloorMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Floor Masters found");

                return SuccessResponse(result, "Floor Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateFloorMasterDetail")]
        public async Task<IActionResult> CreateFloorMasterDetail([FromBody] FloorMasterRequestModel floormaster)
        {
            if (floormaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateFloorMasterDetail(floormaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateFloorMasterDetail")]
        public async Task<IActionResult> UpdateFloorMasterDetail([FromBody] FloorMasterRequestModel floormaster)
        {
            if (floormaster == null || floormaster.FloorCode == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateFloorMasterDetail(floormaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteFloorMasterDetail")]
        public async Task<IActionResult> DeleteFloorMasterDetail([FromQuery] int FloorId, [FromQuery] string FloorCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (FloorCode == null)
                return FailResponse("Invalid FloorCode");

            var result = await _masterService.DeleteFloorMasterDetail(FloorId, FloorCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }
        #endregion

        #region FinancialMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetFinancialMasterList")]
        public async Task<IActionResult> GetFinancialMasterList(string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetFinancialMasterList(BranchCode, PropertyCode);

                if (result == null)
                    return NotFoundResponse($"No Financial Masters found");

                return SuccessResponse(result, "Financial Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("SaveorUpdateFinancialMasterDetail")]
        public async Task<IActionResult> SaveorUpdateFinancialMasterDetail([FromBody] FinancialMasterModel financialmaster)
        {
            if (financialmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.SaveorUpdateFinancialMasterDetail(financialmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteFinancialMasterDetail")]
        public async Task<IActionResult> DeleteFinancialMasterDetail([FromQuery] int FinId, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (FinId == 0)
                return FailResponse("Invalid FinId");

            var result = await _masterService.DeleteFinancialMasterDetail(FinId, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region RoomTypeMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetRoomTypeMasterList")]
        public async Task<IActionResult> GetRoomTypeMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetRoomTypeMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Floor Masters found");

                return SuccessResponse(result, "Floor Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateRoomTypeMasterDetail")]
        public async Task<IActionResult> CreateRoomTypeMasterDetail([FromBody] RoomTypeMasterRequestModel roomtypemaster)
        {
            if (roomtypemaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            if (roomtypemaster.RoomPlanType.Equals("Normal", StringComparison.OrdinalIgnoreCase))
            {
                if (roomtypemaster.PaxRates.Count != 1 ||
                    roomtypemaster.PaxRates[0].PlanName != "NORMAL")
                {
                    return BadRequest("Normal room must have NORMAL PAX rates.");
                }
            }

            if (roomtypemaster.RoomPlanType.Equals("Planned", StringComparison.OrdinalIgnoreCase))
            {
                var allowedPlans = roomtypemaster.PlanName
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var plan in roomtypemaster.PaxRates)
                {
                    if (!allowedPlans.Contains(plan.PlanName))
                    {
                        return BadRequest($"Invalid plan: {plan.PlanName}");
                    }
                }
            }

            var result = await _masterService.CreateRoomTypeMasterDetail(roomtypemaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateRoomTypeMasterDetail")]
        public async Task<IActionResult> UpdateRoomTypeMasterDetail([FromBody] RoomTypeMasterRequestModel roomtypemaster)
        {
            if (roomtypemaster == null || roomtypemaster.RoomCode == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateRoomTypeMasterDetail(roomtypemaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteRoomTypeMasterDetail")]
        public async Task<IActionResult> DeleteRoomTypeMasterDetail([FromQuery] int RoomId, [FromQuery] string RoomCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (RoomCode == null)
                return FailResponse("Invalid RoomCode");

            var result = await _masterService.DeleteRoomTypeMasterDetail(RoomId, RoomCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region PlanMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetPlanMasterList")]
        public async Task<IActionResult> GetPlanMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetPlanMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Plan Masters found");

                return SuccessResponse(result, "Plan Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreatePlanMasterDetail")]
        public async Task<IActionResult> CreatePlanMasterDetail([FromBody] PlanMasterModel planmaster)
        {
            if (planmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreatePlanMasterDetail(planmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdatePlanMasterDetail")]
        public async Task<IActionResult> UpdatePlanMasterDetail([FromBody] PlanMasterModel planmaster)
        {
            if (planmaster == null || planmaster.PlanCode == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdatePlanMasterDetail(planmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeletePlanMasterDetail")]
        public async Task<IActionResult> DeletePlanMasterDetail([FromQuery] int PlanId, [FromQuery] string PlanCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (PlanCode == null)
                return FailResponse("Invalid PlanCode");

            var result = await _masterService.DeletePlanMasterDetail(PlanId, PlanCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region RoomDetailMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetRoomDetailMasterList")]
        public async Task<IActionResult> GetRoomDetailMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetRoomDetailMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No RoomDetail Masters found");

                return SuccessResponse(result, "RoomDetail Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateRoomDetailMasterDetail")]
        public async Task<IActionResult> CreateRoomDetailMasterDetail([FromBody] List<RoomDetailMasterRequestModel> detailmaster)
        {
            if (detailmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateRoomDetailMasterDetail(detailmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateRoomDetailMasterDetail")]
        public async Task<IActionResult> UpdateRoomDetailMasterDetail([FromBody] List<RoomDetailMasterRequestModel> detailmaster)
        {
            if (detailmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateRoomDetailMasterDetail(detailmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteRoomDetailMasterDetail")]
        public async Task<IActionResult> DeleteRoomDetailMasterDetail([FromQuery] int RoomDetailId, [FromQuery] string RoomNo, [FromQuery] string RoomCode, [FromQuery] string FloorCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (RoomNo == null)
                return FailResponse("Invalid RoomNo");

            var result = await _masterService.DeleteRoomDetailMasterDetail(RoomDetailId, RoomNo, RoomCode, FloorCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region PurposeMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetPurposeMasterList")]
        public async Task<IActionResult> GetPurposeMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetPurposeMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Purpose Masters found");

                return SuccessResponse(result, "Purpose Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreatePurposeMasterDetail")]
        public async Task<IActionResult> CreatePurposeMasterDetail([FromBody] PurposeMasterModel purposemaster)
        {
            if (purposemaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreatePurposeMasterDetail(purposemaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdatePurposeMasterDetail")]
        public async Task<IActionResult> UpdatePurposeMasterDetail([FromBody] PurposeMasterModel purposemaster)
        {
            if (purposemaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdatePurposeMasterDetail(purposemaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeletePurposeMasterDetail")]
        public async Task<IActionResult> DeletePurposeMasterDetail([FromQuery] int PurposeId, [FromQuery] string PurposeCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (PurposeCode == null)
                return FailResponse("Invalid Purpose");

            var result = await _masterService.DeletePurposeMasterDetail(PurposeId, PurposeCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region PaymentMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetPaymentMainMasterList")]
        public async Task<IActionResult> GetPaymentMainMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetPaymentMainMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No PaymentMain Masters found");

                return SuccessResponse(result, "PaymentMain Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetPaymentSubMasterList")]
        public async Task<IActionResult> GetPaymentSubMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetPaymentSubMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No PaymentSub Masters found");

                return SuccessResponse(result, "PaymentSub Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("SaveMainandSubPaymentMode")]
        public async Task<IActionResult> SaveMainandSubPaymentMode([FromBody] PaymentModeRequestModel requestmodel)
        {
            if (requestmodel == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.SaveMainandSubPaymentMode(requestmodel);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteMainPaymentModeDetail")]
        public async Task<IActionResult> DeleteMainPaymentModeDetail([FromQuery] int MainModeId, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (MainModeId == 0 )
                return FailResponse("Invalid MainMode");

            var result = await _masterService.DeleteMainPaymentModeDetail(MainModeId, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteSubPaymentModeDetail")]
        public async Task<IActionResult> DeleteSubPaymentModeDetail([FromQuery] int SubModeId, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (SubModeId == 0 )
                return FailResponse("Invalid MainMode");

            var result = await _masterService.DeleteSubPaymentModeDetail(SubModeId, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region IdentityproofMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetIdentityproofMasterList")]
        public async Task<IActionResult> GetIdentityproofMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetIdentityproofMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Identityproof Masters found");

                return SuccessResponse(result, "Identityproof Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateIdentityproofMasterDetail")]
        public async Task<IActionResult> CreateIdentityproofMasterDetail([FromBody] IdentityproofMasterModel proofmaster)
        {
            if (proofmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateIdentityproofMasterDetail(proofmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateIdentityproofMasterDetail")]
        public async Task<IActionResult> UpdateIdentityproofMasterDetail([FromBody] IdentityproofMasterModel proofmaster)
        {
            if (proofmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateIdentityproofMasterDetail(proofmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteIdentityproofMasterDetail")]
        public async Task<IActionResult> DeleteIdentityproofMasterDetail([FromQuery] int IdentityproofId, [FromQuery] string IdentityproofCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (IdentityproofCode == null)
                return FailResponse("Invalid Purpose");

            var result = await _masterService.DeleteIdentityproofMasterDetail(IdentityproofId, IdentityproofCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region TaxMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetTaxMasterList")]
        public async Task<IActionResult> GetTaxMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetTaxMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Tax Masters found");

                return SuccessResponse(result, "Tax Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateTaxMasterDetail")]
        public async Task<IActionResult> CreateTaxMasterDetail([FromBody] TaxMasterModel taxmaster)
        {
            if (taxmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateTaxMasterDetail(taxmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateTaxMasterDetail")]
        public async Task<IActionResult> UpdateTaxMasterDetail([FromBody] TaxMasterModel taxmaster)
        {
            if (taxmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateTaxMasterDetail(taxmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteTaxMasterDetail")]
        public async Task<IActionResult> DeleteTaxMasterDetail([FromQuery] int TaxId, [FromQuery] string TaxCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (TaxCode == null)
                return FailResponse("Invalid TaxCode");

            var result = await _masterService.DeleteTaxMasterDetail(TaxId, TaxCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region TaxDescriptionMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetTaxDescriptionMasterList")]
        public async Task<IActionResult> GetTaxDescriptionMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetTaxDescriptionMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No TaxDescription Masters found");

                return SuccessResponse(result, "TaxDescription Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateTaxDescriptionMasterDetail")]
        public async Task<IActionResult> CreateTaxDescriptionMasterDetail([FromBody] TaxDescriptionMasterModel descriptionmaster)
        {
            if (descriptionmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateTaxDescriptionMasterDetail(descriptionmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateTaxDescriptionMasterDetail")]
        public async Task<IActionResult> UpdateTaxDescriptionMasterDetail([FromBody] TaxDescriptionMasterModel descriptionmaster)
        {
            if (descriptionmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateTaxDescriptionMasterDetail(descriptionmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteTaxDescriptionMasterDetail")]
        public async Task<IActionResult> DeleteTaxDescriptionMasterDetail([FromQuery] int TaxDescriptionId, [FromQuery] string TaxDescriptionCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (TaxDescriptionCode == null)
                return FailResponse("Invalid TaxDescriptionCode");

            var result = await _masterService.DeleteTaxDescriptionMasterDetail(TaxDescriptionId, TaxDescriptionCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region CompanyMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetCompanyMasterList")]
        public async Task<IActionResult> GetCompanyMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetCompanyMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Company Masters found");

                return SuccessResponse(result, "Company Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateCompanyMasterDetail")]
        public async Task<IActionResult> CreateCompanyMasterDetail([FromBody] CompanyMasterModel companymaster)
        {
            if (companymaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateCompanyMasterDetail(companymaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateCompanyMasterDetail")]
        public async Task<IActionResult> UpdateCompanyMasterDetail([FromBody] CompanyMasterModel companymaster)
        {
            if (companymaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateCompanyMasterDetail(companymaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteCompanyMasterDetail")]
        public async Task<IActionResult> DeleteCompanyMasterDetail([FromQuery] int CompanyId, [FromQuery] string CompanyCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (CompanyCode == null)
                return FailResponse("Invalid CompanyCode");

            var result = await _masterService.DeleteCompanyMasterDetail(CompanyId, CompanyCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region CompanyTraiffMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetCompanyTraiffMasterList")]
        public async Task<IActionResult> GetCompanyTraiffMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetCompanyTraiffMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No CompanyTraiff Masters found");

                return SuccessResponse(result, "CompanyTraiff Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateCompanyTraiffMasterDetail")]
        public async Task<IActionResult> CreateCompanyTraiffMasterDetail([FromBody] List<CompanyTraiffRequestModel> cmptraiffmaster)
        {
            if (cmptraiffmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateCompanyTraiffMasterDetail(cmptraiffmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateCompanyTraiffMasterDetail")]
        public async Task<IActionResult> UpdateCompanyTraiffMasterDetail([FromBody] List<CompanyTraiffRequestModel> cmptraiffmaster)
        {
            if (cmptraiffmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateCompanyTraiffMasterDetail(cmptraiffmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteCompanyTraiffMasterDetail")]
        public async Task<IActionResult> DeleteCompanyTraiffMasterDetail([FromQuery] int CompanyTraiffId, [FromQuery] string RoomCode, [FromQuery] string CompanyCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (CompanyCode == null)
                return FailResponse("Invalid CompanyCode");

            var result = await _masterService.DeleteCompanyTraiffMasterDetail(CompanyTraiffId, RoomCode, CompanyCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region RevenueMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetRevenueMasterList")]
        public async Task<IActionResult> GetRevenueMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetRevenueMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Revenue Masters found");

                return SuccessResponse(result, "Revenue Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateRevenueMasterDetail")]
        public async Task<IActionResult> CreateRevenueMasterDetail([FromBody] RevenueMasterModel revenuemaster)
        {
            if (revenuemaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateRevenueMasterDetail(revenuemaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateRevenueMasterDetail")]
        public async Task<IActionResult> UpdateRevenueMasterDetail([FromBody] RevenueMasterModel revenuemaster)
        {
            if (revenuemaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateRevenueMasterDetail(revenuemaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteRevenueMasterDetail")]
        public async Task<IActionResult> DeleteRevenueMasterDetail([FromQuery] int RevenueId, [FromQuery] string RevenueCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (RevenueCode == null)
                return FailResponse("Invalid RevenueCode");

            var result = await _masterService.DeleteRevenueMasterDetail(RevenueId, RevenueCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region StewardMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetStewardMasterList")]
        public async Task<IActionResult> GetStewardMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetStewardMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Steward Masters found");

                return SuccessResponse(result, "Steward Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateStewardMasterDetail")]
        public async Task<IActionResult> CreateStewardMasterDetail([FromBody] StewardMasterModel stewardmaster)
        {
            if (stewardmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateStewardMasterDetail(stewardmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateStewardMasterDetail")]
        public async Task<IActionResult> UpdateStewardMasterDetail([FromBody] StewardMasterModel stewardmaster)
        {
            if (stewardmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateStewardMasterDetail(stewardmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteStewardMasterDetail")]
        public async Task<IActionResult> DeleteStewardMasterDetail([FromQuery] int StewardId, [FromQuery] string StewardCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (StewardCode == null)
                return FailResponse("Invalid StewardCode");

            var result = await _masterService.DeleteStewardMasterDetail(StewardId, StewardCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region ItemMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetItemMasterList")]
        public async Task<IActionResult> GetItemMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetItemMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Item Masters found");

                return SuccessResponse(result, "Item Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateItemMasterDetail")]
        public async Task<IActionResult> CreateItemMasterDetail([FromBody] ItemMasterModel itemmaster)
        {
            if (itemmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateItemMasterDetail(itemmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateItemMasterDetail")]
        public async Task<IActionResult> UpdateItemMasterDetail([FromBody] ItemMasterModel itemmaster)
        {
            if (itemmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateItemMasterDetail(itemmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteItemMasterDetail")]
        public async Task<IActionResult> DeleteItemMasterDetail([FromQuery] int ItemId, [FromQuery] string ItemCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (ItemCode == null)
                return FailResponse("Invalid ItemCode");

            var result = await _masterService.DeleteItemMasterDetail(ItemId, ItemCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region SegmentMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetSegmentMasterList")]
        public async Task<IActionResult> GetSegmentMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetSegmentMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Segment Masters found");

                return SuccessResponse(result, "Segment Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateSegmentMasterDetail")]
        public async Task<IActionResult> CreateSegmentMasterDetail([FromBody] SegmentMasterModel Segmentmaster)
        {
            if (Segmentmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateSegmentMasterDetail(Segmentmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateSegmentMasterDetail")]
        public async Task<IActionResult> UpdateSegmentMasterDetail([FromBody] SegmentMasterModel Segmentmaster)
        {
            if (Segmentmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateSegmentMasterDetail(Segmentmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteSegmentMasterDetail")]
        public async Task<IActionResult> DeleteSegmentMasterDetail([FromQuery] int SegmentId, [FromQuery] string SegmentCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (SegmentCode == null)
                return FailResponse("Invalid SegmentCode");

            var result = await _masterService.DeleteSegmentMasterDetail(SegmentId, SegmentCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region SubSegmentMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetSubSegmentMasterList")]
        public async Task<IActionResult> GetSubSegmentMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetSubSegmentMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No SubSegment Masters found");

                return SuccessResponse(result, "SubSegment Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateSubSegmentMasterDetail")]
        public async Task<IActionResult> CreateSubSegmentMasterDetail([FromBody] SubSegmentMasterModel SubSegmentmaster)
        {
            if (SubSegmentmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateSubSegmentMasterDetail(SubSegmentmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateSubSegmentMasterDetail")]
        public async Task<IActionResult> UpdateSubSegmentMasterDetail([FromBody] SubSegmentMasterModel SubSegmentmaster)
        {
            if (SubSegmentmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateSubSegmentMasterDetail(SubSegmentmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteSubSegmentMasterDetail")]
        public async Task<IActionResult> DeleteSubSegmentMasterDetail([FromQuery] int SubSegmentId, [FromQuery] string SubSegmentCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (SubSegmentCode == null)
                return FailResponse("Invalid SubSegmentCode");

            var result = await _masterService.DeleteSubSegmentMasterDetail(SubSegmentId, SubSegmentCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region GuestMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetGuestMasterList")]
        public async Task<IActionResult> GetGuestMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetGuestMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Guest Masters found");

                return SuccessResponse(result, "Guest Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateGuestMasterDetail")]
        public async Task<IActionResult> CreateGuestMasterDetail([FromBody] GuestMasterModel Guestmaster)
        {
            if (Guestmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateGuestMasterDetail(Guestmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateGuestMasterDetail")]
        public async Task<IActionResult> UpdateGuestMasterDetail([FromBody] GuestMasterModel Guestmaster)
        {
            if (Guestmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateGuestMasterDetail(Guestmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteGuestMasterDetail")]
        public async Task<IActionResult> DeleteGuestMasterDetail([FromQuery] int GuestId, [FromQuery] string GuestCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (GuestCode == null)
                return FailResponse("Invalid GuestCode");

            var result = await _masterService.DeleteGuestMasterDetail(GuestId, GuestCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region FormCMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetFormCMasterList")]
        public async Task<IActionResult> GetFormCMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetFormCMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No FormC Masters found");

                return SuccessResponse(result, "FormC Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateFormCMasterDetail")]
        public async Task<IActionResult> CreateFormCMasterDetail([FromBody] FormCRequestModel FormCmaster)
        {
            if (FormCmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateFormCMasterDetail(FormCmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateFormCMasterDetail")]
        public async Task<IActionResult> UpdateFormCMasterDetail([FromBody] FormCRequestModel FormCmaster)
        {
            if (FormCmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateFormCMasterDetail(FormCmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteFormCMasterDetail")]
        public async Task<IActionResult> DeleteFormCMasterDetail([FromQuery] int FormCId, [FromQuery] string GuestCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (GuestCode == null)
                return FailResponse("Invalid GuestCode");

            var result = await _masterService.DeleteFormCMasterDetail(FormCId, GuestCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region LedgerMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetLedgerMasterList")]
        public async Task<IActionResult> GetLedgerMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetLedgerMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Ledger Masters found");

                return SuccessResponse(result, "Ledger Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateLedgerMasterDetail")]
        public async Task<IActionResult> CreateLedgerMasterDetail([FromBody] LedgerMasterModel Ledgermaster)
        {
            if (Ledgermaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateLedgerMasterDetail(Ledgermaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateLedgerMasterDetail")]
        public async Task<IActionResult> UpdateLedgerMasterDetail([FromBody] LedgerMasterModel Ledgermaster)
        {
            if (Ledgermaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateLedgerMasterDetail(Ledgermaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteLedgerMasterDetail")]
        public async Task<IActionResult> DeleteLedgerMasterDetail([FromQuery] int LedgerId, [FromQuery] string LedgerCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (LedgerCode == null)
                return FailResponse("Invalid LedgerCode");

            var result = await _masterService.DeleteLedgerMasterDetail(LedgerId, LedgerCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region TaxRulesMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetTaxRulesMasterList")]
        public async Task<IActionResult> GetTaxRulesMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetTaxRulesMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No TaxRules Masters found");

                return SuccessResponse(result, "TaxRules Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateTaxRulesMasterDetail")]
        public async Task<IActionResult> CreateTaxRulesMasterDetail([FromBody] TaxRulesMasterModel TaxRulesmaster)
        {
            if (TaxRulesmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateTaxRulesMasterDetail(TaxRulesmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateTaxRulesMasterDetail")]
        public async Task<IActionResult> UpdateTaxRulesMasterDetail([FromBody] TaxRulesMasterModel TaxRulesmaster)
        {
            if (TaxRulesmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateTaxRulesMasterDetail(TaxRulesmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteTaxRulesMasterDetail")]
        public async Task<IActionResult> DeleteTaxRulesMasterDetail([FromQuery] int TaxRulesId, [FromQuery] string TaxRulesCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (TaxRulesCode == null)
                return FailResponse("Invalid TaxRulesCode");

            var result = await _masterService.DeleteTaxRulesMasterDetail(TaxRulesId, TaxRulesCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region DriverMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetDriverMasterList")]
        public async Task<IActionResult> GetDriverMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetDriverMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No Driver Masters found");

                return SuccessResponse(result, "Driver Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateDriverMasterDetail")]
        public async Task<IActionResult> CreateDriverMasterDetail([FromBody] DriverMasterModel Drivermaster)
        {
            if (Drivermaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateDriverMasterDetail(Drivermaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateDriverMasterDetail")]
        public async Task<IActionResult> UpdateDriverMasterDetail([FromBody] DriverMasterModel Drivermaster)
        {
            if (Drivermaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateDriverMasterDetail(Drivermaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteDriverMasterDetail")]
        public async Task<IActionResult> DeleteDriverMasterDetail([FromQuery] int DriverId, [FromQuery] string DriverCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (DriverCode == null)
                return FailResponse("Invalid DriverCode");

            var result = await _masterService.DeleteDriverMasterDetail(DriverId, DriverCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region BookingSourceMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetBookingSourceMasterList")]
        public async Task<IActionResult> GetBookingSourceMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetBookingSourceMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No BookingSource Masters found");

                return SuccessResponse(result, "BookingSource Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateBookingSourceMasterDetail")]
        public async Task<IActionResult> CreateBookingSourceMasterDetail([FromBody] BookingSourceMasterModel BookingSourcemaster)
        {
            if (BookingSourcemaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateBookingSourceMasterDetail(BookingSourcemaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateBookingSourceMasterDetail")]
        public async Task<IActionResult> UpdateBookingSourceMasterDetail([FromBody] BookingSourceMasterModel BookingSourcemaster)
        {
            if (BookingSourcemaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateBookingSourceMasterDetail(BookingSourcemaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteBookingSourceMasterDetail")]
        public async Task<IActionResult> DeleteBookingSourceMasterDetail([FromQuery] int BookingSourceId, [FromQuery] string BookingSourceCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (BookingSourceCode == null)
                return FailResponse("Invalid BookingSourceCode");

            var result = await _masterService.DeleteBookingSourceMasterDetail(BookingSourceId, BookingSourceCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region InstructionDepartmentMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetInstructionDepartmentMasterList")]
        public async Task<IActionResult> GetInstructionDepartmentMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetInstructionDepartmentMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No InstructionDepartment Masters found");

                return SuccessResponse(result, "InstructionDepartment Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateInstructionDepartmentMasterDetail")]
        public async Task<IActionResult> CreateInstructionDepartmentMasterDetail([FromBody] InstructionDepartmentMasterModel InstructionDepartmentmaster)
        {
            if (InstructionDepartmentmaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateInstructionDepartmentMasterDetail(InstructionDepartmentmaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateInstructionDepartmentMasterDetail")]
        public async Task<IActionResult> UpdateInstructionDepartmentMasterDetail([FromBody] InstructionDepartmentMasterModel InstructionDepartmentmaster)
        {
            if (InstructionDepartmentmaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateInstructionDepartmentMasterDetail(InstructionDepartmentmaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteInstructionDepartmentMasterDetail")]
        public async Task<IActionResult> DeleteInstructionDepartmentMasterDetail([FromQuery] int InstructionDepartmentId, [FromQuery] string InstructionDepartmentCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (InstructionDepartmentCode == null)
                return FailResponse("Invalid InstructionDepartmentCode");

            var result = await _masterService.DeleteInstructionDepartmentMasterDetail(InstructionDepartmentId, InstructionDepartmentCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region SalesExecutiveMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetSalesExecutiveMasterList")]
        public async Task<IActionResult> GetSalesExecutiveMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetSalesExecutiveMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No SalesExecutive Masters found");

                return SuccessResponse(result, "SalesExecutive Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateSalesExecutiveMasterDetail")]
        public async Task<IActionResult> CreateSalesExecutiveMasterDetail([FromBody] SalesExecutiveMasterModel SalesExecutivemaster)
        {
            if (SalesExecutivemaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateSalesExecutiveMasterDetail(SalesExecutivemaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateSalesExecutiveMasterDetail")]
        public async Task<IActionResult> UpdateSalesExecutiveMasterDetail([FromBody] SalesExecutiveMasterModel SalesExecutivemaster)
        {
            if (SalesExecutivemaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateSalesExecutiveMasterDetail(SalesExecutivemaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteSalesExecutiveMasterDetail")]
        public async Task<IActionResult> DeleteSalesExecutiveMasterDetail([FromQuery] int SalesExecutiveId, [FromQuery] string SalesExecutiveCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (SalesExecutiveCode == null)
                return FailResponse("Invalid SalesExecutiveCode");

            var result = await _masterService.DeleteSalesExecutiveMasterDetail(SalesExecutiveId, SalesExecutiveCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region ReservationPayModeMaster

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetReservationPayModeMasterList")]
        public async Task<IActionResult> GetReservationPayModeMasterList([FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _masterService.GetReservationPayModeMasterList(BranchCode, PropertyCode);

                if (result == null || !result.Any())
                    return NotFoundResponse($"No ReservationPayMode Masters found");

                return SuccessResponse(result, "ReservationPayMode Master Fetched Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPost("CreateReservationPayModeMasterDetail")]
        public async Task<IActionResult> CreateReservationPayModeMasterDetail([FromBody] ReservationPayModeMasterModel ReservationPayModemaster)
        {
            if (ReservationPayModemaster == null)
                return FailResponse("Invalid request payload");

            if (!ModelState.IsValid)
                return FailResponse("Validation failed");

            var result = await _masterService.CreateReservationPayModeMasterDetail(ReservationPayModemaster);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return ServiceResponse(result);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpPut("UpdateReservationPayModeMasterDetail")]
        public async Task<IActionResult> UpdateReservationPayModeMasterDetail([FromBody] ReservationPayModeMasterModel ReservationPayModemaster)
        {
            if (ReservationPayModemaster == null)
                return FailResponse("Invalid data");

            var success = await _masterService.UpdateReservationPayModeMasterDetail(ReservationPayModemaster);

            if (!success.Success)
                return BadRequest(success.Message);

            return ServiceResponse(success);
        }

        //[AllowAnonymous]
        [Authorize]
        [HttpDelete("DeleteReservationPayModeMasterDetail")]
        public async Task<IActionResult> DeleteReservationPayModeMasterDetail([FromQuery] int ReservationPayModeId, [FromQuery] string ReservationPayModeCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            if (ReservationPayModeCode == null)
                return FailResponse("Invalid ReservationPayModeCode");

            var result = await _masterService.DeleteReservationPayModeMasterDetail(ReservationPayModeId, ReservationPayModeCode, PropertyCode, BranchCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }

        #endregion

        #region Common Method

        //[AllowAnonymous]
        [Authorize]
        [HttpGet("GetNextMasterCode")]
        public async Task<IActionResult> GetNextMasterCode([FromQuery] string TableName, [FromQuery] string CodeColumn, [FromQuery] string Prefix, [FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                if (string.IsNullOrWhiteSpace(TableName))
                    return BadRequest(new { message = "Table name is required." });

                var result = await _masterService.GetNextMasterCode(TableName, CodeColumn, Prefix, PropertyCode, BranchCode);

                return ServiceResponse(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while getting next record number.",
                    error = ex.Message
                });
            }
        }
        #endregion
    }
}
