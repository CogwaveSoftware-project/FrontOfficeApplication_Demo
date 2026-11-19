using FrontOfficeApplication.ProjectEntitiesModel.FileStorage;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectServiceLayer.FileStorage.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography.Xml;
//using static System.Net.WebRequestMethods;

namespace FrontOfficeApplication.ProjectServiceLayer.FileStorage.Services
{
    public class FileStorage_Service : IFileStorage_Service
    {
        private readonly IFileStorage_Repository _filerepository;
        private readonly string _rootPath;
        private readonly CommonMethod _commonMethod;

        private const long MaxFileSize =  20 * 1024 * 1024; // 20 MB
        private static readonly HashSet<string>
        AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp",
                ".img",
                ".pdf"
            };

        public FileStorage_Service (IFileStorage_Repository filerepository, IConfiguration configuration, CommonMethod commonMethod)
        {
            _filerepository = filerepository;
            _commonMethod = commonMethod;

            _rootPath = configuration["FileStorage:RootPath"] ?? throw new InvalidOperationException( "FileStorage:RootPath is not configured.");
        }

        #region FileService

        public async Task<IEnumerable<FileStorageModel>> GetFileStorageList(string TableCode, string BranchCode, string PropertyCode)
        {
            var Storagelist = await _filerepository.GetFileStorageList(TableCode, BranchCode, PropertyCode);

            return Storagelist;
        }

        public async Task<(FileStorageModel File, Stream Content)?> GetFileStorageById(Guid FileId, string TableCode, string BranchCode, string PropertyCode)
        {
            var model = await _filerepository.GetFileStorageById(FileId, TableCode, BranchCode, PropertyCode);

            if (model == null)
                return null;

            var stream = await GetAsync(model.StoragePath);

            if (stream == null)
                return null;

            return (model, stream);
        }

        public async Task<ServiceResult<int>> UploadImageFile(FileUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                throw new InvalidOperationException("File is required.");

            if (request.File.Length > MaxFileSize)
                throw new InvalidOperationException("File size cannot exceed 20 MB.");

            var extension = Path.GetExtension(request.File.FileName);

            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("File type is not supported.");

            var fileId = Guid.NewGuid();

            byte[] content;

            if (ImageProcessor.IsImage(request.File.ContentType))
            {
                await using var input = request.File.OpenReadStream();

                content = await ImageProcessor.ProcessImageAsync(input);

                extension = ".jpg";
            }
            else
            {
                await using var input = request.File.OpenReadStream();

                await using var memory = new MemoryStream();

                await input.CopyToAsync(memory);

                content = memory.ToArray();
            }

            var currentdatetime = _commonMethod.ConvertUtcToIst();

            var storedFileName = fileId.ToString("N") + extension.ToLowerInvariant();

            //var year = currentdatetime.Year.ToString();

            //var month = currentdatetime.Month.ToString("00");

            var safeBranch = SanitizePathPart(request.BranchCode);

            var safeReference = SanitizePathPart(request.TableCode);

            var dateFolder = currentdatetime.ToString("dd-MM-yyyy");


            //var relativePath = Path.Combine(safeBranch, safeReference, year, month, storedFileName).Replace('\\', '/');
            var relativePath = Path.Combine(safeBranch, safeReference, dateFolder, storedFileName).Replace('\\', '/');

            await using (var contentStream = new MemoryStream(content))
            {
                await SaveAsync(contentStream, relativePath);
            }

            try
            {
                var model = new FileStorageModel
                {
                    FileId = fileId,
                    ReferenceCode = request.ReferenceCode,
                    ReferenceType = request.ReferenceType,
                    FileCategory = request.FileCategory,
                    OriginalFileName = Path.GetFileName(request.File.FileName),
                    StoredFileName = storedFileName,
                    StorageProvider = "Local",
                    StoragePath = relativePath,
                    ContentType = ImageContentType(extension, request.File.ContentType),
                    FileExtension = extension,
                    FileSize = content.Length,
                    IsFileActive = true,
                    TableCode = request.TableCode,
                    UserCode = request.UserCode,
                    IPAddress = request.IPAddress,
                    PropertyCode = request.PropertyCode,
                    BranchCode = request.BranchCode

                };
                int result = await _filerepository.CreateFileStorageDetail(model);

                if (result > 0)
                {
                    return ServiceResult<int>.Ok(result, $"Stored File Detail Inserted successfully");
                }
                else
                {
                    return ServiceResult<int>.NotFound($"Failed to Inserted Stored File Detail");
                }
            }
            catch
            {
                await DeleteAsync(relativePath);
                throw;
            }
        }

        public async Task<ServiceResult<int>> DeleteImageFileDetail(Guid FileId, string TableCode, string BranchCode, string PropertyCode)
        {
            var model = await _filerepository.GetFileStorageById(FileId, TableCode, BranchCode, PropertyCode);

            if (model == null)
                return ServiceResult<int>.NotFound($"Stored File Detail not found");

            await DeleteAsync(model.StoragePath);

            var result = await _filerepository.DeleteFileStorageDetail(FileId, TableCode, BranchCode, PropertyCode);

            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Stored File Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Stored File Detail not found");
            }
        }

        private static string SanitizePathPart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Invalid path value.");

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }

            return value;
        }

        private static string ImageContentType(string extension, string originalContentType)
        {
            return extension.ToLowerInvariant()
                switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",

                ".png" => "image/png",

                ".webp" => "image/webp",

                _ => originalContentType
            };
        }

        #endregion

        #region FileStorageService

        public async Task SaveAsync( Stream stream, string relativePath)
        {
            var fullPath = GetFullPath(relativePath);

            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStream = new FileStream( fullPath, FileMode.Create, FileAccess.Write, FileShare.None);

            await stream.CopyToAsync(fileStream);
        }

        public Task<Stream?> GetAsync( string relativePath)
        {
            var fullPath = GetFullPath(relativePath);

            if (!File.Exists(fullPath))
                return Task.FromResult<Stream?>(null);

            Stream stream = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return Task.FromResult<Stream?>(stream);
        }

        public Task DeleteAsync( string relativePath)
        {
            var fullPath = GetFullPath(relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string relativePath)
        {
            var fullPath = GetFullPath(relativePath);

            return Task.FromResult(File.Exists(fullPath));
        }

        private string GetFullPath( string relativePath)
        {
            var normalized = relativePath
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar);

            if (Path.IsPathRooted(normalized))
                throw new InvalidOperationException("Absolute paths are not allowed.");

            var fullPath = Path.GetFullPath( Path.Combine( _rootPath, normalized));

            var root = Path.GetFullPath(_rootPath).TrimEnd( Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith( root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid storage path.");
            }

            return fullPath;
        }

        #endregion
    }
}
