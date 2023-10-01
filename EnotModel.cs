
namespace EnotPayment
{  

    public class EnotModelNew
    {
        public string amount { get; set; }
        public int code { get; set; }
        public string credited { get; set; }
        public string currency { get; set; }

        public CustomField custom_fields { get; set; }

        public string invoice_id { get; set; }

        public string order_id { get; set; }

        public string pay_service { get; set; }

        public string pay_time { get; set; }

        public string payer_details { get; set; }

        public string status { get; set; }

        public int type { get; set; }

    }

    public class CustomField
    {
        public CustomField() { }

        public CustomField(string email)
        {
            Email = email;
        }
        public string Email { get; set; }
    }

    public class WebhookModel
    {
        public string amount { get; set; }

        public string order_id { get; set; }

        public string email { get; set; }

        public string currency { get; set; }

        public string custom_fields { get; set; }

        public string comment { get; set; }

        public string shop_id { get; set; }

        public int expire { get; set; }
    }

        public class CreateInvoiceResponce
    {
            public EnotInvoiceData? data { get; set; }

            public int status { get; set; }

            public bool status_check { get; set; }

            public string error { get; set; }
}

        public class EnotInvoiceData
        {
            public string id { get; set; }

            public string amount { get; set; }            

            public string currency { get; set; }

            public string url { get; set; }

            public string expired { get; set; }
    }

    public class InvoiceDetails
    {
        public InvoiceDetailsExtended data { get; set; }
        public int status { get; set; }
        public bool status_check { get; set; }

        public string? error { get; set; } 

    }

    public class InvoiceDetailsExtended
    {
        public string invoice_id { get; set; }

        public string order_id { get; set; }

        public string shop_id { get; set; }

        public string status { get; set; }

        public double? invoice_amount { get; set; }

        public double? credited { get; set; }

        public string currency { get; set; }

        public string pay_service { get; set; }

        public double? commission_amount { get; set; }

        public double? commission_percent { get; set; }

        public double? shop_commission_amount { get; set; }

        public double? shop_commission_percent { get; set; }

        public double? user_commission_amount { get; set; }

        public double? user_commission { get; set; }

        public CustomField custom_field { get; set; }

        public string? created_at { get; set; }

        public string? expired_at { get; set; }

        public string? paid_at { get; set; }
    }
}