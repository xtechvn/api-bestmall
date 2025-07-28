using HuloToys_Service.Models.Orders;
using HuloToys_Service.Utilities.Lib;
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
                result_part += "vnp_Version=2.1.0" + "&";
                result_part += "vnp_Command=pay" + "&";
                result_part += "vnp_TmnCode="+ TMN_CODE + "&";
                result_part += "vnp_Amount=" + (order.total_amount * 100) + "&";
                result_part += "vnp_BankCode=VNPAYQR&";
                result_part += "vnp_CreateDate="+DateTime.Now.ToString("yyyyMMddHHmmss") +"&";
                result_part += "vnp_CurrCode=VND&";
                result_part += "vnp_IpAddr="+ ip_client + "&";
                result_part += "vnp_Locale="+ country + "&";
                result_part += "vnp_OrderInfo=" + ("Thanh toan don hang"+order.order_no) + "&";
                result_part += "vnp_OrderType=100000&";
                result_part += "vnp_ExpireDate="+DateTime.Now.AddHours(1).ToString("yyyyMMddHHmmss") +"&";
                result_part += "vnp_TxnRef=" + (order.order_no+ DateTime.Now.ToString("yyyyMMddHHmmss")) + "&";

                result_part += "vnp_ReturnUrl=" + RETURN_URL.Replace("{url}", order._id) + "&";
                string vnp_SecureHash = HmacSHA512(SECRET_KEY, result_part);
                result_part += "vnp_SecureHash=" + vnp_SecureHash;
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
}
