using FrontOfficeApplication.ProjectEntitiesModel.ProductLicence;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;

namespace FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Interfaces
{
    public interface IProductLicence_Service
    {
        Task<ProductLicenceModel> GetProductLicenceKey(string branchCode);

        Task<ServiceResult<int>> SaveProductLicenceKey(InsertProductLicenceModel model);

        Task<string> DecodePass(string encptPass);
    }
}
