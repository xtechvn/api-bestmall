using HuloToys_Service.Models.Queue;
using HuloToys_Service.RabitMQ;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json.Linq;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.QUEUE
{
    [Route("api")]
    [ApiController]
    public class QueueController : Controller
    {
        private IConfiguration configuration;
        public QueueController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        [HttpPost("queue/insert-message.json")]
        public async Task<ActionResult> PushMessagetoQueue([FromForm]string token, [FromForm] string j_param_queue, [FromForm] string queue_name)
        {
            try
            {
                JArray objParr = null;
                /*
                // Tạo message để push vào queue
                    var j_param = new Dictionary<string, object>
                            {
                                  { "store_name", "Sp_GetAllArticle" },
                                { "index_es", "es_hulotoys_sp_get_article" },
                                {"project_type", Convert.ToInt16(ProjectType.HULOTOYS) },
                                  {"id" , Id }

                            };
                
                var data_product = JsonConvert.SerializeObject(j_param);
                var json_input = new Dictionary<string, object>
                {
                    {"j_param_queue",data_product},
                    {"queue_name", "queue_checkout_order"}
                    
                };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(json_input), configuration["DataBaseConfig:key_api:b2c"]);
                */
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]) 
                    || j_param_queue==null || j_param_queue.Trim()==""
                    || queue_name == null || queue_name.Trim() == "")
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }
               

                // setting queue
                var work_queue = new WorkQueueClient(configuration);
                var queue_setting = new QueueSettingViewModel
                {
                    host = configuration["Queue:Host"],
                    v_host = configuration["Queue:V_Host_Sync"],
                    port = Convert.ToInt32(configuration["Queue:Port"]),
                    username = configuration["Queue:Username"],
                    password = configuration["Queue:Password"]
                };

                // push queue
                var response_queue = work_queue.InsertQueueSimpleWithDurable(queue_setting, j_param_queue, queue_name);

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Successful"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg =ex.ToString()
                });
            }

        }       
       
    }
}
