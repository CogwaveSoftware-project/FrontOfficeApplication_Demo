using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;

namespace FrontOfficeApplication.ProjectServiceLayer.Master.Interfaces
{
    public interface IMaster_Repository
    {
        #region FloorMaster

        Task<IEnumerable<FloorMasterModel>> GetFloorMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateFloorMasterDetail(FloorMasterRequestModel floormaster);

        Task <int> UpdateFloorMasterDetail(FloorMasterRequestModel floormaster);

        Task<int> DeleteFloorMasterDetail(int FloorId, string FloorCode, string PropertyCode, string BranchCode);

        #endregion

        #region FinancialMaster

        Task<FinancialMasterModel> GetFinancialMasterList(string BranchCode, string PropertyCode);

        Task<int> SaveorUpdateFinancialMasterDetail(FinancialMasterModel financialmaster);

        Task<int> DeleteFinancialMasterDetail(int FinId, string PropertyCode, string BranchCode);

        #endregion

        #region RoomTypeMaster

        Task<IEnumerable<RoomTypeMasterModel>> GetRoomTypeMasterList(string BranchCode, string PropertyCode);

        //Task<IEnumerable<RoomTariffMasterModel>> GetRoomTariffMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateRoomTypeMasterDetail(RoomTypeMasterRequestModel roomtypemaster);

        Task<int> UpdateRoomTypeMasterDetail(RoomTypeMasterRequestModel roomtypemaster);

        Task<int> DeleteRoomTypeMasterDetail(int RoomId, string RoomCode, string PropertyCode, string BranchCode);

        #endregion

        #region PlanMaster

        Task<IEnumerable<PlanMasterModel>> GetPlanMasterList(string BranchCode, string PropertyCode);

        Task<int> CreatePlanMasterDetail(PlanMasterModel planmaster);

        Task<int> UpdatePlanMasterDetail(PlanMasterModel planmaster);

        Task<int> DeletePlanMasterDetail(int PlanId, string PlanCode, string PropertyCode, string BranchCode);

        #endregion

        #region RoomDetailMaster

        Task<IEnumerable<RoomDetailMasterModel>> GetRoomDetailMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateRoomDetailMasterDetail(RoomDetailMasterRequestModel detailmaster);

        Task<int> UpdateRoomDetailMasterDetail(RoomDetailMasterRequestModel detailmaster);

        Task<int> DeleteRoomDetailMasterDetail(int RoomDetailId, string RoomNo, string RoomCode, string FloorCode, string PropertyCode, string BranchCode);

        #endregion

        #region PurposeMaster

        Task<IEnumerable<PurposeMasterModel>> GetPurposeMasterList(string BranchCode, string PropertyCode);

        Task<int> CreatePurposeMasterDetail(PurposeMasterModel purposemaster);

        Task<int> UpdatePurposeMasterDetail(PurposeMasterModel purposemaster);

        Task<int> DeletePurposeMasterDetail(int PurposeId, string PurposeCode, string PropertyCode, string BranchCode);

        #endregion

        #region PaymentMaster

        Task<IEnumerable<PaymentMainModeModel>> GetPaymentMainMasterList(string PropertyCode, string BranchCode);

        Task<IEnumerable<PaymentSubModeModel>> GetPaymentSubMasterList(string PropertyCode, string BranchCode);

        Task<int> SaveMainandSubPaymentMode(PaymentModeRequestModel requestmodel);

        Task<bool> DeleteMainPaymentModeDetail(int MainModeId, string PropertyCode, string BranchCode);

        Task<bool> DeleteSubPaymentModeDetail(int SubModeId, string PropertyCode, string BranchCode);

        #endregion

        #region IdentityproofMaster

        Task<IEnumerable<IdentityproofMasterModel>> GetIdentityproofMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateIdentityproofMasterDetail(IdentityproofMasterModel proofmaster);

        Task<int> UpdateIdentityproofMasterDetail(IdentityproofMasterModel proofmaster);

        Task<int> DeleteIdentityproofMasterDetail(int IdentityproofId, string IdentityproofCode, string PropertyCode, string BranchCode);

        #endregion

        #region TaxMaster

        Task<IEnumerable<TaxMasterModel>> GetTaxMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateTaxMasterDetail(TaxMasterModel taxmaster);

        Task<int> UpdateTaxMasterDetail(TaxMasterModel taxmaster);

        Task<int> DeleteTaxMasterDetail(int TaxId, string TaxCode, string PropertyCode, string BranchCode);

        #endregion

        #region TaxDescriptionMaster

        Task<IEnumerable<TaxDescriptionMasterModel>> GetTaxDescriptionMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateTaxDescriptionMasterDetail(TaxDescriptionMasterModel descriptionmaster);

        Task<int> UpdateTaxDescriptionMasterDetail(TaxDescriptionMasterModel descriptionmaster);

        Task<int> DeleteTaxDescriptionMasterDetail(int TaxDescriptionId, string TaxDescriptionCode, string PropertyCode, string BranchCode);

        #endregion

        #region CompanyMaster

        Task<IEnumerable<CompanyMasterModel>> GetCompanyMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateCompanyMasterDetail(CompanyMasterModel companymaster);

        Task<int> UpdateCompanyMasterDetail(CompanyMasterModel companymaster);

        Task<int> DeleteCompanyMasterDetail(int CompanyId, string CompanyCode, string PropertyCode, string BranchCode);

        #endregion

        #region CompanyTraiffMaster

        Task<IEnumerable<CompanyTraiffMasterModel>> GetCompanyTraiffMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateCompanyTraiffMasterDetail(CompanyTraiffRequestModel cmptraiffmaster);

        Task<int> UpdateCompanyTraiffMasterDetail(CompanyTraiffRequestModel cmptraiffmaster);

        Task<int> DeleteCompanyTraiffMasterDetail(int CompanyTraiffId, string RoomCode, string CompanyCode, string PropertyCode, string BranchCode);

        #endregion

        #region RevenueMaster

        Task<IEnumerable<RevenueMasterModel>> GetRevenueMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateRevenueMasterDetail(RevenueMasterModel revenuemaster);

        Task<int> UpdateRevenueMasterDetail(RevenueMasterModel revenuemaster);

        Task<int> DeleteRevenueMasterDetail(int RevenueId, string RevenueCode, string PropertyCode, string BranchCode);

        #endregion

        #region StewardMaster

        Task<IEnumerable<StewardMasterModel>> GetStewardMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateStewardMasterDetail(StewardMasterModel stewardmaster);

        Task<int> UpdateStewardMasterDetail(StewardMasterModel stewardmaster);

        Task<int> DeleteStewardMasterDetail(int StewardId, string StewardCode, string PropertyCode, string BranchCode);

        #endregion

        #region ItemMaster

        Task<IEnumerable<ItemMasterModel>> GetItemMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateItemMasterDetail(ItemMasterModel itemmaster);

        Task<int> UpdateItemMasterDetail(ItemMasterModel itemmaster);

        Task<int> DeleteItemMasterDetail(int ItemId, string ItemCode, string PropertyCode, string BranchCode);

        #endregion

        #region SegmentMaster

        Task<IEnumerable<SegmentMasterModel>> GetSegmentMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateSegmentMasterDetail(SegmentMasterModel Segmentmaster);

        Task<int> UpdateSegmentMasterDetail(SegmentMasterModel Segmentmaster);

        Task<int> DeleteSegmentMasterDetail(int SegmentId, string SegmentCode, string PropertyCode, string BranchCode);

        #endregion

        #region SubSegmentMaster

        Task<IEnumerable<SubSegmentMasterModel>> GetSubSegmentMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateSubSegmentMasterDetail(SubSegmentMasterModel SubSegmentmaster);

        Task<int> UpdateSubSegmentMasterDetail(SubSegmentMasterModel SubSegmentmaster);

        Task<int> DeleteSubSegmentMasterDetail(int SubSegmentId, string SubSegmentCode, string PropertyCode, string BranchCode);

        #endregion

        #region GuestMaster

        Task<IEnumerable<GuestMasterModel>> GetGuestMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateGuestMasterDetail(GuestMasterModel Guestmaster);

        Task<int> UpdateGuestMasterDetail(GuestMasterModel Guestmaster);

        Task<int> DeleteGuestMasterDetail(int GuestId, string GuestCode, string PropertyCode, string BranchCode);

        #endregion

        #region FormCMaster

        Task<IEnumerable<FormCMasterModel>> GetFormCMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateFormCMasterDetail(FormCRequestModel FormCmaster);

        Task<int> UpdateFormCMasterDetail(FormCRequestModel FormCmaster);

        Task<int> DeleteFormCMasterDetail(int FormCId, string GuestCode, string PropertyCode, string BranchCode);

        #endregion

        #region LedgerMaster

        Task<IEnumerable<LedgerMasterModel>> GetLedgerMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateLedgerMasterDetail(LedgerMasterModel Ledgermaster);

        Task<int> UpdateLedgerMasterDetail(LedgerMasterModel Ledgermaster);

        Task<int> DeleteLedgerMasterDetail(int LedgerId, string LedgerCode, string PropertyCode, string BranchCode);

        #endregion

        #region TaxRulesMaster

        Task<IEnumerable<TaxRulesMasterModel>> GetTaxRulesMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateTaxRulesMasterDetail(TaxRulesMasterModel TaxRulesmaster);

        Task<int> UpdateTaxRulesMasterDetail(TaxRulesMasterModel TaxRulesmaster);

        Task<int> DeleteTaxRulesMasterDetail(int TaxRulesId, string TaxRulesCode, string PropertyCode, string BranchCode);

        #endregion

        #region DriverMaster

        Task<IEnumerable<DriverMasterModel>> GetDriverMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateDriverMasterDetail(DriverMasterModel Drivermaster);

        Task<int> UpdateDriverMasterDetail(DriverMasterModel Drivermaster);

        Task<int> DeleteDriverMasterDetail(int DriverId, string DriverCode, string PropertyCode, string BranchCode);

        #endregion

        #region BookingSourceMaster

        Task<IEnumerable<BookingSourceMasterModel>> GetBookingSourceMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateBookingSourceMasterDetail(BookingSourceMasterModel BookingSourcemaster);

        Task<int> UpdateBookingSourceMasterDetail(BookingSourceMasterModel BookingSourcemaster);

        Task<int> DeleteBookingSourceMasterDetail(int BookingSourceId, string BookingSourceCode, string PropertyCode, string BranchCode);

        #endregion

        #region InstructionDepartmentMaster

        Task<IEnumerable<InstructionDepartmentMasterModel>> GetInstructionDepartmentMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateInstructionDepartmentMasterDetail(InstructionDepartmentMasterModel InstructionDepartmentmaster);

        Task<int> UpdateInstructionDepartmentMasterDetail(InstructionDepartmentMasterModel InstructionDepartmentmaster);

        Task<int> DeleteInstructionDepartmentMasterDetail(int InstructionDepartmentId, string InstructionDepartmentCode, string PropertyCode, string BranchCode);

        #endregion

        #region SalesExecutiveMaster

        Task<IEnumerable<SalesExecutiveMasterModel>> GetSalesExecutiveMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateSalesExecutiveMasterDetail(SalesExecutiveMasterModel SalesExecutivemaster);

        Task<int> UpdateSalesExecutiveMasterDetail(SalesExecutiveMasterModel SalesExecutivemaster);

        Task<int> DeleteSalesExecutiveMasterDetail(int SalesExecutiveId, string SalesExecutiveCode, string PropertyCode, string BranchCode);

        #endregion

        #region ReservationPayModeMaster

        Task<IEnumerable<ReservationPayModeMasterModel>> GetReservationPayModeMasterList(string BranchCode, string PropertyCode);

        Task<int> CreateReservationPayModeMasterDetail(ReservationPayModeMasterModel ReservationPayModemaster);

        Task<int> UpdateReservationPayModeMasterDetail(ReservationPayModeMasterModel ReservationPayModemaster);

        Task<int> DeleteReservationPayModeMasterDetail(int ReservationPayModeId, string ReservationPayModeCode, string PropertyCode, string BranchCode);

        #endregion

        #region Common Method

        //Task<string> GetNextMasterCode(string tableName, string CodeColumn, string Prefix, string PropertyCode, string BranchCode);

        #endregion
    }
}
