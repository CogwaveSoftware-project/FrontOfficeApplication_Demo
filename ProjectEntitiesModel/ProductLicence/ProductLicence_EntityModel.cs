namespace FrontOfficeApplication.ProjectEntitiesModel.ProductLicence
{
    public class ProductLicenceModel
    {
        public string SerialKey { get; set; } = string.Empty;
        public string ProductKey { get; set; } = string.Empty;
        public DateTime TrDate { get; set; } = DateTime.Now;
        public DateTime ValidDate { get; set; } = DateTime.Now;
        public int NoDays { get; set; } = 0;
        public DateTime IntimationDate { get; set; } = DateTime.Now;
        public int IndimationDays { get; set; } = 0;
        public string ClientName { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
        public string BranchCode { get; set; } = string.Empty;
        public string Encryptedserialkey { get; set; } = string.Empty;
        public string EncryptedToDate { get; set; } = string.Empty;
        public string EncryptedIntimationDate { get; set; } = string.Empty;

    }

    public class InsertProductLicenceModel
    {
        public string SerialKey { get; set; } = string.Empty;
        public string ProductKey { get; set; } = string.Empty;
        public DateTime TrDate { get; set; } = DateTime.Now;
        public DateTime ValidDate { get; set; } = DateTime.Now;
        public string ClientName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }
}
