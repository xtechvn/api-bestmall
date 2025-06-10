using HuloToys_Service.Models;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Newtonsoft.Json;
using System.Reflection;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Order.Business
{

    public partial class OrderService
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public OrderService(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
        }

        public async Task<List<UserLoginModel>> getOrderList()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString());
                return null;
            }
        }



    }
}
