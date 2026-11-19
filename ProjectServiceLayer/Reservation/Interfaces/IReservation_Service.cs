using FrontOfficeApplication.ProjectEntitiesModel.Reservation;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectServiceLayer.Reservation.Interfaces
{
    public interface IReservation_Service
    {
        #region  ReservationTransaction

        List<string> GetChartAvailability(int nDays, DateTime checkInDate, string PropertyCode, string BranchCode);

        Task<IEnumerable<RoomAvailability>> GetRoomAvailability( int noDays, DateTime checkInDate, string PropertyCode, string BranchCode);

        Task<IEnumerable<TaxDetailsResponseDto>> GetTaxDetails(string PropertyCode, string BranchCode);

        Task<TaxRulesDetailsModel> GetTaxRulesDetails(string BranchCode, string Property);

        Task<IEnumerable<PlanDetailsModel>> GetPlanDetails(string PropertyCode, string BranchCode);

        Task<IEnumerable<RoomTraiffDetailResponseDto>> GetRoomTraiffDetails(string PropertyCode, string BranchCode);

        Task<IEnumerable<CompanyTraiffDetailResponseDto>> GetCompanyTraiffDetails(string PropertyCode, string BranchCode);

        Task<ReservationDetailsCombinedResponseDto> GetCombinedReservationDetails(string PropertyCode, string BranchCode);

        //Task<IEnumerable<ReservationPayModeMasterModel>> GetReservationPayModeMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<string>> CreateReservationDetail(ReservationRequestModel ReservationDetail);

        Task<ServiceResult<int>> BookingCancellationDetail(BookingCancellationModel requestDetail);

        Task<IEnumerable<GuestSearchDetailModel>> GetGuestSearchList(string SearchingName, string PropertyCode, string BranchCode);

        Task<ReservationCompleteDetailsDto> GetReservationCompleteDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode);
        #endregion
    }
}
