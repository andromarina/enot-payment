using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace EnotPayment
{
    public class PaymentService
    {
        private AppSettings _appSettings;
       public PaymentService(IOptions<AppSettings> appSettings) { 
            _appSettings = appSettings.Value;
       }
        public string ProcessPayment(string enotResponce, string signature)
        {
            EnotModelNew enotModel = System.Text.Json.JsonSerializer.Deserialize<EnotModelNew>(enotResponce);
            string email = enotModel.custom_fields.Email;

            if (enotModel.code != 1)
            {
                throw new ApplicationException("Transaction was not successful: " + enotModel.invoice_id);
            }
            try
            {
                if (!ValidateEnotSignature(enotModel, signature))
                {
                    throw new ApplicationException("Bad sign for transaction: " + enotModel.invoice_id);
                }             
                    return "Successfull payment for Email: " + email;
            }
            catch (Exception ex)
            {            
                throw new ApplicationException(ex.Message);
            }
        }


        //https://docs.enot.io/webhook
        private bool ValidateEnotSignature(EnotModelNew enotModel, string signature)
        {
            // Вычисляется хеш из предварительно отсортированных по ключу от a - zA - Z тела
            // хука по алгоритму sha256 hmac без экранированных слешей и unicode символов.

            string key = _appSettings.AdditionalKey;

            var encoder = new UTF8Encoding();
            var modelAsString = System.Text.Json.JsonSerializer.Serialize(enotModel);

            string hmacHash = CalculateHMAC(encoder.GetBytes(modelAsString), encoder.GetBytes(key));
            if (hmacHash.Equals(signature))
            {
                return true;
            }
            return false;
        }

        private static string CalculateHMAC(byte[] data, byte[] key)
        {
            var hmacsha256 = new HMACSHA256(key);
            byte[] hashmessage = hmacsha256.ComputeHash(data);
            return Convert.ToHexString(hashmessage).ToLower();
        }


        public string GetEnotPaymentLink(string amount, string email)
        {
            amount = amount + ".00";
            string shopId = _appSettings.ShopId;
            CustomField cf = new CustomField(email);
            long id = DateTime.Now.Ticks;
                   

            WebhookModel createInvoiceModel = new WebhookModel
            {
                currency = "EUR",
                email = email,
                expire = 300,
                amount = amount,
                custom_fields = System.Text.Json.JsonSerializer.Serialize(cf),
                shop_id = shopId,
                order_id = id.ToString(),
                comment = "Test payment"
            };

            string result = CreateInvoice(createInvoiceModel).Result.data.url;
            return result;
        }

        private async Task<CreateInvoiceResponce> CreateInvoice(WebhookModel enotInvoiceModel)
        {
            string secret = _appSettings.SecretKey;
            string enotCreateInvoiceURL = _appSettings.BaseUrl + "/create";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            //https://docs.enot.io/authentication
            httpClient.DefaultRequestHeaders.Add("x-api-key", secret);

            var stringPayload = JsonConvert.SerializeObject(enotInvoiceModel);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(enotCreateInvoiceURL, httpContent);
            string order = await response.Content.ReadAsStringAsync();

            Console.WriteLine(order);
            CreateInvoiceResponce result = System.Text.Json.JsonSerializer.Deserialize<CreateInvoiceResponce>(order);
            if (result.error != null)
            {
                throw new ApplicationException(result.error);
            }
            return result;
        }

        public async Task<InvoiceDetails> GetInvoiceInfo(string invoiceId)
        {
            string shopId = _appSettings.ShopId;
            string enotGetInvoiceURL = _appSettings.BaseUrl + "/info";
            string secret = _appSettings.SecretKey;

            var param = new Dictionary<string, string>() { { "invoice_id", invoiceId }, { "shop_id", shopId } };

            var newUrl = new Uri(QueryHelpers.AddQueryString(enotGetInvoiceURL, param));

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", secret);

            HttpResponseMessage response = await httpClient.GetAsync(newUrl);
            string order = await response.Content.ReadAsStringAsync();

            InvoiceDetails result = System.Text.Json.JsonSerializer.Deserialize<InvoiceDetails>(order);
            if (result.error != null)
            {
                Console.WriteLine("Getting invoice with id: " + invoiceId + " failed, because of: " + result.error);
            }
            return result;
        }
    }
}
