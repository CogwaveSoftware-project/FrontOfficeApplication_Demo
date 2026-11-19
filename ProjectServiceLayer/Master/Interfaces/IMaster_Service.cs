using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectServiceLayer.Master.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FrontOfficeApplication.ProjectServiceLayer.Master.Interfaces
{
    public interface IMaster_Service
    {
        #region FloorMaster

        Task<IEnumerable<FloorMasterModel>> GetFloorMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateFloorMasterDetail(FloorMasterRequestModel floormaster);

        Task<ServiceResult<int>> UpdateFloorMasterDetail(FloorMasterRequestModel floormaster);

        Task<ServiceResult<int>> DeleteFloorMasterDetail(int FloorId, string FloorCode, string PropertyCode, string BranchCode);

        #endregion

        #region FinancialMaster

        Task<FinancialMasterModel> GetFinancialMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> SaveorUpdateFinancialMasterDetail(FinancialMasterModel financialmaster);

        Task<ServiceResult<int>> DeleteFinancialMasterDetail(int FinId, string PropertyCode, string BranchCode);

        #endregion

        #region RoomTypeMaster

        Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypeMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateRoomTypeMasterDetail(RoomTypeMasterRequestModel roomtypemaster);

        Task<ServiceResult<int>> UpdateRoomTypeMasterDetail(RoomTypeMasterRequestModel roomtypemaster);

        Task<ServiceResult<int>> DeleteRoomTypeMasterDetail(int RoomId, string RoomCode, string PropertyCode, string BranchCode);

        #endregion

        #region PlanMaster

        Task<IEnumerable<PlanMasterModel>> GetPlanMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreatePlanMasterDetail(PlanMasterModel planmaster);

        Task<ServiceResult<int>> UpdatePlanMasterDetail(PlanMasterModel planmaster);

        Task<ServiceResult<int>> DeletePlanMasterDetail(int PlanId, string PlanCode, string PropertyCode, string BranchCode);

        #endregion

        #region RoomDetailMaster

        Task<IEnumerable<RoomDetailMasterModel>> GetRoomDetailMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateRoomDetailMasterDetail(List<RoomDetailMasterRequestModel> detailmaster);

        Task<ServiceResult<int>> UpdateRoomDetailMasterDetail(List<RoomDetailMasterRequestModel> detailmaster);

        Task<ServiceResult<int>> DeleteRoomDetailMasterDetail(int RoomDetailId, string RoomNo, string RoomCode, string FloorCode, string PropertyCode, string BranchCode);

        #endregion

        #region PurposeMaster

        Task<IEnumerable<PurposeMasterModel>> GetPurposeMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreatePurposeMasterDetail(PurposeMasterModel purposemaster);

        Task<ServiceResult<int>> UpdatePurposeMasterDetail(PurposeMasterModel purposemaster);

        Task<ServiceResult<int>> DeletePurposeMasterDetail(int PurposeId, string PurposeCode, string PropertyCode, string BranchCode);

        #endregion

        #region PaymentMaster

        Task<IEnumerable<PaymentMainModeModel>> GetPaymentMainMasterList(string BranchCode, string PropertyCode);

        Task<IEnumerable<MainandSubModeResponseDto>> GetPaymentSubMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> SaveMainandSubPaymentMode(PaymentModeRequestModel requestmodel);

        Task<ServiceResult<bool>> DeleteMainPaymentModeDetail(int MainModeId, string PropertyCode, string BranchCode);

        Task<ServiceResult<bool>> DeleteSubPaymentModeDetail(int SubModeId, string PropertyCode, string BranchCode);

        #endregion

        #region IdentityproofMaster

        Task<IEnumerable<IdentityproofMasterModel>> GetIdentityproofMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateIdentityproofMasterDetail(IdentityproofMasterModel proofmaster);

        Task<ServiceResult<int>> UpdateIdentityproofMasterDetail(IdentityproofMasterModel proofmaster);

        Task<ServiceResult<int>> DeleteIdentityproofMasterDetail(int IdentityproofId, string IdentityproofCode, string PropertyCode, string BranchCode);

        #endregion

        #region TaxMaster

        Task<IEnumerable<TaxMasterModel>> GetTaxMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateTaxMasterDetail(TaxMasterModel taxmaster);

        Task<ServiceResult<int>> UpdateTaxMasterDetail(TaxMasterModel taxmaster);

        Task<ServiceResult<int>> DeleteTaxMasterDetail(int TaxId, string TaxCode, string PropertyCode, string BranchCode);

        #endregion

        #region TaxDescriptionMaster

        Task<IEnumerable<TaxDescriptionMasterModel>> GetTaxDescriptionMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateTaxDescriptionMasterDetail(TaxDescriptionMasterModel descriptionmaster);

        Task<ServiceResult<int>> UpdateTaxDescriptionMasterDetail(TaxDescriptionMasterModel descriptionmaster);

        Task<ServiceResult<int>> DeleteTaxDescriptionMasterDetail(int TaxDescriptionId, string TaxDescriptionCode, string PropertyCode, string BranchCode);

        #endregion

        #region CompanyMaster

        Task<IEnumerable<CompanyMasterModel>> GetCompanyMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateCompanyMasterDetail(CompanyMasterModel descriptionmaster);

        Task<ServiceResult<int>> UpdateCompanyMasterDetail(CompanyMasterModel descriptionmaster);

        Task<ServiceResult<int>> DeleteCompanyMasterDetail(int CompanyId, string CompanyCode, string PropertyCode, string BranchCode);

        #endregion

        #region CompanyTraiffMaster

        Task<IEnumerable<CompanyTraiffResponseDto>> GetCompanyTraiffMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateCompanyTraiffMasterDetail(List<CompanyTraiffRequestModel> cmptraiffmaster);

        Task<ServiceResult<int>> UpdateCompanyTraiffMasterDetail(List<CompanyTraiffRequestModel> cmptraiffmaster);

        Task<ServiceResult<int>> DeleteCompanyTraiffMasterDetail(int CompanyTraiffId, string RoomCode, string CompanyCode, string PropertyCode, string BranchCode);

        #endregion

        #region RevenueMaster

        Task<IEnumerable<RevenueMasterModel>> GetRevenueMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateRevenueMasterDetail(RevenueMasterModel revenuemaster);

        Task<ServiceResult<int>> UpdateRevenueMasterDetail(RevenueMasterModel revenuemaster);

        Task<ServiceResult<int>> DeleteRevenueMasterDetail(int RevenueId, string RevenueCode, string PropertyCode, string BranchCode);

        #endregion

        #region StewardMaster

        Task<IEnumerable<StewardMasterModel>> GetStewardMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateStewardMasterDetail(StewardMasterModel stewardmaster);

        Task<ServiceResult<int>> UpdateStewardMasterDetail(StewardMasterModel stewardmaster);

        Task<ServiceResult<int>> DeleteStewardMasterDetail(int StewardId, string StewardCode, string PropertyCode, string BranchCode);

        #endregion

        #region ItemMaster

        Task<IEnumerable<ItemMasterModel>> GetItemMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateItemMasterDetail(ItemMasterModel itemmaster);

        Task<ServiceResult<int>> UpdateItemMasterDetail(ItemMasterModel itemmaster);

        Task<ServiceResult<int>> DeleteItemMasterDetail(int ItemId, string ItemCode, string PropertyCode, string BranchCode);

        #endregion

        #region SegmentMaster

        Task<IEnumerable<SegmentMasterModel>> GetSegmentMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateSegmentMasterDetail(SegmentMasterModel Segmentmaster);

        Task<ServiceResult<int>> UpdateSegmentMasterDetail(SegmentMasterModel Segmentmaster);

        Task<ServiceResult<int>> DeleteSegmentMasterDetail(int SegmentId, string SegmentCode, string PropertyCode, string BranchCode);

        #endregion

        #region SubSegmentMaster

        Task<IEnumerable<SubSegmentMasterModel>> GetSubSegmentMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateSubSegmentMasterDetail(SubSegmentMasterModel SubSegmentmaster);

        Task<ServiceResult<int>> UpdateSubSegmentMasterDetail(SubSegmentMasterModel SubSegmentmaster);

        Task<ServiceResult<int>> DeleteSubSegmentMasterDetail(int SubSegmentId, string SubSegmentCode, string PropertyCode, string BranchCode);

        #endregion

        #region GuestMaster

        Task<IEnumerable<GuestMasterModel>> GetGuestMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateGuestMasterDetail(GuestMasterModel Guestmaster);

        Task<ServiceResult<int>> UpdateGuestMasterDetail(GuestMasterModel Guestmaster);

        Task<ServiceResult<int>> DeleteGuestMasterDetail(int GuestId, string GuestCode, string PropertyCode, string BranchCode);

        #endregion

        #region FormCMaster

        Task<IEnumerable<FormCResponseDto>> GetFormCMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateFormCMasterDetail(FormCRequestModel FormCmaster);

        Task<ServiceResult<int>> UpdateFormCMasterDetail(FormCRequestModel FormCmaster);

        Task<ServiceResult<int>> DeleteFormCMasterDetail(int FormCId, string GuestCode, string PropertyCode, string BranchCode);

        #endregion

        #region LedgerMaster

        Task<IEnumerable<LedgerMasterModel>> GetLedgerMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateLedgerMasterDetail(LedgerMasterModel Ledgermaster);

        Task<ServiceResult<int>> UpdateLedgerMasterDetail(LedgerMasterModel Ledgermaster);

        Task<ServiceResult<int>> DeleteLedgerMasterDetail(int LedgerId, string LedgerCode, string PropertyCode, string BranchCode);

        #endregion

        #region TaxRulesMaster

        Task<IEnumerable<TaxRulesMasterModel>> GetTaxRulesMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateTaxRulesMasterDetail(TaxRulesMasterModel TaxRulesmaster);

        Task<ServiceResult<int>> UpdateTaxRulesMasterDetail(TaxRulesMasterModel TaxRulesmaster);

        Task<ServiceResult<int>> DeleteTaxRulesMasterDetail(int TaxRulesId, string TaxRulesCode, string PropertyCode, string BranchCode);

        #endregion

        #region DriverMaster

        Task<IEnumerable<DriverMasterModel>> GetDriverMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateDriverMasterDetail(DriverMasterModel Drivermaster);

        Task<ServiceResult<int>> UpdateDriverMasterDetail(DriverMasterModel Drivermaster);

        Task<ServiceResult<int>> DeleteDriverMasterDetail(int DriverId, string DriverCode, string PropertyCode, string BranchCode);

        #endregion

        #region BookingSourceMaster

        Task<IEnumerable<BookingSourceMasterModel>> GetBookingSourceMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateBookingSourceMasterDetail(BookingSourceMasterModel BookingSourcemaster);

        Task<ServiceResult<int>> UpdateBookingSourceMasterDetail(BookingSourceMasterModel BookingSourcemaster);

        Task<ServiceResult<int>> DeleteBookingSourceMasterDetail(int BookingSourceId, string BookingSourceCode, string PropertyCode, string BranchCode);

        #endregion

        #region InstructionDepartmentMaster

        Task<IEnumerable<InstructionDepartmentMasterModel>> GetInstructionDepartmentMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateInstructionDepartmentMasterDetail(InstructionDepartmentMasterModel InstructionDepartmentmaster);

        Task<ServiceResult<int>> UpdateInstructionDepartmentMasterDetail(InstructionDepartmentMasterModel InstructionDepartmentmaster);

        Task<ServiceResult<int>> DeleteInstructionDepartmentMasterDetail(int InstructionDepartmentId, string InstructionDepartmentCode, string PropertyCode, string BranchCode);

        #endregion

        #region SalesExecutiveMaster

        Task<IEnumerable<SalesExecutiveMasterModel>> GetSalesExecutiveMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateSalesExecutiveMasterDetail(SalesExecutiveMasterModel SalesExecutivemaster);

        Task<ServiceResult<int>> UpdateSalesExecutiveMasterDetail(SalesExecutiveMasterModel SalesExecutivemaster);

        Task<ServiceResult<int>> DeleteSalesExecutiveMasterDetail(int SalesExecutiveId, string SalesExecutiveCode, string PropertyCode, string BranchCode);

        #endregion

        #region  ReservationPayModeMaster

        Task<IEnumerable< ReservationPayModeMasterModel>> GetReservationPayModeMasterList(string BranchCode, string PropertyCode);

        Task<ServiceResult<int>> CreateReservationPayModeMasterDetail( ReservationPayModeMasterModel  ReservationPayModemaster);

        Task<ServiceResult<int>> UpdateReservationPayModeMasterDetail( ReservationPayModeMasterModel  ReservationPayModemaster);

        Task<ServiceResult<int>> DeleteReservationPayModeMasterDetail(int  ReservationPayModeId, string  ReservationPayModeCode, string PropertyCode, string BranchCode);

        #endregion

        #region Common Method

        Task<ServiceResult<string>> GetNextMasterCode(string tableName, string CodeColumn, string Prefix, string PropertyCode, string BranchCode);

        #endregion
    }
}
