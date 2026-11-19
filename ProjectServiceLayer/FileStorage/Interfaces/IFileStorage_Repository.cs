using FrontOfficeApplication.ProjectEntitiesModel.FileStorage;
using FrontOfficeApplication.ProjectEntitiesModel.Master;

namespace FrontOfficeApplication.ProjectServiceLayer.FileStorage.Interfaces
{
    public interface IFileStorage_Repository
    {
        #region FileStorageaster

        Task<IEnumerable<FileStorageModel>> GetFileStorageList(string TableCode, string BranchCode, string PropertyCode);

        Task<FileStorageModel> GetFileStorageById(Guid FileId, string TableCode, string BranchCode, string PropertyCode);

        Task<int> CreateFileStorageDetail(FileStorageModel filerequest);

        Task<int> UpdateFileStorageDetail(FileStorageModel filerequest);

        Task<int> DeleteFileStorageDetail(Guid FileId, string TableCode, string PropertyCode, string BranchCode);

        #endregion
    }
}
