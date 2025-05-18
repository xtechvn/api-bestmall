using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Models;
using HuloToys_Service.Models.Queue;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.RedisWorker;

using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using Utilities.Contants;
//cuonglv tesst git
namespace HuloToys_Service.Controllers.Test
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        private readonly WorkQueueClient work_queue;
        public TestController(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
            work_queue = new WorkQueueClient(configuration);

        }
        /// <summary>
        /// Test login 
        /// </summary>
        /// <returns></returns>
        [HttpGet("verify-authent.json")]
         // Bật login lấy token      
        public async Task<IActionResult> verifyAuthent()
        {
            try
            {
                return Ok(new { msg = "You are authenticated" });
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return null;
            }
        }

        /// <summary>
        /// Test ElasticSearch
        /// </summary>
        /// <returns></returns>
        [HttpGet("test-elastic.json")]
         // Bật login lấy token      
        public async Task<IActionResult> testElastic()
        {
            try
            {
                var index_es = configuration["DataBaseConfig:Elastic:index_product"];
                var es_host = configuration["DataBaseConfig:Elastic:Host"];

                IESRepository<object> _ESRepository = new ESRepository<object>(es_host, configuration);
                var result_product = _ESRepository.FindById("product", "đồ chơi", "product_id");
                return Ok(new { data = result_product });
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new { error = 1, msg = ex.ToString() });
            }
        }

        /// <summary>
        /// Test RABBITMQ
        /// </summary>
        /// <returns></returns>
        [HttpPost("test-push-queue.json")]
         // Bật login lấy token      
        public async Task<IActionResult> testRabbitMQ()
        {
            try
            {
                bool response_queue = false;
                var j_param = new Dictionary<string, object>
                {
                    {"data_push","test message queue"}, // có thể là json
                    {"type",QueueType.ADD_ADDRESS}
                };
                var _data_push = JsonConvert.SerializeObject(j_param);

                // Execute Push Queue

                response_queue = work_queue.InsertQueueSimple(_data_push, QueueName.queue_app_push);
                if (response_queue)
                {
                    return Ok(new { msg = "done" });
                }
                else
                {
                    return Ok(new { msg = "fail" });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new { msg = ex.ToString() });
            }
        }
    }
}
