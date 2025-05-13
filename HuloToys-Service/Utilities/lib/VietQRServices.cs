using HuloToys_Service.Models.Payment;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace HuloToys_Service.Utilities.lib
{
    public class VietQRServices
    {
        private readonly List<VietQRBankModel> vietQRBanks;
        private readonly string xClientId = "ba09d2bf-7f59-442f-8c26-49a8d48e78d7";
        private readonly string xClientKey= "a479a45c-47d5-41c1-9f83-990d65cd832a";
        public VietQRServices(IConfiguration configuration)
        {
            try
            {
                var data = File.ReadAllText("databank.json");
                vietQRBanks = JsonConvert.DeserializeObject<List<VietQRBankModel>>(data);
            }
            catch { }
        }
        public async Task<string> GetVietQRCode(string account_number, string account_name, int bank_id, string order_no, double amount)
        {
            try
            {
                string bank_code = vietQRBanks.First(x => x.id == bank_id).bin;
                var options = new RestClientOptions("https://api.vietqr.io");
                var client = new RestClient(options);
                var request = new RestRequest("/v2/generate", Method.Post);
                request.AddHeader("x-client-id", xClientId);
                request.AddHeader("x-api-key", xClientKey);
                request.AddHeader("Content-Type", "application/json");
                var body = "{ \"accountNo\": \""
                    + account_number
                    + "\", \"accountName\": \""+ account_name + "\", \"acqId\": \""
                    + (bank_code.Length > 6 ? bank_code.Substring(0, 6) : bank_code)
                    + "\", \"addInfo\": \""
                    + order_no
                    + " THANH TOAN\", \"amount\": \"" + Math.Round(amount, 0)
                    + "\", \"template\": \"compact\" }";
                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request);
                var result= response.Content;
                var jsonData = JObject.Parse(result);
                var status = int.Parse(jsonData["code"].ToString());

                if (status == 0)
                {
                    return jsonData["data"]["qrDataURL"].ToString();
                }
            }
            catch
            {

            }
            return null;
        }
    }
}
