using HuloToys_Service.Models.Orders;
using HuloToys_Service.Utilities.Lib;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace HuloToys_Service.Controllers.Payment.Bussiness
{
    public class VNPayService
    {
        private readonly IConfiguration configuration;
        public readonly string DOMAIN = "https://sandbox.vnpayment.vn/paymentv2/";
        private readonly string API_PAY = "vpcpay.html";
        private readonly string TMN_CODE = "BMTES1TT";
        private readonly string RETURN_URL = "https://bestmall.com.vn/order/payment/{url}";
        private readonly string SECRET_KEY = "693PXB8X9BOAWRPPOB3DL9IOHPJY8904";


        public VNPayService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> BuildURL(OrderDetailMongoDbModel order,string ip_client,string country="vn")
        {
            string result = DOMAIN + API_PAY + "?";

            try
            {
                string result_part = "";
                //result_part += "vnp_Version=2.1.0" + "&";
                //result_part += "vnp_Command=pay" + "&";
                //result_part += "vnp_TmnCode="+ WebUtility.UrlEncode(TMN_CODE) + "&";
                //result_part += "vnp_Amount=" + WebUtility.UrlEncode((order.total_amount * 100).ToString()) + "&";
                //result_part += "vnp_BankCode=&";
                //result_part += "vnp_CreateDate="+ WebUtility.UrlEncode(DateTime.Now.ToString("yyyyMMddHHmmss")) +"&";
                //result_part += "vnp_CurrCode=VND&";
                //result_part += "vnp_IpAddr="+ WebUtility.UrlEncode(ip_client) + "&";
                //result_part += "vnp_Locale="+ country + "&";
                //result_part += "vnp_OrderInfo=" + WebUtility.UrlEncode(("Thanh toan don hang "+order.order_no)) + "&";
                //result_part += "vnp_OrderType=100000&";
                //result_part += "vnp_ExpireDate="+ WebUtility.UrlEncode(DateTime.Now.AddHours(1).ToString("yyyyMMddHHmmss")) +"&";
                //result_part += "vnp_TxnRef=" + WebUtility.UrlEncode(order.order_no) + "&";

                //result_part += "vnp_ReturnUrl=" + RETURN_URL.Replace("{url}", order._id);
                VnpayPaymentRequest paymentRequest = new VnpayPaymentRequest(
                    tmnCode: TMN_CODE,
                    totalAmount: Convert.ToDecimal(((double)order.total_amount * 100)),
                    ipClient: ip_client,
                    country: country,
                    orderNo: order.order_no,
                    returnUrl: RETURN_URL.Replace("{url}", order._id)
                );

                result_part=paymentRequest.ToQueryString();
                var data = JsonConvert.SerializeObject(paymentRequest);
                string vnp_SecureHash = HmacSHA512(SECRET_KEY, result_part);
                result_part += "&"+ "vnp_SecureHash=" + vnp_SecureHash;
                return result + result_part;
            }
            catch (Exception ex) {
                LogHelper.InsertLogTelegram("BuildURL - VNPayService: error [" + (DOMAIN + API_PAY) + "] :" + ex.ToString());
            }
            return result;
        }
        public async Task<bool> ValidateURL(string url_part, string hash)
        {

            try
            {
                string myChecksum = HmacSHA512(SECRET_KEY, url_part);
                return myChecksum.Equals(hash, StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ValidateURL - VNPayService: error [" + (DOMAIN + API_PAY) + "] :" + ex.ToString());
            }
            return false;
        }
        public static String HmacSHA512(string key, String inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }
    public class VnpayPaymentRequest
    {
        public string vnp_Version { get; set; } = "2.1.0";
        public string vnp_Command { get; set; } = "pay";
        public string vnp_TmnCode { get; set; }
        public string vnp_Amount { get; set; }
        //public string vnp_BankCode { get; set; } = "";
        public string vnp_CreateDate { get; set; }
        public string vnp_CurrCode { get; set; } = "VND";
        public string vnp_IpAddr { get; set; }
        public string vnp_Locale { get; set; }
        public string vnp_OrderInfo { get; set; }
        public string vnp_OrderType { get; set; } = "100000";
        public string vnp_ExpireDate { get; set; }
        public string vnp_TxnRef { get; set; }
        public string vnp_ReturnUrl { get; set; }

        // Constructor to easily populate values
        public VnpayPaymentRequest(
            string tmnCode,
            decimal totalAmount,
            string ipClient,
            string country,
            string orderNo,
            string returnUrl)
        {
            vnp_TmnCode = tmnCode;
            vnp_Amount = (totalAmount * 100).ToString();
            vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            vnp_IpAddr = ipClient;
            vnp_Locale = country;
            vnp_OrderInfo = orderNo;
            vnp_ExpireDate = DateTime.Now.AddHours(1).ToString("yyyyMMddHHmmss");
            vnp_TxnRef = orderNo;
            vnp_ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Converts the object properties into a URL-encoded query string.
        /// Properties with null or empty values will be excluded.
        /// </summary>
        /// <returns>A URL-encoded query string.</returns>
        public string ToQueryString()
        {
            var properties = GetType().GetProperties()
                .Where(p => p.GetValue(this) != null) // Exclude null properties
                .OrderBy(p => p.Name) // Optional: order properties alphabetically for consistent URL
                .ToList();

            var queryParams = new List<string>();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(this)?.ToString();
                queryParams.Add($"{prop.Name}={WebUtility.UrlEncode(value)}");
            }
            return string.Join("&", queryParams);
        }
    }
}
