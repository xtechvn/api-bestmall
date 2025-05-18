using API_CORE.Controllers.Models;
using API_CORE.Controllers.RedisWorker;
using API_CORE.Controllers.Utilities.Lib;
using Newtonsoft.Json;
using System.Reflection;
using Utilities.Contants;

namespace API_CORE.Controllers.Controllers.Order.Business
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
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString());
                return null;
            }
        }



    }
}
