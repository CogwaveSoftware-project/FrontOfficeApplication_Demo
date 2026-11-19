using FrontOfficeApplication.ProjectEntitiesModel.ProductLicence;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Services
{
    public class ProductLicence_Service : IProductLicence_Service
    {
        private readonly IProductLicence_Repository _repository;

        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");

        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");

        public ProductLicence_Service(IProductLicence_Repository repository)
        {
            _repository = repository;
        }

        public async Task<ProductLicenceModel> GetProductLicenceKey(string branchcode)
        {
            var productLicenceKey = await _repository.GetProductLicenceKey(branchcode);
            return productLicenceKey;
        }

        public async Task<ServiceResult<int>> SaveProductLicenceKey(InsertProductLicenceModel model)
        {
            DateTime currentdate = ConvertUtcToIst();

            string serial = model.SerialKey;
            DateTime fromdate = model.TrDate;
            DateTime todate = model.ValidDate;
            int totaldays = (todate.Date - fromdate.Date).Days;
            DateTime intimationdate = todate.AddDays(-10);
            int indimationdays = 10;

            //if (totaldays < indimationdays)
            //{
            //    return new ProductLicenceServiceResult<int>
            //    {
            //        Success = false,
            //        Message = "Please Enter ValidDate, Validate Should be More then 10 Days",
            //        Data = 0
            //    };
            //}

            //string ser = "1234-5678-9012-3456";
            string ser1 = serial.Replace("-", "");

            bool validproductkey = await DualCrackCode(decimal.Parse(model.ProductKey), decimal.Parse(ser1));

            if (!validproductkey)
            {
                return ServiceResult<int>.Fail(0, $"Invalid product or serial key.");
            }

            string Encodeserialkey = await EncodePass(model.SerialKey);
            string Encodeproductkey = await EncodePass(model.ProductKey);
            string encryptedserialkey = await Encrypt(model.SerialKey);
            string encryptedToDate = await Encrypt(todate.ToString("yyyy-MM-dd"));
            string encryptedIntimationDate = await Encrypt(intimationdate.ToString("yyyy-MM-dd"));

            var result = await _repository.SaveProductLicenceKey(model, totaldays, intimationdate, indimationdays, Encodeserialkey, Encodeproductkey, encryptedserialkey, encryptedToDate, encryptedIntimationDate);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Successfully saved product or serial key.");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to save product or serial key.");
            }
        }

        public DateTime ConvertUtcToIst()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Ensure input is treated as UTC
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            }

            TimeZoneInfo istZone;

            // Handle Windows & Linux
            try
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
        }

        public async Task<bool> DualCrackCode(decimal productKey, decimal serialKey)
        {
            try
            {
                decimal keyValue = decimal.Round(productKey, 0);
                keyValue = decimal.Round(keyValue / 120, 0);
                keyValue = decimal.Round(keyValue - 99, 0);
                keyValue = decimal.Round(keyValue / 15, 0);

                return decimal.Round(keyValue, 0) == decimal.Round(serialKey, 0);
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> Encrypt(string plainText)
        {
            //var plainText = DateTime.Now.ToString("yyyyMMddHHmmss");

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor();

            byte[] input = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(input, 0, input.Length);

            return Convert.ToBase64String(encrypted);
        }

        public async Task<string> Decrypt(string cipher)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor();

            byte[] input = Convert.FromBase64String(cipher);
            byte[] decrypted = decryptor.TransformFinalBlock(input, 0, input.Length);

            return Encoding.UTF8.GetString(decrypted);
        }

        public async Task<string> EncodePass(string txtpass)
        {
            if (txtpass == null)
                throw new ArgumentNullException(nameof(txtpass));

            StringBuilder encPass = new StringBuilder();

            for (int i = 0; i < txtpass.Length; i++)
            {
                char ch = txtpass.Substring(i, 1)[0];

                int ascv = (int)ch;

                if (ascv >= 0 && ascv <= 47)
                {
                    ascv = (int)ch;
                }
                else if (ascv >= 58 && ascv <= 64)
                {
                    ascv = (int)ch;
                }
                else if (ascv >= 48 && ascv <= 57)
                {
                    ascv = (int)ch + 195;
                }
                else
                {
                    ascv = (int)ch + 100;
                }
                encPass.Append((char)ascv);
            }
            return encPass.ToString();
        }

        public async Task<string> DecodePass(string encptPass)
        {
            try
            {
                if (encptPass == null)
                    throw new ArgumentNullException(nameof(encptPass));

                StringBuilder decPass = new StringBuilder();

                for (int i = 0; i < encptPass.Length; i++)
                {
                    char ch = encptPass.Substring(i, 1)[0];

                    int ascv = (int)ch;

                    if (ascv >= 0 && ascv <= 47)
                    {
                        ascv = (int)ch;
                    }
                    else if (ascv >= 58 && ascv <= 64)
                    {
                        ascv = (int)ch;
                    }
                    else if (ascv >= 243 && ascv <= 255)
                    {
                        ascv = (int)ch - 195;
                    }
                    else
                    {
                        ascv = (int)ch - 100;
                    }

                    decPass.Append((char)ascv);
                }

                return decPass.ToString();
            }
            catch
            {
                throw new ArgumentException("Invalid encoded password");
            }
        }
    }
}
