using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;

namespace FrontOfficeApplication.ProjectEntitiesModel.Reservation
{
    public class RoomAvailability
    {
        public string RoomCode { get; set; } = string.Empty;

        public int TotalRooms { get; set; } = 0;

        public List<RoomAvailabilityDate> Dates { get; set; } = [];
    }

    public class RoomAvailabilityDate
    {
        public DateTime Date { get; set; } = DateTime.MinValue;

        public int AvailableRooms { get; set; } = 0;
    }

    public class RoomAllocated
    {
        public string RoomCode { get; set; } = string.Empty;

        public int Allocated { get; set; } = 0;
    }

    public class ReservationRequestModel
    {
        public DateTime CheckInDate { get; set; } = DateTime.MinValue;
        public DateTime CheckOutDate { get; set; } = DateTime.MinValue;
        public int NoofDays { get; set; } = 0;
        public TimeSpan CheckInTime { get; set; } = TimeSpan.MinValue;
        public TimeSpan CheckOutTime { get; set; } = TimeSpan.MinValue;

        public int ReservationId { get; set; } = 0;
        public string ReservationNo { get; set; } = string.Empty;
        public string PMSCMS { get; set; } = string.Empty;

        public ReservationGuestDetailModel GuestDetail { get; set; } = new();
        public List<BookingDetailModel> BookingDetail { get; set; } = [];
        public List<PaymentDetailModel> PaymentDetail { get; set; } = [];
        public List<SpecialInstructionModel> InstructionDetail { get; set; } = [];
        public List<ReservationChargeModel> ReservationChargeDetail { get; set; } = [];

        public bool IsExistingReservation { get; set; } = false;
        public bool IsAmend { get; set; } = false;
        public bool IsReservationExtraCharged { get; set; } = false;
        public bool IsSpecialInstruction { get; set; } = false;
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class ReservationGuestDetailModel
    {
        public int GuestId { get; set; } = 0;
        public string GuestCode { get; set; } = string.Empty;
        public string GuestTitle { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;

        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public string GuestGender { get; set; } = string.Empty;
        public string GuestAddress { get; set; } = string.Empty;
        public string GuestCity { get; set; } = string.Empty;
        public string GuestState { get; set; } = string.Empty;
        public string GuestNation { get; set; } = string.Empty;
        public string GuestPinCode { get; set; } = string.Empty;
        public string GuestMobileNo { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;
        public string ReferenceMobileNo { get; set; } = string.Empty;
        public string ReferenceEmail { get; set; } = string.Empty;

        public string SegmentName { get; set; } = string.Empty;
        public string SubSegmentName { get; set; } = string.Empty;
        public string SalesExecutiveName { get; set; } = string.Empty;
        public string BookingSourceName { get; set; } = string.Empty;
        public string PurposeName { get; set; } = string.Empty;
        public string ReservationPayModeName { get; set; } = string.Empty;
    }

    public class TaxDetailsModel
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string TaxDescriptionCode { get; set; } = string.Empty;
        public string TaxDescriptionName { get; set; } = string.Empty;
        public decimal TaxPercentage { get; set; } = 0;
        public decimal FromAmount { get; set; } = 0;
        public decimal ToAmount { get; set; } = 0;
        public DateTime FromDate { get; set; } = DateTime.MinValue;
        public DateTime ToDate { get; set; } = DateTime.MinValue;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class TaxRulesDetailsModel
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
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }

    public class PlanDetailsModel
    {
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal FoodRate { get; set; } = 0;
        public string TaxCode { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class RoomTraiffDetailModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int Allocated { get; set; } = 0;
        public string RoomPlanType { get; set; } = string.Empty;
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int PAX { get; set; } = 0;
        public string PAXRates { get; set; } = string.Empty;
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class CompanyTraiffDetailModel
    {
        public string CompanyCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string TraiffCategory { get; set; } = string.Empty;
        public string RoomPlanType { get; set; } = string.Empty;
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int PAX { get; set; } = 0;
        public string PAXRates { get; set; } = string.Empty;
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class BookingDetailModel
    {
        public int ReservationSlaveId{ get; set; } = 0;
        public int ForEverId { get; set; } = 0;
        public string ReservationSlaveCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int NoofRoom { get; set; } = 0;
        public int NoofAdult { get; set; } = 0;
        public int NoofChild { get; set; } = 0;
        public string PlanCode { get; set; } = string.Empty;
        public int Allocated { get; set; } = 0;
        public TaxBreakdownResponseDto TaxCalculationDetails { get; set; } = new();
        public decimal FoodRate { get; set; } = 0;
        public decimal Traiff { get; set; } = 0;
        public string DiscountType { get; set; } = string.Empty;
        public decimal Netpayable { get; set; } = 0;

    }

    public class PaymentDetailModel
    {
        public int AdvanceReceiptId { get; set; } = 0;
        public int MainModeId { get; set; } = 0;
        public string MainModeType { get; set; } = string.Empty;
        public int SubModeId { get; set; } = 0;
        public string SubModeType { get; set; } = string.Empty;

        public ChequeDetails ChequeDetail { get; set; } = new();
        public CardDetails CardDetail { get; set; } = new();
        public OnlineDetails OnlineDetail { get; set; } = new();

        public class ChequeDetails
        {
            public string ChequeNo { get; set; } = string.Empty;
            public string BankName { get; set; } = string.Empty;
            public DateTime ChequeDate { get; set; } = DateTime.MinValue;
        }

        public class CardDetails
        {
            public string CardHolderName { get; set; } = string.Empty;
            public string CardNumber { get; set; } = string.Empty;
        }

        public class OnlineDetails
        {
            public string AccountNo { get; set; } = string.Empty;
            public string AccountHolderName { get; set; } = string.Empty;
            public string BankName { get; set; } = string.Empty;
            public DateTime OnlineDate { get; set; } = DateTime.MinValue;
        }

        public decimal AmountPaid { get; set; } = 0;
        public decimal AmountBalance { get; set; } = 0;
       
    }

    public class ReservationChargeModel
    {
        public int ReservationChargeId { get; set; } = 0;
        public string BookingNo { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ChargesDescription { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;

    }

    public class SpecialInstructionModel
    {
        public int SpecialInstructionId { get; set; } = 0;
        public string InstructionDepartmentCode { get; set; } = string.Empty;
        public string InstructionDescriptionName { get; set; } = string.Empty;
        public string SpecialInstructionDescription { get; set; } = string.Empty;
        public string SpecialInstructionRemark { get; set; } = string.Empty;
        public string BookingNo { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
    }

    public class BookingCancellationModel
    {
        public int ModuleId { get; set; } = 0;
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public DateTime CancelDate { get; set; } = DateTime.MinValue;
        public TimeSpan CancelTime { get; set; } = TimeSpan.MinValue;
        public string CancelRemarks { get; set; } = string.Empty;

        public int MainModeId { get; set; } = 0;
        public string MainModeType { get; set; } = string.Empty;
        public int SubModeId { get; set; } = 0;
        public string SubModeType { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; } = DateTime.MinValue;
        public decimal NetAmount { get; set; } = 0;
        public decimal AmountPaid { get; set; } = 0;
        public decimal RefundPercentage { get; set; } = 0;
        public decimal RefundAmount { get; set; } = 0;

        public string UserCode { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class GuestSearchDetailModel
    {
        public int GuestId { get; set; } = 0;
        public string GuestCode { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestMobileNo { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
    }

    public class GuestMaster
    {
        public int GuestId { get; set; } = 0;
        public string GuestCode { get; set; } = string.Empty;
        public string GuestTitle { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestGender { get; set; } = string.Empty;
        public DateTime? GuestDOB { get; set; }
        public DateTime? GuestDOA { get; set; }
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
        public bool IsGuestActive { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class ReservationMaster
    {
        public int ReservationId { get; set; } = 0;
        public string BookingNo { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public decimal? Traiff { get; set; }
        public int? StayDays { get; set; }
        public int? NoofRooms { get; set; }
        public bool? CUpdate { get; set; }
        public bool? SUpdate { get; set; }
        public bool? EUpdate { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string BookingSourceName { get; set; } = string.Empty;
        public string SegmentName { get; set; } = string.Empty;
        public string SubSegmentName { get; set; } = string.Empty;
        public string PurposeName { get; set; } = string.Empty;
        public string SalesExecutiveName { get; set; } = string.Empty;
        public string ReservationPayModeName { get; set; } = string.Empty;
        public string PMSCMS { get; set; } = string.Empty;
        public string FlagPMSCMS { get; set; } = string.Empty;
        public string FlagAmend { get; set; } = string.Empty;
    }

    public class ReservationSlave
    {
        public int ReservationSlaveId { get; set; } = 0;
        public string ReservationSlaveCode { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
        public DateTime? ArrivalDate { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public DateTime? DepatureDate { get; set; }
        public TimeSpan DepatureTime { get; set; }
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public int NoofRooms { get; set; } = 0;
        public int AdultPax { get; set; } = 0;
        public int ChildPax { get; set; } = 0;
        public int TotalPax { get; set; } = 0;
        public decimal Traiff { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public decimal NetPayTraiff { get; set; } = 0;
        public string TaxCode { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public string RoomAllocated { get; set; } = string.Empty;
        public bool? GUpdate { get; set; }
        public bool? CUpdate { get; set; }
        public bool? SUpdate { get; set; }
        public bool? RUpdate { get; set; }
    }

    public class ForEverData
    {
        public int ForEverId { get; set; } = 0;
        public string BookingNo { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int NoofRooms { get; set; } = 0;
        public string RoomStatus { get; set; } = string.Empty;
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepatureDate { get; set; }
        public TimeSpan TillTime { get; set; }
        public string ReservationStatus { get; set; } = string.Empty;
        public string TranscationType { get; set; } = string.Empty;
        public string TranscationStatus { get; set; } = string.Empty;
        public string PMSCMS { get; set; } = string.Empty;
        public string FlagPMSCMS { get; set; } = string.Empty;
        public string RandomNo { get; set; } = string.Empty;
        public string CheckInFlag { get; set; } = string.Empty;
    }

    public class AdvanceReceipt
    {
        public int AdvanceReceiptId { get; set; } = 0;
        public string ReceiptNo { get; set; } = string.Empty;
        public string BookingNo { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public int MainModeId { get; set; } = 0;
        public string MainModeType { get; set; } = string.Empty;
        public int SubModeId { get; set; } = 0;
        public string SubModeType { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public DateTime? TransactionDate { get; set; }
        public decimal AmountPaid { get; set; } = 0;
        public decimal AmountBalance { get; set; } = 0;
    }

    public class ReservationCharge
    {
        public int ReservationChargeId { get; set; } = 0;
        public string BookingNo { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ChargesDescription { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
    }

    public class SpecialInstruction
    {
        public int SpecialInstructionId { get; set; } = 0;
        public string InstructionDepartmentCode { get; set; } = string.Empty;
        public string InstructionDepartmentName { get; set; } = string.Empty;
        public string SpecialInstructionDescription { get; set; } = string.Empty;
        public string SpecialInstructionRemark { get; set; } = string.Empty;
        public string BookingNo { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
    }

    public class TaxDetails
    {
        public int TaxDetailsId { get; set; } = 0;
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public decimal TariffAmount { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalTaxAmount { get; set; } = 0;
        public decimal GrandTotal { get; set; } = 0;
        public string CategorizedTaxesJson { get; set; } = string.Empty;

        public Dictionary<string, List<TaxItemDto>> TaxCalculationDetails { get; set; } = [];
    }
}
