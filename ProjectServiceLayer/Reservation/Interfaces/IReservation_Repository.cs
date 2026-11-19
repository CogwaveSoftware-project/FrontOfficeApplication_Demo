using FrontOfficeApplication.ProjectEntitiesModel.Reservation;
using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.DTOs;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Data;

namespace FrontOfficeApplication.ProjectServiceLayer.Reservation.Interfaces
{
    public interface IReservation_Repository
    {
        #region ReservationTransaction
        Task<IEnumerable<RoomAllocated>> GetRoomAllocated(string PropertyCode, string BranchCode);

        Task<int> GetOccupiedRooms( string RoomCode, DateTime CheckInDate, DateTime CheckOutDate, string PropertyCode, string BranchCode);

        Task<IEnumerable<TaxDetailsModel>> GetTaxDetails(string PropertyCode, string BranchCode);

        Task<TaxRulesDetailsModel> GetTaxRulesDetails(string BranchCode, string PropertyCode);

        Task<IEnumerable<PlanDetailsModel>> GetPlanDetails(string PropertyCode, string BranchCode);

        Task<IEnumerable<RoomTraiffDetailModel>> GetRoomTraiffDetails(string PropertyCode, string BranchCode);

        Task<IEnumerable<CompanyTraiffDetailModel>> GetCompanyTraiffDetails(string PropertyCode, string BranchCode);

        //Task<IEnumerable<ReservationPayModeMasterModel>> GetReservationPayModeMasterList(string BranchCode, string PropertyCode);

        Task<bool> CheckExistingReservation(string ReservationNo, string PropertyCode, string BranchCode, IDbConnection connection, IDbTransaction transaction);

        Task<ReservationSaveResult> SaveUpdate_GuestProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);

        Task<ReservationSaveResult> SaveUpdate_ReservationProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);

        Task<ReservationSaveResult> SaveUpdate_ReservationSlaveProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);

        Task<ReservationSaveResult> SaveUpdate_ForeverDataProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);
        
        Task<ReservationSaveResult> SaveUpdate_ChannelManagerProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);

        Task<ReservationSaveResult> SaveUpdate_AdvanceReceiptProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);

        Task<ReservationSaveResult> SaveUpdate_TaxDetailsProcedure(TaxBreakdownResponseDto TaxDetail, ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);

        Task<ReservationSaveResult> SaveUpdate_ReservationChargesProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);

        Task<ReservationSaveResult> SaveUpdate_SpecialInstructionProcedure(ReservationRequestModel ReservationDetail, IDbConnection connection, IDbTransaction transaction);

        Task<bool> DeleteMasterRecord(string ReservationNo, string PropertyCode, string BranchCode, IDbConnection connection, IDbTransaction transaction);

        Task<int> DeleteReservation(string ReservationNo, string PropertyCode, string BranchCode, IDbConnection connection, IDbTransaction transaction);

        Task<int> BookingCancellationDetail(BookingCancellationModel requestDetail);

        Task<IEnumerable<GuestSearchDetailModel>> GetGuestSearchList(string SearchingName, string PropertyCode, string BranchCode);

        Task<GuestMaster> GetGuestDetails(string GuestCode, string PropertyCode, string BranchCode);

        Task<ReservationMaster> GetReservationMasterDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode);

        Task<List<ReservationSlave>> GetReservationSlaveDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode);

        Task<List<ForEverData>> GetForEverDataDetails(string ReservationNo, string PropertyCode, string BranchCode);

        Task<List<AdvanceReceipt>> GetAdvanceReceiptDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode);

        Task<List<ReservationCharge>> GetReservationChargeDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode);

        Task<List<SpecialInstruction>> GetSpecialInstructionDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode);

        Task<TaxDetails> GetTaxDetailsDetails(string GuestCode, string PropertyCode, string BranchCode);

        #endregion
    }
}
