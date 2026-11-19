using FrontOfficeApplication.ProjectEntitiesModel.FileStorage;
using FrontOfficeApplication.ProjectServiceLayer.FileStorage.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace FrontOfficeApplication.ProjectAPI.Controllers.FileStorage
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileStorageController : BaseController
    {
        public readonly IFileStorage_Service _fileService;

        public FileStorageController (IFileStorage_Service fileService)
        {
            _fileService = fileService;
        }

        [Authorize]
        [HttpGet("GetFileStorageList")]
        public async Task<IActionResult> GetFileStorageList([FromQuery] string TableCode, [FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _fileService.GetFileStorageList(TableCode, BranchCode, PropertyCode);

                if (result == null)
                    return NotFoundResponse($"No FileStorage found");

                return SuccessResponse(result, "FileStorage Fetched Successfully");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("GetFileStorageById")]
        public async Task<IActionResult> GetFileStorageById([FromQuery] Guid FileId, [FromQuery] string TableCode, [FromQuery] string PropertyCode)
        {
            try
            {
                var BranchCode = User.FindFirst("Branch_code")?.Value;

                if (string.IsNullOrWhiteSpace(BranchCode))
                    return Unauthorized(new { message = "Branch code is missing from the user claims." });

                var result = await _fileService.GetFileStorageById(FileId, TableCode, BranchCode, PropertyCode);

                if (result == null)
                    return NotFoundResponse($"No Guest Masters found");

                return File(result.Value.Content, result.Value.File.ContentType ?? "application/octet-stream", enableRangeProcessing: true);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("UploadImageFile")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> UploadImageFile([FromForm] FileUploadRequest request)
        {
            try
            {
                var result = await _fileService.UploadImageFile(request);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                return ServiceResponse(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpGet("{fileId:guid}/download")]
        //public async Task<IActionResult> Download( Guid fileId)
        //{
        //    var result =
        //        await _fileService.GetAsync(
        //            fileId);

        //    if (result == null)
        //        return NotFound();

        //    return File(
        //        result.Value.Content,
        //        result.Value.File.ContentType
        //            ?? "application/octet-stream",
        //        result.Value.File.OriginalFileName,
        //        enableRangeProcessing: true);
        //}

        [Authorize]
        [HttpDelete("DeleteImageFileDetail")]
        public async Task<IActionResult> DeleteImageFileDetail([FromQuery] Guid FileId, [FromQuery] string TableCode, [FromQuery] string PropertyCode)
        {
            var BranchCode = User.FindFirst("Branch_code")?.Value;

            if (string.IsNullOrWhiteSpace(BranchCode))
                return Unauthorized(new { message = "Branch code is missing from the user claims." });

            var result = await _fileService.DeleteImageFileDetail(FileId, TableCode, BranchCode, PropertyCode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return ServiceResponse(result);
        }
    }
}
