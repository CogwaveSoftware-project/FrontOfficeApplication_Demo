using FrontOfficeApplication.ProjectEntitiesModel.Reservation;

namespace FrontOfficeApplication.ProjectServiceLayer.Reservation.DTOs
{
    public class TaxDetailsResponseDto
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public List<TaxDescriptionDto> TaxDescriptions { get; set; } = [];
    }

    public class TaxDescriptionDto
    {
        public string TaxDescriptionCode { get; set; } = string.Empty;
        public string TaxDescriptionName { get; set; } = string.Empty;
        public decimal TaxPercentage { get; set; } = 0;
        public decimal FromAmount { get; set; } = 0;
        public decimal ToAmount { get; set; } = 0;
        public DateTime FromDate { get; set; } = DateTime.MinValue;
        public DateTime ToDate { get; set; } = DateTime.MinValue;
    }

    public class RoomTraiffDetailResponseDto
    {
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int Allocated { get; set; } = 0;
        public string RoomPlanType { get; set; } = string.Empty;
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int Pax { get; set; } = 0;
        public List<PlanRatedetailsDto> PaxRates { get; set; } = [];
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class PlanRatedetailsDto
    {
        public int PlanId { get; set; } = 0;
        public string PlanName { get; set; } = string.Empty;
        public List<PaxRateDto> PaxRates { get; set; } = [];
    }

    public class PaxRateDto
    {
        public int PaxNo { get; set; }
        public decimal Rate { get; set; }
    }

    public class CompanyTraiffDetailResponseDto
    {
        public string CompanyCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string TraiffCategory { get; set; } = string.Empty;
        public string RoomPlanType { get; set; } = string.Empty;
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int Pax { get; set; } = 0;
        public List<PlanRatedetailsDto> PaxRates { get; set; } = [];
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class ReservationDetailsCombinedResponseDto
    {
        public IEnumerable<TaxDetailsResponseDto> TaxDetails { get; set; } = [];
        public TaxRulesDetailsModel TaxRulesDetails { get; set; } = new();
        public IEnumerable<PlanDetailsModel> PlanDetails { get; set; } = [];
        public IEnumerable<RoomTraiffDetailResponseDto> RoomTariffDetails { get; set; } = [];
        public IEnumerable<CompanyTraiffDetailResponseDto> CompanyTariffDetails { get; set; } = [];
    }

    public class ReservationSaveResult
    {
        public int InsertId { get; set; } = 0;
        public string ActionType { get; set; } = string.Empty;
        public int StatusCode { get; set; } = 0;
        public string Message { get; set; } = string.Empty;
    }


    public class ReservationCompleteDetailsDto
    {
        public GuestMaster GuestDetails { get; set; } = new();
        public ReservationMaster ReservationMaster { get; set; } = new();
        public List<ReservationSlave> ReservationSlaves { get; set; } = [];
        public List<ForEverData> ForEverData { get; set; } = [];
        public List<AdvanceReceipt> AdvanceReceipts { get; set; } = [];
        public List<ReservationCharge> ReservationCharges { get; set; } = [];
        public List<SpecialInstruction> SpecialInstructions { get; set; } = [];
        public TaxDetails TaxDetails { get; set; } = new();
    }
}


