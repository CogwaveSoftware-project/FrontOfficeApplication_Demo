using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.ProductLicence;
using FrontOfficeApplication.ProjectEntitiesModel.UserAccessMaster;
using FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Interfaces;
using Microsoft.AspNetCore.Connections;

namespace FrontOfficeApplication.ProjectInfrastructure.ProductLicence
{
    public class ProductLicence_DAL : IProductLicence_Repository
    {
        private readonly IDbConnectionFactory _factory;

        public ProductLicence_DAL(IDbConnectionFactory connectionFactory)
        {
            _factory = connectionFactory;
        }

        public async Task<ProductLicenceModel> GetProductLicenceKey(string branchCode)
        {
            using var connection = await _factory.CreateConnection(branchCode, DbNames.HMS);


            const string sql = " select SerialKey, ProductKey, TrDate, ValidDate, NoDays, IntimationDate, IndimationDays, ClientName, Status, BranchCode, Encryptedserialkey, EncryptedToDate, EncryptedIntimationDate " +
                " From Tbl_Config_Man WHERE BranchCode = @BranchCode";

            var result = await connection.QueryFirstOrDefaultAsync<ProductLicenceModel>(sql, new { BranchCode = branchCode });
            return result!;
        }

        public async Task<int> SaveProductLicenceKey(InsertProductLicenceModel model, int TotalDays, DateTime IntimationDate, int IndimationDays, string Encodeserialkey, string Encodeproductkey, string Encryptedserialkey, string EncryptedToDate, string EncryptedIntimationDate)
        {
            using var connection = await _factory.CreateConnection(model.BranchCode, DbNames.HMS);

            const string sql = @"
            IF EXISTS (SELECT 1 FROM Tbl_Config_Man WHERE BranchCode = @BranchCode)
            BEGIN
            UPDATE Tbl_Config_Man
            SET
                SerialKey = @serialkey,
                ProductKey = @productkey,
                TrDate = @trdate,
                ValidDate = @validdate,
                NoDays = @nodays,
                IntimationDate = @intimationdate,
                IndimationDays = @indimationdays,
                Status = @status,
                Encryptedserialkey = @encryptedserialkey,
                EncryptedToDate = @encryptedtodate,
                EncryptedIntimationDate = @encryptedIntimationDate
            WHERE ClientName = @clientname And BranchCode = @branchcode;
            END
            ELSE
            BEGIN
            INSERT INTO Tbl_Config_Man
            ( SerialKey, ProductKey, TrDate, ValidDate, NoDays, IntimationDate, IndimationDays, ClientName, Status, BranchCode, Encryptedserialkey, EncryptedToDate, EncryptedIntimationDate )
            VALUES
            ( @serialkey, @productkey, @trdate, @validdate, @nodays, @intimationdate, @indimationdays, @clientname, @status, @branchcode, @encryptedserialkey, @encryptedtodate, @encryptedintimationdate );
            END";

            var parameters = new
            {
                branchcode = model.BranchCode,
                serialkey = Encodeserialkey,
                productkey = Encodeproductkey,
                trdate = model.TrDate.Date,
                validdate = model.ValidDate.Date,
                nodays = TotalDays,
                intimationdate = IntimationDate,
                indimationdays = IndimationDays,
                clientname = model.ClientName,
                status = true,
                encryptedserialkey = Encryptedserialkey,
                encryptedIntimationDate = EncryptedIntimationDate,
                encryptedtodate = EncryptedToDate
            };
            int rowaffected = await connection.ExecuteAsync(sql, parameters);
            return rowaffected;
        }
    }
}
