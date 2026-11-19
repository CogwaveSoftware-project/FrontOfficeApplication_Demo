using FrontOfficeApplication.ProjectEntitiesModel.FileStorage;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using static System.Net.WebRequestMethods;

namespace FrontOfficeApplication.ProjectServiceLayer.FileStorage.Interfaces
{
    public interface IFileStorage_Service
    {
        Task SaveAsync( Stream stream, string relativePath);

        Task<Stream?> GetAsync( string relativePath);

        Task DeleteAsync( string relativePath);

        Task<bool> ExistsAsync( string relativePath);



        Task<IEnumerable<FileStorageModel>> GetFileStorageList(string TableCode, string BranchCode, string PropertyCode);

        Task<(FileStorageModel File, Stream Content)?> GetFileStorageById(Guid FileId, string TableCode, string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> UploadImageFile(FileUploadRequest request);

        Task<ServiceResult<int>> DeleteImageFileDetail(Guid FileId, string TableCode, string BranchCode, string PropertyCode);
    }
}
