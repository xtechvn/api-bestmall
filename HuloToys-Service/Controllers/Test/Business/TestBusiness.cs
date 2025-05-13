using HuloToys_Service.Models;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Newtonsoft.Json;
using System.Reflection;
using Utilities.Contants;

// Create By: cuonglv
namespace HuloToys_Service.Controllers.Test.Business
{
    public partial class TestBusiness
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public TestBusiness(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
        }   
        /// <summary>
        /// Lấy ra danh sách User
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public async Task<List<UserLoginModel>> getListUser(int user_id)
        {
            try
            {
                string cache_key = CacheType.USER_LIST;
                int db_index = Convert.ToInt32(configuration["DataBaseConfig:Redis:db_common"]);
                var gr = new List<UserLoginModel>();

                var j_product_detail = await redisService.GetAsync(cache_key, db_index);
                if (!string.IsNullOrEmpty(j_product_detail) && j_product_detail != "null")
                {
                    gr = JsonConvert.DeserializeObject<List<UserLoginModel>>(j_product_detail);
                    return gr;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {                
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString());
                return null;                
            }
        }



    }
}
