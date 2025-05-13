using Caching.Elasticsearch;
using HuloToys_Service.Models.Client;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Utilities;

namespace HuloToys_Service.Controllers.Client.Business
{
    public class ClientServices 
    {
        private readonly IConfiguration _configuration;
        private readonly ClientESService _clientESService;
        private readonly AccountClientESService _accountClientESService;
        public ClientServices(IConfiguration configuration) {

            _configuration=configuration;
            _clientESService = new ClientESService(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _accountClientESService = new AccountClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);

        }

        public async Task<string> GenerateToken(string user_name,string? ip)
        {
            string token = null;
            try
            {

                ClientFELoginModel model = new ClientFELoginModel()
                {
                    exprire = DateTime.Now.ToUniversalTime().AddDays(30),
                    ip=ip,
                    user_name=user_name,
                };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(model), _configuration["KEY:private_key"]);
            }
            catch
            {

            }
            return token;
        }
        public async Task<long> GetAccountClientIdFromToken(string token)
        {
            long account_client_id = -1;
            try
            {
                var decoded = CommonHelper.Decode(token, _configuration["KEY:private_key"]);
                if(decoded!=null && decoded.Trim() != "")
                {
                    var model=JsonConvert.DeserializeObject<ClientFELoginModel>(decoded);

                    if (model!=null && model.user_name!=null && model.user_name.Trim() != "")
                    {
                        var account = _accountClientESService.GetByUsername(model.user_name);
                        if (account == null)
                        {
                            LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], 
                                "GetAccountClientIdFromToken with ["+model.user_name+"] ID=" + (account == null ? "[accountclient null]" : account.Id.ToString()));

                        }
                        else
                        {
                            account_client_id = account.Id;

                        }
                    }

                }
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], "GetAccountClientIdFromToken"+ex.ToString());

            }
            return account_client_id;
        }
        public DateTime GetExpiredTimeFromToken(string token)
        {
            DateTime time= DateTime.Now.AddDays(1);
            try
            {
                var decoded = CommonHelper.Decode(token, _configuration["KEY:private_key"]);
                if (decoded != null && decoded.Trim() != "")
                {
                    var model = JsonConvert.DeserializeObject<ClientFELoginModel>(decoded);
                    if (model != null && model.user_name != null && model.user_name.Trim() != "")
                    {
                        time = model.exprire;
                    }

                }
            }
            catch
            {

            }
            return time;
        }
    }
}
