using FrontOfficeApplication.ProjectEntitiesModel.Master;

namespace FrontOfficeApplication.ProjectServiceLayer.Master.DTOs
{
    public class RoomTypeResponseDto
    {
        // RoomType information
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

        // Tariff information
        public decimal DefaultTraiff { get; set; } = decimal.Zero;
        public int Pax { get; set; } = 0;
        public List<PlanRateDto> PaxRates { get; set; } = [];
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public bool Taxinclude { get; set; } = false;
        public bool Package { get; set; } = false;

        //Common
        public int UserCode { get; set; } = 0;
        public string IPAddress { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class MainandSubModeResponseDto
    {
        public int MainModeId { get; set; } = 0;

        public List<PaymentSubModeModel> SubModes { get; set; } = [];
    }

    public class CompanyTraiffResponseDto
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

    public class FormCResponseDto
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
}
