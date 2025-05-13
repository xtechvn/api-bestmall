using HuloToys_Service.RabitMQ;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.MongoDb;
using static System.Runtime.InteropServices.JavaScript.JSType;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Payment;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly WorkQueueClient workQueueClient;
        private readonly VietQRServices _vietQRServices;
        private readonly OrderMongodbService _orderMongodbService;

        public PaymentController(IConfiguration _configuration)
        {
            configuration = _configuration;
            workQueueClient = new WorkQueueClient(configuration);
            _vietQRServices = new VietQRServices(configuration);
            _orderMongodbService = new OrderMongodbService(configuration);

        }
        [HttpPost("qr-code")]
        public async Task<ActionResult> QrCode([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersGeneralRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid,
                        });

                    }
                    var model = await _orderMongodbService.FindById(request.id);
                    if(model!=null && model._id!=null&& model._id.Trim()!="")
                    {
                        var qr = await _vietQRServices.GetVietQRCode(
                      configuration["BankTransfer:AccountNumber"]
                      , configuration["BankTransfer:AccountName"]
                      , Convert.ToInt32(configuration["BankTransfer:BankId"])
                      , model.order_no
                      , model.total_amount);
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = ResponseMessages.Success,
                            data = qr
                        });
                    }


                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });

        }
        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout([FromBody] APIRequestGenericModel  input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<PaymentCheckoutRequestModel>(objParr[0].ToString());
                    //if (request == null || request.user_name == null || request.user_name.Trim() == ""
                    //    || request.phone == null || request.phone.Trim() == ""
                    //    || request.password == null || request.password.Trim() == ""
                    //    || request.confirm_password == null || request.confirm_password.Trim() == ""
                    //    || request.password.Trim() != request.confirm_password.Trim())
                    //{

                    //    return Ok(new
                    //    {
                    //        status = (int)ResponseType.FAILED,
                    //        msg = "Dữ liệu không chính xác, vui lòng kiểm tra lại / liên hệ bộ phận chăm sóc để được hỗ trợ"
                    //    });
                    //}

                    //AccountClientViewModel model = new AccountClientViewModel()
                    //{
                    //    ClientId = -1,
                    //    ClientType = 0,
                    //    Email = request.email == null || request.email.Trim() == "" ? "" : request.email.Trim(),
                    //    Id = -1,
                    //    isReceiverInfoEmail = request.is_receive_email == true ? (byte)1 : (byte)0,
                    //    Name = request.user_name.Trim(),
                    //    Password = request.password,
                    //    Phone = request.phone,
                    //    Status = 0,
                    //    UserName = request.user_name
                    //};
                    //var queue_model = new ClientConsumerQueueModel()
                    //{
                    //    data_receiver = JsonConvert.SerializeObject(model),
                    //    queue_type = QueueType.ADD_USER
                    //};
                    //bool result = workQueueClient.InsertQueueSimple(new Models.QueueSettingViewModel()
                    //{
                    //    host = configuration["Queue:Host"],
                    //    port = Convert.ToInt32(configuration["Queue:Port"]),
                    //    v_host = configuration["Queue:V_Host"],
                    //    username = configuration["Queue:Username"],
                    //    password = configuration["Queue:Password"],
                    //}, JsonConvert.SerializeObject(queue_model), QueueName.queue_app_push);
                    //if (result)
                    //{
                    //    return Ok(new
                    //    {
                    //        status = (int)ResponseType.SUCCESS,
                    //        msg = "Success"
                    //    });
                    //}

                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Dữ liệu không chính xác, vui lòng kiểm tra lại / liên hệ bộ phận chăm sóc để được hỗ trợ"
            });

        }

    }
}
