using FrontOfficeApplication.ProjectEntitiesModel.CheckInAndOut;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;

namespace FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.Interfaces
{
    public interface ICheckIn_Service
    {
        Task<ServiceResult<string>> CreateCheckInDetail(CheckInRequestModel RequestDetail);
    }
}
