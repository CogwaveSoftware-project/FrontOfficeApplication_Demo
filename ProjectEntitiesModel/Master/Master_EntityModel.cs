using FrontOfficeApplication.ProjectServiceLayer.Master.DTOs;

namespace FrontOfficeApplication.ProjectEntitiesModel.Master
{
    #region FloorMaster

    public class FloorMasterModel
    {
        public int FloorId { get; set; } = 0;
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public string FloorDescription { get; set; } = string.Empty;
        public bool FloorStatus { get; set; } = false;
        public int MaxRoom { get; set; } = 0;
        public int Allocated { get; set; } = 0;
        public int UnAllocated { get; set; } = 0;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class FloorMasterRequestModel
    {
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public string FloorDescription { get; set; } = string.Empty;
        public bool FloorStatus { get; set; } = false;
        public int MaxRoom { get; set; } = 0;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion


    #region Financial Master

    public class FinancialMasterModel
    {
        public int FinId { get; set; } = 0;
        public DateTime FinFromDate { get; set; } = DateTime.MinValue;
        public DateTime FinToDate { get; set; } = DateTime.MinValue;
        public int FinCurrentYear { get; set; } = 0;
        public int FinNewYear { get; set; } = 0;
        public bool CurrentStatus { get; set; } = false;
        public string BackUpTook { get; set; } = string.Empty;
        public string FinalClose { get; set; } = string.Empty;
        public int FinCode { get; set; } = 0;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region RoomTypeMaster

    public class RoomTypeMasterModel
    {
        public int RoomId { get; set; } = 0;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string RoomDescription { get; set; } = string.Empty;
        public string RoomMode { get; set; } = string.Empty;
        public bool IsRoomActive { get; set; } = false;
        public int TotalRooms { get; set; } = 0;
        public int Allocated { get; set; } = 0;
        public int UnAllocated { get; set; } = 0;
        public string RoomPlanType { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string ChannelManagerCode { get; set; } = string.Empty;
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int Pax { get; set; } = 0;
        public string PaxRates { get; set; } = string.Empty;
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public bool Taxinclude { get; set; } = false;
        public bool Package { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    //public class RoomTariffMasterModel
    //{
    //    public int RoomTariffId { get; set; } = 0;
    //    public string RoomCode { get; set; } = string.Empty;
    //    public decimal DefaultTraiff { get; set; } = decimal.Zero;
    //    public int Pax { get; set; } = 0;
    //    public string PaxRates { get; set; } = string.Empty;
    //    public decimal ExtraAdult { get; set; } = decimal.Zero;
    //    public decimal ExtraChild { get; set; } = decimal.Zero;
    //    public bool TariffStatus { get; set; } = false;
    //    public bool Taxinclude { get; set; } = false;
    //    public bool Package { get; set; } = false;
    //    public int UserCode { get; set; } = 0;
    //    public string IPAddress { get; set; } = string.Empty;
    //    public string PropertyCode { get; set; } = string.Empty;
    //    public string BranchCode { get; set; } = string.Empty;
    //}

    public class RoomTypeMasterRequestModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string RoomDescription { get; set; } = string.Empty;
        public string RoomMode { get; set; } = string.Empty;
        public bool IsRoomActive { get; set; } = false;
        public int TotalRooms { get; set; } = 0;
        public string RoomPlanType { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string ChannelManagerCode { get; set; } = string.Empty;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int Pax { get; set; } = 0;
        //public string PaxRates { get; set; } = string.Empty;
        public List<PlanRateDto> PaxRates { get; set; } = [];
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public bool Taxinclude { get; set; } = false;
        public bool Package { get; set; } = false;
    }

    public class PaxRateDto
    {
        public int PaxNo { get; set; }
        public decimal Rate { get; set; }
    }

    public class PlanRateDto
    {
        public int PlanId { get; set; } = 0;
        public string PlanName { get; set; } = string.Empty;
        public List<PaxRateDto> PaxRates { get; set; } = [];
    }

    #endregion

    #region PlanMaster

    public class PlanMasterModel
    {
        public int PlanId { get; set; } = 0;
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string PlanDescription { get; set; } = string.Empty;
        public bool IsPlanActive { get; set; } = false;
        public bool IsDefaultPlan { get; set; } = false;
        public decimal FoodRate { get; set; } = decimal.Zero;
        public string TaxCode { get; set; } = string.Empty;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region RoomDetailMaster

    public class RoomDetailMasterModel
    {
        public int RoomDetailId { get; set; } = 0;
        public string RoomNo { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomDetailDescription { get; set; } = string.Empty;
        public bool IsRoomDetailActive { get; set; } = false;
        public int DefaultPax { get; set; } = 0;
        public char Priority { get; set; } = 'A';
        public string FloorCode { get; set; } = string.Empty;
        public char RoomStatus { get; set; } = 'A';
        public string CurrentCheckIn { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public string CheckInBillNo { get; set; } = string.Empty;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class RoomDetailMasterRequestModel
    {
        public int RoomDetailId { get; set; } = 0;
        public string RoomNo { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomDetailDescription { get; set; } = string.Empty;
        public bool IsRoomDetailActive { get; set; } = false;
        public int DefaultPax { get; set; } = 0;
        public char Priority { get; set; } = 'A';
        public string FloorCode { get; set; } = string.Empty;
        public char RoomStatus { get; set; } = 'A';
        public string CurrentCheckIn { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public string CheckInBillNo { get; set; } = string.Empty;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

        public string OldRoomNo { get; set; } = string.Empty;
        public string OldRoomCode { get; set; } = string.Empty;
        public string OldFloorCode { get; set; } = string.Empty;
    }
    #endregion

    #region RoomDetailMaster

    public class PurposeMasterModel
    {
        public int PurposeId { get; set; } = 0;
        public string PurposeCode { get; set; } = string.Empty;
        public string PurposeName { get; set; } = string.Empty;
        public bool IsPurposeActive { get; set; } = false;  
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region PaymentMaster

    public class PaymentMainModeModel
    {
        public int MainModeId { get; set; } = 0;
        public string MainModeType { get; set; } = string.Empty;
        public bool IsMainModeActive { get; set; } = false;  
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class PaymentSubModeModel
    {
        public int SubModeId { get; set; } = 0;
        public string SubModeType { get; set; } = string.Empty;
        public bool IsSubModeActive { get; set; } = false;  
        public int MainModeId { get; set; } = 0;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class PaymentModeRequestModel
    {
        public int MainModeId { get; set; } = 0;
        public string MainModeType { get; set; } = string.Empty;
        public bool IsMainModeActive { get; set; } = false;
        public int SubModeId { get; set; } = 0;
        public string SubModeType { get; set; } = string.Empty;
        public bool IsSubModeActive { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public bool IsExistingMainMode { get; set; } = false;
    }

    #endregion

    #region  IdentityproofMaster

    public class IdentityproofMasterModel
    {
        public int IdentityproofId { get; set; } = 0;
        public string IdentityproofCode { get; set; } = string.Empty;
        public string IdentityproofName { get; set; } = string.Empty;
        public bool IsIdentityproofActive { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region  TaxMaster

    public class TaxMasterModel
    {
        public int TaxId { get; set; } = 0;
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public bool IsTaxActive { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class TaxDescriptionMasterModel
    {
        public int TaxDescriptionId { get; set; } = 0;
        public string TaxDescriptionCode { get; set; } = string.Empty;
        public string TaxDescriptionName { get; set; } = string.Empty;
        public decimal TaxPercentage { get; set; } = 0;
        public bool IsTaxDescriptionActive { get; set; } = false;
        public string TaxCode { get; set; } = string.Empty;
        public decimal FromAmount { get; set; } = 0;
        public decimal ToAmount { get; set; } = 0;
        public DateTime FromDate { get; set; } = DateTime.MinValue;
        public DateTime ToDate { get; set; } = DateTime.MinValue;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region CompanyMaster
    public class CompanyMasterModel
    {
        public int CompanyId { get; set; } = 0;
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyType { get; set; } = string.Empty;
        public string CompanyContactPerson { get; set; } = string.Empty;
        public string CompanyMobileNo { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyCity { get; set; } = string.Empty;
        public string CompanyState { get; set; } = string.Empty;
        public string CompanyNation { get; set; } = string.Empty;
        public string CompanyPincode { get; set; } = string.Empty;
        public bool IsCompanyActive { get; set; } = true;
        public string CompanyGSTNo { get; set; } = string.Empty;
        public string CompanyGSTType { get; set; } = string.Empty;
        public string CompanyGSTStateCode { get; set; } = string.Empty;
        public decimal CompanyOpeningBalance { get; set; } = 0;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region CompanyTraiffMaster

    public class CompanyTraiffMasterModel
    {
        public int CompanyTraiffId { get; set; } = 0;
        public string CompanyCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string TraiffCategory { get; set; } = string.Empty;
        public string RoomPlanType { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int Pax { get; set; } = 0;
        public string PaxRates { get; set; } = string.Empty;
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public bool TaxInclude { get; set; } = false;
        public bool ServiceTax { get; set; } = false;
        public bool Package { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class CompanyTraiffRequestModel
    {
        public int CompanyTraiffId { get; set; } = 0;
        public string CompanyCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string TraiffCategory { get; set; } = string.Empty;
        public string RoomPlanType { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int Pax { get; set; } = 0;
        public List<PlanRateDto> PaxRates { get; set; } = [];
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public bool TaxInclude { get; set; } = false;
        public bool ServiceTax { get; set; } = false;
        public bool Package { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region RevenueMaster
    public class RevenueMasterModel
    {
        public int RevenueId { get; set; } = 0;
        public string RevenueCode { get; set; } = string.Empty;
        public string RevenueName { get; set; } = string.Empty;
        public bool IsRevenueActive { get; set; } = true;
        public string TaxCode { get; set; } = string.Empty;
        public string SACCode { get; set; } = string.Empty;
        public bool IsHouseKeeping { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region StewardMaster
    public class StewardMasterModel
    {
        public int StewardId { get; set; } = 0;
        public string StewardCode { get; set; } = string.Empty;
        public string StewardName { get; set; } = string.Empty;
        public string StewardDOB { get; set; } = string.Empty;
        public string StewardAddress { get; set; } = string.Empty;
        public string StewardMobileNo { get; set; } = string.Empty;
        public string StewardEmail { get; set; } = string.Empty;
        public string StewardCity { get; set; } = string.Empty;
        public string StewardState { get; set; } = string.Empty;
        public string StewardNation { get; set; } = string.Empty;
        public string StewardPincode { get; set; } = string.Empty;
        public bool IsStewardActive { get; set; } = true;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region ItemMaster
    public class ItemMasterModel
    {
        public int ItemId { get; set; } = 0;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal ItemRate { get; set; } = 0;
        public string RevenueCode { get; set; } = string.Empty;
        public string RevenueName { get; set; } = string.Empty;
        public bool IsItemActive { get; set; } = true;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region SegmentMaster
    public class SegmentMasterModel
    {
        public int SegmentId { get; set; } = 0;
        public string SegmentCode { get; set; } = string.Empty;
        public string SegmentName { get; set; } = string.Empty;
        public string SegmentDescription { get; set; } = string.Empty;
        public bool IsSegmentActive { get; set; } = true;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region SubSegmentMaster
    public class SubSegmentMasterModel
    {
        public int SubSegmentId { get; set; } = 0;
        public string SubSegmentCode { get; set; } = string.Empty;
        public string SubSegmentName { get; set; } = string.Empty;
        public string SegmentCode { get; set; } = string.Empty;
        public bool IsSubSegmentActive { get; set; } = true;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region GuestMaster
    public class GuestMasterModel
    {
        public int GuestId { get; set; } = 0;
        public string GuestCode { get; set; } = string.Empty;
        public string GuestTitle { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestGender { get; set; } = string.Empty;
        public DateTime GuestDOB { get; set; } = DateTime.MinValue;
        public DateTime GuestDOA { get; set; } = DateTime.MinValue;
        public string GuestAddress { get; set; } = string.Empty;
        public string GuestCity { get; set; } = string.Empty;
        public string GuestState { get; set; } = string.Empty;
        public string GuestNation { get; set; } = string.Empty;
        public string GuestPinCode { get; set; } = string.Empty;
        public string GuestMobileNo { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string IdentityproofCode { get; set; } = string.Empty;
        public string IdentityproofName { get; set; } = string.Empty;
        public string GuestVehicleType { get; set; } = string.Empty;
        public string GuestVehicleNo { get; set; } = string.Empty;
        public bool IsGuestActive { get; set; } = true;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }
    #endregion

    #region FormCMaster

    public class FormCMasterModel
    {
        public int FormCId { get; set; } = 0;
        public string FormCCode { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string CheckinNo { get; set; } = string.Empty;

        public string IndiaAddress { get; set; } = string.Empty;
        public string IndiaCity { get; set; } = string.Empty;
        public string IndiaState { get; set; } = string.Empty;
        public string IndiaPinCode { get; set; } = string.Empty;
        public string IndiaCountry { get; set; } = string.Empty;

        public string PermanentAddress { get; set; } = string.Empty;
        public string PermanentCity { get; set; } = string.Empty;
        public string PermanentState { get; set; } = string.Empty;
        public string PermanentPinCode { get; set; } = string.Empty;
        public string PermanentCountry { get; set; } = string.Empty;

        public string PassportNo { get; set; } = string.Empty;
        public string PassportIssueCity { get; set; } = string.Empty;
        public string PassportIssueCountry { get; set; } = string.Empty;
        public DateTime PassportIssueDate { get; set; } = DateTime.MinValue;
        public DateTime PassportExpiryDate { get; set; } = DateTime.MinValue;

        public string VisaNo { get; set; } = string.Empty;
        public string VisaIssueCity { get; set; } = string.Empty;
        public string VisaIssueCountry { get; set; } = string.Empty;
        public DateTime VisaIssueDate { get; set; } = DateTime.MinValue;
        public DateTime VisaExpiryDate { get; set; } = DateTime.MinValue;
        public string VisaType { get; set; } = string.Empty;

        public string ArrivalCity { get; set; } = string.Empty;
        public string ArrivalCountry { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; } = DateTime.MinValue;

        public DateTime HotelArrivalDate { get; set; } = DateTime.MinValue;
        public TimeSpan HotelArrivalTime { get; set; } = TimeSpan.MinValue;
        public int NoofStay { get; set; } = 0;
        public string VisitPurpose { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; } = DateTime.MinValue;

        public string NextDestinationPlace { get; set; } = string.Empty;
        public string NextDestinationCity { get; set; } = string.Empty;
        public string NextDestinationState { get; set; } = string.Empty;
        public string NextDestinationCountry { get; set; } = string.Empty;

        public string MobileNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class IndianAddressDetail
    {
        public string IndiaAddress { get; set; } = string.Empty;
        public string IndiaCity { get; set; } = string.Empty;
        public string IndiaState { get; set; } = string.Empty;
        public string IndiaPinCode { get; set; } = string.Empty;
        public string IndiaCountry { get; set; } = string.Empty;
    }

    public class PermanentAddressDetail
    {
        public string PermanentAddress { get; set; } = string.Empty;
        public string PermanentCity { get; set; } = string.Empty;
        public string PermanentState { get; set; } = string.Empty;
        public string PermanentPinCode { get; set; } = string.Empty;
        public string PermanentCountry { get; set; } = string.Empty;
    }

    public class PassportDetail
    {
        public string PassportNo { get; set; } = string.Empty;
        public string PassportIssueCity { get; set; } = string.Empty;
        public string PassportIssueCountry { get; set; } = string.Empty;
        public DateTime PassportIssueDate { get; set; } = DateTime.MinValue;
        public DateTime PassportExpiryDate { get; set; } = DateTime.MinValue;
    }

    public class VisaDetail
    {
        public string VisaNo { get; set; } = string.Empty;
        public string VisaIssueCity { get; set; } = string.Empty;
        public string VisaIssueCountry { get; set; } = string.Empty;
        public DateTime VisaIssueDate { get; set; } = DateTime.MinValue;
        public DateTime VisaExpiryDate { get; set; } = DateTime.MinValue;
        public string VisaType { get; set; } = string.Empty;
    }

    public class ArrivalDetail
    {
        public string ArrivalCity { get; set; } = string.Empty;
        public string ArrivalCountry { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; } = DateTime.MinValue;

        public DateTime HotelArrivalDate { get; set; } = DateTime.MinValue;
        public TimeSpan HotelArrivalTime { get; set; } = TimeSpan.MinValue;
        public int NoofStay { get; set; } = 0;
        public string VisitPurpose { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; } = DateTime.MinValue;

    }

    public class NextDestinationDetail
    {
        public string NextDestinationPlace { get; set; } = string.Empty;
        public string NextDestinationCity { get; set; } = string.Empty;
        public string NextDestinationState { get; set; } = string.Empty;
        public string NextDestinationCountry { get; set; } = string.Empty;

    }

    public class ContactDetail
    {
        public string MobileNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class FormCRequestModel
    {
        public int FormCId { get; set; } = 0;
        public string FormCCode { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string CheckinNo { get; set; } = string.Empty;

        public IndianAddressDetail IndiaDetails { get; set; } = new();

        public PermanentAddressDetail PermanentDetails { get; set; } = new();

        public PassportDetail PassportDetails { get; set; } = new();

        public VisaDetail VisaDetails { get; set; } = new();

        public ArrivalDetail ArrivalDetails { get; set; } = new();

        public NextDestinationDetail NextDestinationDetails { get; set; } = new();

        public ContactDetail ContactDetails { get; set; } = new();

        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region LedgerMaster
    public class LedgerMasterModel
    {
        public int LedgerId { get; set; } = 0;
        public string LedgerCode { get; set; } = string.Empty;
        public string LedgerName { get; set; } = string.Empty;
        public decimal LedgerOpeningBalance { get; set; } = 0;
        public bool IsLedgerActive { get; set; } = true;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region TaxRulesMaster
    public class TaxRulesMasterModel
    {
        public int TaxRuleId { get; set; } = 0;
        public string TaxRuleCode { get; set; } = string.Empty;
        public string Traiff { get; set; } = string.Empty;
        public string TraiffOperation { get; set; } = string.Empty;
        public string Type1 { get; set; } = string.Empty;
        public string Operation1 { get; set; } = string.Empty;
        public string Type2 { get; set; } = string.Empty;
        public bool IsTaxRuleActive { get; set; } = false;
        public bool IsSeprateExtraBedTax { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }

    #endregion

    #region DriverMaster
    public class DriverMasterModel
    {
        public int DriverId { get; set; } = 0;
        public string DriverCode { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string DriverMobileNo { get; set; } = string.Empty;
        public string DriverVechileNo { get; set; } = string.Empty;
        public bool IsDriverActive { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }

    #endregion

    #region BookingSourceMaster
    public class BookingSourceMasterModel
    {
        public int BookingSourceId { get; set; } = 0;
        public string BookingSourceCode { get; set; } = string.Empty;
        public string BookingSourceName { get; set; } = string.Empty;
        public bool IsBookingSourceActive { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }

    #endregion

    #region InstructionDepartmentMaster
    public class InstructionDepartmentMasterModel
    {
        public int InstructionDepartmentId { get; set; } = 0;
        public string InstructionDepartmentCode { get; set; } = string.Empty;
        public string InstructionDepartmentName { get; set; } = string.Empty;
        public bool IsInstructionDepartmentActive { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }

    #endregion

    #region SalesExecutiveMaster
    public class SalesExecutiveMasterModel
    {
        public int SalesExecutiveId { get; set; } = 0;
        public string SalesExecutiveCode { get; set; } = string.Empty;
        public string SalesExecutiveName { get; set; } = string.Empty;
        public bool IsSalesExecutiveActive { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }

    #endregion

    #region ReservationPayModeMaster
    public class ReservationPayModeMasterModel
    {
        public int ReservationPayModeId { get; set; } = 0;
        public string ReservationPayModeCode { get; set; } = string.Empty;
        public string ReservationPayModeName { get; set; } = string.Empty;
        public bool IsReservationPayModeActive { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }

    #endregion
}
