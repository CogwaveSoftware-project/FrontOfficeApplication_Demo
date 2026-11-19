using FrontOfficeApplication.ProjectEntitiesModel.CheckInAndOut;
using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;
using FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.DTOs;
using System.Data;
using System.Transactions;

namespace FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.Interfaces
{
    public interface ICheckIn_Repository
    {
        Task<bool> CheckExistingCheckIn(string ReservationNo, string PropertyCode, string BranchCode, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_GuestProcedure(CheckInGuestDetailModel GuestDetail, CheckInRequestModel ChechinDetail, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_FormCProcedure(FormCRequestModel FormCDetail, CheckInRequestModel ChechinDetail, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_GRCBookingProcedure(string NextGRCCode, string PrimaryGuestCode, CheckInRequestModel ChechinDetail, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_CompanyMasterProcedure(CheckInGuestDetailModel GuestDetail, CheckInRequestModel ChechinDetail, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_CheckInProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_CheckInSlaveProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_ForeverDataProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_ChannelManagerProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_AdvanceReceiptProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_TaxDetailsProcedure(TaxBreakdownResponseDto taxDetail, CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_CheckInChargesProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

        Task<CheckInSaveResult> SaveUpdate_SpecialInstructionProcedure(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

        Task<int> InsertDiscountValue(CheckInRequestModel request, IDbConnection connection, IDbTransaction transaction);

    }
}
