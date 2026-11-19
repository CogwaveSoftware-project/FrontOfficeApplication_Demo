namespace FrontOfficeApplication.ProjectEntitiesModel.FileStorage
{
    public class FileStorageModel
    {
        public Guid FileId { get; set; } = Guid.NewGuid();
        public string ReferenceCode { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public string FileCategory { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string StorageProvider { get; set; } = "Local";
        public string StoragePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSize { get; set; } = long.MinValue;
        public bool IsFileActive { get; set; } = false;
        public string TableCode { get; set; } = string.Empty;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class FileUploadRequest
    {
        public string ReferenceCode { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public string FileCategory { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
        public string TableCode { get; set; } = string.Empty;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }
}
