using FrontOfficeApplication.ProjectServiceLayer.Reservation.DTOs;

namespace FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation
{
    public class TaxCalculationPayloadDto
    {
        public TaxCalculationRequestDto CalculationRequest { get; set; } = new();
        public IEnumerable<TaxDetailsResponseDto> TaxDetails { get; set; } = [];
    }

    public class TaxCalculationRequestDto
    {
        public decimal TariffAmount { get; set; } = 0;
        public DateTime CalculationDate { get; set; } = DateTime.MinValue;
        public bool IsForeignOrInterstateClient { get; set; } // True for IGST, False for CGST/SGST
        public decimal ExtraAdult { get; set; } = decimal.Zero;
        public decimal ExtraChild { get; set; } = decimal.Zero;
        public decimal DiscountValue { get; set; } = decimal.Zero;
        public bool IsDiscountPercentage { get; set; } = false;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }

    public class TaxCalculationResultDto
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string TaxDescriptionCode { get; set; } = string.Empty;
        public string TaxDescriptionName { get; set; } = string.Empty;
        public decimal TaxPercentage { get; set; } = 0;
        public decimal TaxableAmount { get; set; } = 0;
        public decimal CalculatedTaxAmount { get; set; } = 0;
    }

    public class TaxBreakdownResponseDto1
    {
        public decimal TariffAmount { get; set; } = 0;
        public List<TaxItemDto> CGST { get; set; } = [];
        public List<TaxItemDto> SGST { get; set; } = [];
        public List<TaxItemDto> IGST { get; set; } = [];
        public List<TaxItemDto> OtherTaxes { get; set; } = [];
        public decimal TotalTaxAmount { get; set; } = 0;
        public decimal GrandTotal { get; set; } = 0;
    }

    public class TaxBreakdownResponseDto
    {
        public int TaxDetailsId { get; set; } = 0;
        public decimal TariffAmount { get; set; } = 0;
        public Dictionary<string, List<TaxItemDto>> CategorizedTaxes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalTaxAmount { get; set; } = 0;
        public decimal GrandTotal { get; set; } = 0;
    }

    public class TaxItemDto
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public string TaxDescriptionCode { get; set; } = string.Empty;
        public string TaxDescriptionName { get; set; } = string.Empty;
        public decimal TaxPercentage { get; set; } = 0;
        public decimal CalculatedTaxAmount { get; set; } = 0;
    }
}
