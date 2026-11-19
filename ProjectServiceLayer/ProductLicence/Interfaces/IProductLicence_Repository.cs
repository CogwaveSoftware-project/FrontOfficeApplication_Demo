using FrontOfficeApplication.ProjectEntitiesModel.ProductLicence;

namespace FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Interfaces
{
    public interface IProductLicence_Repository
    {
        Task<ProductLicenceModel> GetProductLicenceKey(string branchCode);

        Task<int> SaveProductLicenceKey(InsertProductLicenceModel model, int totaldays, DateTime intimationdate, int indimationdays, string Encodeserialkey, string Encodeproductkey, string encryptedserialkey, string encryptedToDate, string encryptedIntimationDate);
    }
}
