using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.FileStorage;
using FrontOfficeApplication.ProjectServiceLayer;
using FrontOfficeApplication.ProjectServiceLayer.FileStorage.Interfaces;
using Microsoft.AspNetCore.Connections;
using System.Data;
using System.Data.Common;
using static System.Net.WebRequestMethods;

namespace FrontOfficeApplication.ProjectInfrastructure.FileStorage
{
    public class FileStorage_DAL : IFileStorage_Repository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly CommonMethod _commonMethod;
        public FileStorage_DAL(IDbConnectionFactory connectionFactory, CommonMethod commonMethod) 
        {
            _factory = connectionFactory;
            _commonMethod = commonMethod;
        }

        #region FileStorage Master

        public async Task<IEnumerable<FileStorageModel>> GetFileStorageList(string TableCode, string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select FileId, ReferenceCode, ReferenceType, FileCategory, OriginalFileName, StoredFileName,
                StorageProvider, StoragePath, ContentType, FileExtension, FileSize, IsFileActive, TableCode,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Image_FileStorage
                Where TableCode = @tablecode And PropertyCode = @propertyCode And BranchCode = @branchCode Order By FileId; ";

            var StorageList = await connection.QueryAsync<FileStorageModel>(selectquery, new { tablecode = TableCode, propertycode = PropertyCode, branchcode = BranchCode });
            return StorageList;
        }

        public async Task<FileStorageModel> GetFileStorageById(Guid FileId, string TableCode, string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select FileId, ReferenceCode, ReferenceType, FileCategory, OriginalFileName, StoredFileName,
                StorageProvider, StoragePath, ContentType, FileExtension, FileSize, IsFileActive, TableCode,
                UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Image_FileStorage
                Where FileId = @fileid And TableCode = @tablecode And PropertyCode = @propertyCode And BranchCode = @branchCode Order By FileId; ";

            var filedetail = await connection.QueryFirstOrDefaultAsync<FileStorageModel>(selectquery, new { fileid = FileId, tablecode = TableCode, propertycode = PropertyCode, branchcode = BranchCode });
            return filedetail ?? throw new KeyNotFoundException($"File with ID {FileId} not found.");
        }


        public async Task<int> CreateFileStorageDetail(FileStorageModel filerequest)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            int newId = 0;

            try
            {
                var insertquery = @"
                INSERT INTO Tbl_Image_FileStorage
                (
                    FileId, ReferenceCode, ReferenceType, FileCategory, OriginalFileName, StoredFileName,
                    StorageProvider, StoragePath, ContentType, FileExtension, FileSize, IsFileActive, TableCode, 
                    UserCode, IPAddress, PropertyCode, BranchCode
                )
                VALUES 
                (
                    @fileid, @referencecode, @referencetype, @filecategory, @originalfilename, @storedfilename,
                    @storageprovider, @storagepath, @contenttype, @fileextension, @filesize, @isfileactive, @tablecode, 
                    @usercode, @ipaddress, @propertycode, @branchcode
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT) AS FileStorageId;";

                newId = await connection.QuerySingleAsync<int>(insertquery, new
                {
                    fileid = filerequest.FileId,
                    referencecode = filerequest.ReferenceCode,
                    referencetype = filerequest.ReferenceType,
                    filecategory = filerequest.FileCategory,
                    originalfilename = filerequest.OriginalFileName,
                    storedfilename = filerequest.StoredFileName,
                    storageprovider = filerequest.StorageProvider,
                    storagepath = filerequest.StoragePath,
                    contenttype = filerequest.ContentType,
                    fileextension = filerequest.FileExtension,
                    filesize = filerequest.FileSize,
                    isfileactive = filerequest.IsFileActive,
                    tablecode = filerequest.TableCode,
                    usercode = filerequest.UserCode,
                    ipaddress = filerequest.IPAddress,
                    propertycode = filerequest.PropertyCode,
                    branchcode = filerequest.BranchCode,
                });

                return newId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> UpdateFileStorageDetail(FileStorageModel filerequest)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                var updatefloorQuery = @"
                UPDATE Tbl_Image_FileStorage
                SET
                ReferenceCode = COALESCE(@referencecode, referencecode),
                ReferenceType = COALESCE(@referencetype, referencetype),
                FileCategory = COALESCE(@filecategory, filecategory),
                OriginalFileName = COALESCE(@originalfilename, originalfilename),
                StoredFileName = COALESCE(@storedfilename, storedfilename),
                StorageProvider = COALESCE(@storageprovider, storageprovider),
                StoragePath = COALESCE(@storagepath, storagepath),
                ContentType = COALESCE(@contenttype, contenttype),
                FileExtension = COALESCE(@fileextension, fileextension),
                FileSize = COALESCE(@filesize, filesize),
                IsFileActive = COALESCE(@isfileactive, isfileactive),
                TableCode = COALESCE(@tablecode, tablecode),
                IPAddress = COALESCE(@ipaddress, ipaddress),
                ModifiedDate = COALESCE(@modifieddate, modifieddate),
                ModifiedBy = COALESCE(@modifiedby, modifiedby)
                WHERE FileId = @fileid And PropertyCode = @propertycode And BranchCode = @branchcode;";

                var rowsAffected = await connection.ExecuteAsync(
                    updatefloorQuery,
                    new
                    {
                        fileid = filerequest.FileId,
                        referencecode = filerequest.ReferenceCode,
                        referencetype = filerequest.ReferenceType,
                        filecategory = filerequest.FileCategory,
                        originalfilename = filerequest.OriginalFileName,
                        storedfilename = filerequest.StoredFileName,
                        storageprovider = filerequest.StorageProvider,
                        storagepath = filerequest.StoragePath,
                        contenttype = filerequest.ContentType,
                        fileextension = filerequest.FileExtension,
                        filesize = filerequest.FileSize,
                        isfileactive = filerequest.IsFileActive,
                        tablecode = filerequest.TableCode,
                        ipaddress = filerequest.IPAddress,
                        propertycode = filerequest.PropertyCode,
                        branchcode = filerequest.BranchCode,
                        modifieddate = _commonMethod.ConvertUtcToIst(),
                        modifiedby = filerequest.UserCode,
                    });

                if (rowsAffected == 0)
                {
                    return 0;
                }

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> DeleteFileStorageDetail(Guid FileId, string TableCode, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            try
            {
                const string deletequery4 = @"DELETE FROM Tbl_Image_FileStorage 
                      WHERE FileId = @fileid And TableCode = @tablecode And PropertyCode = @propertycode And BranchCode = @branchcode";
                var rowsAffected = await connection.ExecuteAsync(deletequery4, new { fileid = FileId, tablecode = TableCode, propertycode = PropertyCode, branchcode = BranchCode });

                return rowsAffected;
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}