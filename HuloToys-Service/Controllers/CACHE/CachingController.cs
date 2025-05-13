using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.CACHE
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class CachingController : Controller
    {
        private IConfiguration configuration;
        private readonly RedisConn redisService;
      
        public CachingController(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
            redisService.Connect();


        }


        /// Clear cache bài viết
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("sync-article.json")]
        public async Task<ActionResult> clearCacheArticle([FromBody] APIRequestGenericModel input)
        {
            try
            {
                string j_param = "{'article_id':'39','category_id':'35'}";
                //  token = CommonHelper.Encode(j_param, configuration["DataBaseConfig:key_api:api_manual"]);

                JArray objParr = null;
                if (input != null && input.token!=null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {

                    long article_id = Convert.ToInt64(objParr[0]["article_id"]);
                    var category_list_id = objParr[0]["category_id"].ToString().Split(",");
                    redisService.clear(CacheType.ARTICLE_ID + article_id, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    for (int i = 0; i <= category_list_id.Length - 1; i++)
                    {
                        int category_id = Convert.ToInt32(category_list_id[i]);
                        redisService.clear(CacheType.ARTICLE_CATEGORY_ID + "22", Convert.ToInt32(configuration["Redis:Database:db_common"]));
                        redisService.clear(CacheType.CATEGORY_NEWS + "1", Convert.ToInt32(configuration["Redis:Database:db_common"]));
                        redisService.clear(CacheType.CATEGORY_NEWS + category_id, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                        redisService.clear(CacheType.ARTICLE_MOST_VIEWED, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    }

                    return Ok(new { status = (int)ResponseType.SUCCESS, _token = input.token, msg = "Sync Successfully !!!", article_id = article_id, category_list_id = category_list_id });
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.FAILED, _token = input.token, msg = "Token Error !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "sync-article.json - clearCacheArticle " + ex.ToString() + " token=" + input.token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, _token = input.token, msg = "Sync error !!!" });
            }
        }
    }
}
