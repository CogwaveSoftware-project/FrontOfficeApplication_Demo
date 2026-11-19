using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.GST;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Newtonsoft.Json;
using System.Data;
using System.Text.Json.Serialization;

namespace FrontOfficeApplication.ProjectServiceLayer.GST
{
    public class EInvoiceGST
    {
        private readonly IDbConnectionFactory _factory;
        private readonly IHttpClientFactory _httpClientFactory;

        public EInvoiceGST(IDbConnectionFactory factory,IHttpClientFactory httpClientFactory)
        {
            _factory = factory;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GSTDetailsModel> GSTVerification(string Gstin, string PropertyCode, string BranchCode)
        {
            if (string.IsNullOrWhiteSpace(Gstin))
                throw new ArgumentException("Gstin is required.", nameof(Gstin));

            // Get existing authentication
            var authentication = await GetAuthentication(Gstin, PropertyCode, BranchCode) ?? throw new Exception("E-Invoice authentication details not found.");

            // Get existing authentication
            var resultauthentication = await GetAuthenticationResult(Gstin, PropertyCode, BranchCode);

            if (resultauthentication != null)
            {
                DateTime CurrentTime = DateTime.Now;
                DateTime perviouskeyTime = resultauthentication.Data_TokenExpiry;
                if (CurrentTime >= perviouskeyTime)
                {
                    // Authenticate and get new AuthToken
                    _ = await GSTAuthenticate(authentication);
                }
                
            }
            else
            {
                // Authenticate and get new AuthToken
                _ = await GSTAuthenticate(authentication);
            }
            
            // Get updated authentication details
            authentication = await GetAuthentication(Gstin, PropertyCode, BranchCode) ?? throw new Exception("E-Invoice authentication details not found.");

            //if (authentication == null)
            //    throw new Exception( "E-Invoice authentication details not found.");

            var baseUrl = await GetEInvoiceURL() ?? throw new Exception("E-Invoice URL not configured.");

            //if (string.IsNullOrWhiteSpace(baseUrl))
            //    throw new Exception( "E-Invoice URL not configured.");

            var url =
                $"{baseUrl}/einvoice/type/SYNC_GSTIN_FROMCP/version/V1_03" +
                $"?param1={Uri.EscapeDataString(Gstin)}" +
                $"&email={Uri.EscapeDataString(authentication.Email ?? "")}";

            var client = _httpClientFactory.CreateClient();

            // Headers
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add( "ip_address", authentication.Ip_Address ?? "");

            client.DefaultRequestHeaders.Add( "client_id", authentication.Client_Id ?? "");

            client.DefaultRequestHeaders.Add( "client_secret", authentication.Client_Secret ?? "");

            client.DefaultRequestHeaders.Add( "username", authentication.UserName ?? "");

            client.DefaultRequestHeaders.Add( "auth-token", authentication.AuthToken ?? "");

            client.DefaultRequestHeaders.Add( "gstin", authentication.Gstin ?? "");

            // API call
            var response = await client.GetAsync(url);

            var jsonResult = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"GST verification failed. " +
                    $"StatusCode: {response.StatusCode}, " +
                    $"Response: {jsonResult}");
            }

            var result = JsonConvert.DeserializeObject<GSTDetailsModel>(jsonResult) ?? throw new Exception("Invalid response received from GST API.");

            // Save GST details
            _ = await AddTaxpayerDetailsMasterGST(result, PropertyCode, BranchCode);

            return result;
        }

        public async Task<AuthenticationResult?> GSTAuthenticate(AuthenticationModel request)
        {
            var authentication = await GetAuthentication(request.Gstin, request.PropertyCode, request.BranchCode) ?? throw new Exception("Authentication details not found.");

            //if (authentication == null) 
            //    throw new Exception( "Authentication details not found.");

            var baseUrl = await GetEInvoiceURL();

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception( "E-Invoice URL not configured.");

            var url =
                $"{baseUrl}/einvoice/authenticate" +
                $"?email={Uri.EscapeDataString(authentication.Email ?? "")}";

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add( "username", authentication.UserName ?? "");

            client.DefaultRequestHeaders.Add( "password", authentication.Password ?? "");

            client.DefaultRequestHeaders.Add( "ip_address", authentication.Ip_Address ?? "");

            client.DefaultRequestHeaders.Add( "client_id", authentication.Client_Id ?? "");

            client.DefaultRequestHeaders.Add( "client_secret", authentication.Client_Secret ?? "");

            client.DefaultRequestHeaders.Add( "gstin", authentication.Gstin ?? "");

            // API call
            var response = await client.GetAsync(url);

            var jsonResult = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Authentication failed. " +
                    $"StatusCode: {response.StatusCode}, " +
                    $"Response: {jsonResult}");
            }

            var result = JsonConvert.DeserializeObject<AuthenticationResult>( jsonResult) ?? throw new Exception("Invalid authentication response.");

            //if (result == null)
            //    throw new Exception("Invalid authentication response.");

            // Update authentication token
            var details = new AuthenticationModel
            {
                EInId = authentication.EInId,
                AuthToken = result.DataDetail?.AuthToken ?? string.Empty,
                PropertyCode = authentication.PropertyCode,
                BranchCode = authentication.BranchCode
            };

            // Update authentication Token
            await UpdateAuthentication(details);

            // Save authentication response
            await AddAuthenticationResult(result, request.PropertyCode, request.BranchCode);

            return result;
        }

        public async Task<string?> GetEInvoiceURL()
        {
            using var connection = _factory.CreateMasterConnection();

            var selectquery = @"Select UrlName From Tbl_EInvoice_Url; ";

            var Url = await connection.QueryFirstOrDefaultAsync<string>(selectquery);
            return Url;
        }

        public async Task<AuthenticationModel?> GetAuthentication(string Gstin, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectQuery = @" SELECT EInId, Email, UserName, Password, Ip_Address, Client_Id, Client_Secret, Gstin, AuthToken, PropertyCode, BranchCode
            FROM Tbl_EInvoice_Authenticate WHERE (@gstin IS NULL OR @gstin = '' OR Gstin = @gstin) And PropertyCode = @propertyCode And BranchCode = @branchCode; ";

            var result = await connection.QueryFirstOrDefaultAsync<AuthenticationModel>(selectQuery, new { gstin = Gstin, propertycode = PropertyCode, branchcode = BranchCode });
            return result;
        }

        public async Task<AuthenticationResultResponse?> GetAuthenticationResult(string Gstin, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectQuery = @" SELECT Status_cd, Status_desc, Data_UserName, Data_TokenExpiry, Data_Sek, Data_ClientId, Data_AuthToken, 
            Header_UserName, Header_Password, Header_Ip_Address, Header_Client_id, Header_Client_secret, Header_Gstin, Header_Gst_username,
            Header_Txn, PropertyCode, BranchCode FROM Tbl_EInvoice_Authenticate_Result 
            WHERE (@gstin IS NULL OR @gstin = '' OR Header_Gstin = @gstin) And PropertyCode = @propertyCode And BranchCode = @branchCode 
            Order by AuthResultId Desc; ";

            var result = await connection.QueryFirstOrDefaultAsync<AuthenticationResultResponse>(selectQuery, new { gstin = Gstin, propertycode = PropertyCode, branchcode = BranchCode });
            return result;
        }

        public async Task<int> UpdateAuthentication( AuthenticationModel details)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var updateQuery = @" UPDATE Tbl_EInvoice_Authenticate SET AuthToken = @authtoken WHERE EInId = @einid And PropertyCode = @propertyCode And BranchCode = @branchCode; ";

            return await connection.ExecuteAsync( updateQuery, new { einid = details.EInId, authtoken= details.AuthToken, propertyCode = details.PropertyCode, branchCode = details.BranchCode });
        }

        public async Task<int> AddAuthenticationResult( AuthenticationResult result, string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string insertQuery = @"
            INSERT INTO Tbl_EInvoice_Authenticate_Result
            (
                Status_cd, Status_desc, Data_UserName, Data_TokenExpiry, Data_Sek, Data_ClientId, Data_AuthToken, Header_UserName, Header_Password,
                Header_Ip_Address, Header_Client_id, Header_Client_secret, Header_Gstin, Header_Gst_username, Header_Txn, PropertyCode, BranchCode
            )
            VALUES
            (
                @status_cd, @status_desc, @data_username, @data_tokenexpiry, @data_sek, @data_clientid, @data_authtoken, @header_username, 
                @header_password, @header_ip_address, @header_client_id, @header_client_secret, @header_gstin, @header_gst_username, 
                @header_txn, @propertycode, @branchcode
            );";

            var rowaffected = await connection.ExecuteAsync(insertQuery,
                new
                {
                    status_cd = result.Status_cd,
                    status_desc = result.Status_desc,

                    data_username = result.DataDetail?.UserName,
                    data_tokenexpiry = result.DataDetail?.TokenExpiry,
                    data_sek = result.DataDetail?.Sek,
                    data_clientid = result.DataDetail?.ClientId,
                    data_authtoken = result.DataDetail?.AuthToken,

                    header_username = result.HeaderDetail.username,
                    Header_password = result.HeaderDetail.password,
                    Header_ip_address = result.HeaderDetail.ip_address,
                    Header_client_id = result.HeaderDetail.client_id,
                    Header_client_secret = result.HeaderDetail.client_secret,
                    Header_gstin = result.HeaderDetail.gstin,
                    Header_gst_username = result.HeaderDetail.gst_username,
                    Header_txn = result.HeaderDetail.txn,

                    propertycode = PropertyCode,
                    branchcode = BranchCode
                });

            return rowaffected;
        }

        public async Task<bool> AddTaxpayerDetailsMasterGST(GSTDetailsModel result, string PropertyCode, string BranchCode)
        {
            try
            {
                if (result?.Data == null ||string.IsNullOrWhiteSpace(result.Data.Gstin))
                {
                    return false;
                }

                using var connection = await _factory.CreateConnection(DbNames.HMS);

                // Check whether GSTIN already exists
                const string checkQuery = @" SELECT COUNT(1) FROM Tbl_Taxpayer_Details WHERE TaxpayerGSTNo = @gstno 
                And PropertyCode = @propertyCode And BranchCode = @branchCode; ";

                var exists = await connection.ExecuteScalarAsync<int>( checkQuery, new { gstno = result.Data.Gstin, propertyCode = PropertyCode, branchCode = BranchCode });

                // Already exists
                if (exists > 0)
                {
                    return true;
                }

                // Insert GST details
                const string insertQuery = @"
                INSERT INTO Tbl_Taxpayer_Details
                (
                    TaxpayerGSTNo, LegalName, TradeName, GSTType, State, BuildingNo, BuildingName, FloorNo, StreetName, Location, StateCode, 
                    PinCode, PropertyCode, BranchCode
                )
                VALUES
                (
                    @taxpayergstno, @legalname, @tradename, @gsttype, @state, @buildingno, @buildingname, @floorno, @streetname, @location, @statecode,
                    @pincode, @propertycode, @branchcode
                );";

                var rowsAffected = await connection.ExecuteAsync(
                    insertQuery,
                    new
                    {
                        taxpayergstno = result.Data.Gstin,
                        legalname = result.Data.LegalName,
                        tradename = result.Data.TradeName,
                        gsttype = result.Data.TxpType,
                        state = string.Empty,
                        buildingno = result.Data.AddrBno,
                        buildingname = result.Data.AddrBnm,
                        floorno = result.Data.AddrFlno,
                        streetname = result.Data.AddrSt,
                        location = result.Data.AddrLoc,
                        statecode = result.Data.StateCode,
                        pincode = result.Data.AddrPncd,
                        propertycode = PropertyCode,
                        branchcode = BranchCode
                    });

                return rowsAffected > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
