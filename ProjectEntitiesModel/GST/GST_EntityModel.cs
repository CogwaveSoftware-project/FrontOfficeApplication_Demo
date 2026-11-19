using Newtonsoft.Json;

namespace FrontOfficeApplication.ProjectEntitiesModel.GST
{
    public class GSTDetailsModel
    {

        public GSTDetails Data { get; set; } = new();
        public string Status_cd { get; set; } = string.Empty;
        public string Status_desc { get; set; } = string.Empty;

    }

    public class GSTDetails
    {
        public string Gstin { get; set; } = string.Empty;
        public string TradeName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string AddrBnm { get; set; } = string.Empty;
        public string AddrBno { get; set; } = string.Empty;
        public string AddrFlno { get; set; } = string.Empty;
        public string AddrSt { get; set; } = string.Empty;
        public string AddrLoc { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string AddrPncd { get; set; } = string.Empty;
        public string TxpType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string BlkStatus { get; set; } = string.Empty;
        public string DtReg { get; set; } = string.Empty;
        public string DtDReg { get; set; } = string.Empty;
    }

    public class AuthenticationModel
    {
        public int EInId { get; set; } = 0;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Ip_Address { get; set; } = string.Empty;
        public string Client_Id { get; set; } = string.Empty;
        public string Client_Secret { get; set; } = string.Empty;
        public string Gstin { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class AuthenticationResult
    {
        public class Data
        {
            public string UserName { get; set; } = string.Empty;
            public string TokenExpiry { get; set; } = string.Empty;
            public string Sek { get; set; } = string.Empty;
            public string ClientId { get; set; } = string.Empty;
            public string AuthToken { get; set; } = string.Empty;
        }
        public string Status_cd { get; set; } = string.Empty;
        public string Status_desc { get; set; } = string.Empty;
        [JsonProperty("data")]
        public virtual Data DataDetail { get; set; } = new();
        [JsonProperty("header")]
        public virtual Header HeaderDetail { get; set; } = new();

        public class Header
        {
            public string username { get; set; } = string.Empty;
            public string password { get; set; } = string.Empty;
            public string ip_address { get; set; } = string.Empty;
            public string client_id { get; set; } = string.Empty;
            public string client_secret { get; set; } = string.Empty;
            public string gstin { get; set; } = string.Empty;
            public string gst_username { get; set; } = string.Empty;
            public string txn { get; set; } = string.Empty;
        }
    }


    public class AuthenticationResultResponse
    {
        public string Status_cd { get; set; } = string.Empty;
        public string Status_desc { get; set; } = string.Empty;

        public string Data_UserName { get; set; } = string.Empty;
        public DateTime Data_TokenExpiry { get; set; } = DateTime.MinValue;
        public string Data_Sek { get; set; } = string.Empty;
        public string Data_ClientId { get; set; } = string.Empty;
        public string Data_AuthToken { get; set; } = string.Empty;

        public string Header_UserName { get; set; } = string.Empty;
        public string Header_Password { get; set; } = string.Empty;
        public string Header_Ip_Address { get; set; } = string.Empty;
        public string Header_Client_id { get; set; } = string.Empty;
        public string Header_Client_secret { get; set; } = string.Empty;
        public string Header_Gstin { get; set; } = string.Empty;
        public string Header_Gst_username { get; set; } = string.Empty;
        public string Header_Txn { get; set; } = string.Empty;

        public string PropertyCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

}
