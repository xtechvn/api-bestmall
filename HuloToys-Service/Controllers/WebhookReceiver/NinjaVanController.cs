using App_Push_Consummer.Model.Comments;
using Caching.Elasticsearch;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities.Contants;
using HuloToys_Service.Utilities.lib;

namespace HuloToys_Service.Controllers.Carrier
{
    [ApiController]
    [Route("api/receiver/[controller]")]
    public class NinjaVanController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly WorkQueueClient workQueueClient;
        private readonly OrderESService orderESRepository;
        private readonly OrderMongodbService orderMongodbService;
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly AccountClientESService accountClientESService;
        private readonly CartMongodbService _cartMongodbService;
        private readonly WorkQueueClient work_queue;
        private readonly RedisConn _redisService;
        private readonly ClientESService clientESService;
        private readonly RaitingESService raitingESService;

        public NinjaVanController(IConfiguration _configuration, RedisConn redisService)
        {
            configuration = _configuration;

            workQueueClient = new WorkQueueClient(configuration);
            orderESRepository = new OrderESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            accountClientESService = new AccountClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            orderMongodbService = new OrderMongodbService(configuration);
            _productDetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _cartMongodbService = new CartMongodbService(configuration);
            work_queue = new WorkQueueClient(configuration);
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            clientESService = new ClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);

        }
        [HttpPost("pending")]
        public async Task<ActionResult> PendingPickup([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
        [HttpPost("dispatch-pickup")]
        public async Task<ActionResult> DispatchPickup([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
        [HttpPost("exception-pickup")]
        public async Task<ActionResult> ExceptionPickup([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
        [HttpPost("picked-up")]
        public async Task<ActionResult> PickedUp([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
       
        [HttpPost("on-delivery")]
        public async Task<ActionResult> OnDelivery([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
        [HttpPost("at-pudo")]
        public async Task<ActionResult> AtPUDO([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
        [HttpPost("deliver-to-customer")]
        public async Task<ActionResult> DeliverToCustomer([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        } 
        [HttpPost("cancelled")]
        public async Task<ActionResult> CancelledShipping([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
        [HttpPost("return-to-sender")]
        public async Task<ActionResult> RenturnToSender([FromBody] string body)
        {
            try
            {
                //string clientSecret = configuration["NinjaVan:client_secret"];
                //string jsonBody = body;
                //string calculatedHmac = NinjaVanWebhookHelper.calculateHmac (jsonBody, clientSecret);
                //if (NinjaVanWebhookHelper.verifyWebhook(calculatedHmac, "HMAC-HEADER-RECEIVED-IN-REQUEST"))
                //{

                //}
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], body);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
    }
}
