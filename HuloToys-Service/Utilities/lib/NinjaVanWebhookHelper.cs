using System.Security.Cryptography;
using System.Text;

namespace HuloToys_Service.Utilities.lib
{
    public static class NinjaVanWebhookHelper
    {
        public static string calculateHmac(string jsonBody, string clientSecret)
        {
            Encoding encoding = Encoding.UTF8;
            var key = encoding.GetBytes(clientSecret);
            HMACSHA256 hmacsha256 = new HMACSHA256(key);
            var byteArray = encoding.GetBytes(jsonBody);
            var result = hmacsha256.ComputeHash(byteArray);
            return Convert.ToBase64String(result);
        }

        public static bool verifyWebhook(string calculatedHmac, string hmacHeader)
        {
            return calculatedHmac.Equals(hmacHeader);
        }
    }
}
