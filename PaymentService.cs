
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
            string email =enotModel.custom_fields.Email;

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

        private bool ValidateEnotSignature(EnotModelNew enotModel, string signature)
        {
            // b863ff88afff14abc3ba1812ac2823c65de87701f099f0f0283a862f164e5490
            // Вычисляется хеш из предварительно отсортированных по ключу от a - zA - Z тела
            // хука по алгоритму sha256 hmac без экранированных слешей и unicode символов.

            string key = _appSettings.EnotSignBack;

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
            CustomField cf = new CustomField(email);
            string shopId = _appSettings.EnotShopId;

            long id = DateTime.Now.Ticks;
            double amountDouble;
            Double.TryParse(amount, out amountDouble);
         

            WebhookModel createInvoiceModel = new WebhookModel
            {
                currency = "EUR",
                email = cf.Email,
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
            string secret = _appSettings.EnotSignFront;
            string enotCreateInvoiceURL = _appSettings.EnotBaseUrl + "/create";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("x-api-key", secret);

            var stringPayload = JsonConvert.SerializeObject(enotInvoiceModel);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(enotCreateInvoiceURL, httpContent);
            string order = await response.Content.ReadAsStringAsync();

            CreateInvoiceResponce result = System.Text.Json.JsonSerializer.Deserialize<CreateInvoiceResponce>(order);
            if (result.error != null)
            {
                throw new ApplicationException(result.error);
            }
            return result;
        }

        private async Task<InvoiceDetails> GetInvoiceInfo(string invoiceId)
        {
            string shopId = _appSettings.EnotShopId;
            string enotGetInvoiceURL = _appSettings.EnotBaseUrl + "/info";
            string secret = _appSettings.EnotSignFront;

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
