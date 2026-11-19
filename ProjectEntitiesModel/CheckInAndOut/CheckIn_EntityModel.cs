using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.Reservation;
using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;

namespace FrontOfficeApplication.ProjectEntitiesModel.CheckInAndOut
{
    public class CheckInRequestModel
    {
        public string RoomNo { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; } = DateTime.MinValue;
        public DateTime CheckOutDate { get; set; } = DateTime.MinValue;
        public int NoofDays { get; set; } = 0;
        public TimeSpan CheckInTime { get; set; } = TimeSpan.MinValue;
        public TimeSpan CheckOutTime { get; set; } = TimeSpan.MinValue;

        public int CheckInId { get; set; } = 0;
        public string CheckInNo { get; set; } = string.Empty;
        public string PMSCMS { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;

        public List<CheckInGuestDetailModel> GuestDetails { get; set; } = [];

        public DriverMasterModel DriverDetails { get; set; } = new();
        public List<FormCRequestModel> FormCDetails { get; set; } = [];
        public List<PaymentDetailModel> PaymentDetail { get; set; } = [];

        public string CheckInType { get; set; } = "WalkIn"; 
        public string ReservationNo { get; set; } = string.Empty;
        public string GRCNo { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; } = 0;
        public int GracePeriod { get; set; } = 0;

        public List<CheckInBookingDetailModel> BookingDetail { get; set; } = [];
        public List<ReservationChargeModel> CheckInChargeDetail { get; set; } = [];
        public List<SpecialInstructionModel> InstructionDetail { get; set; } = [];

        public bool IsExistingCheckIn { get; set; } = false;
        public bool IsAmend { get; set; } = false;
        public bool IsCheckInExtraCharged { get; set; } = false;
        public bool IsSpecialInstruction { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class CheckInGuestDetailModel
    {
        public int GuestId { get; set; } = 0;
        public string GuestCode { get; set; } = string.Empty;
        public string GuestTitle { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;

        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;

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

        public string Reference { get; set; } = string.Empty;
        public string ReferenceMobileNo { get; set; } = string.Empty;
        public string ReferenceEmail { get; set; } = string.Empty;

        public string SegmentName { get; set; } = string.Empty;
        public string SubSegmentName { get; set; } = string.Empty;
        public string SalesExecutiveName { get; set; } = string.Empty;
        public string BookingSourceName { get; set; } = string.Empty;
        public string PurposeName { get; set; } = string.Empty;
        public string ConventionFees { get; set; } = string.Empty;
        public bool IsPrimaryGuest { get; set; } = false;
        public string BookingId { get; set; } = string.Empty;
    }

    public class CheckInBookingDetailModel
    {
        public int CheckInSlaveId { get; set; } = 0;
        public int ForEverId { get; set; } = 0;
        public string RoomNo { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int NoofRoom { get; set; } = 0;
        public int NoofAdult { get; set; } = 0;
        public int NoofChild { get; set; } = 0;
        public string PlanCode { get; set; } = "EP";
        public decimal Traiff { get; set; } = 0;
        public decimal FoodRate { get; set; } = 0;
        public decimal Netpayable { get; set; } = 0;
        public string DiscountType { get; set; } = string.Empty;
        public int Allocated { get; set; } = 0;
        public TaxBreakdownResponseDto TaxCalculationDetails { get; set; } = new();
    }


}
