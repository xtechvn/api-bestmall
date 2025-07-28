using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using HuloToys_Service.Controllers.Payment.Bussiness;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Models.Payment;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly VNPayService _vNPayService;

        public PaymentController(IConfiguration _configuration, OrderMongodbService orderMongodbService, VNPayService vNPayService)
        {
            configuration = _configuration;
            workQueueClient = new WorkQueueClient(configuration);
            _vietQRServices = new VietQRServices(configuration);
            _orderMongodbService = orderMongodbService;
            _vNPayService=vNPayService;

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
                      , configuration["BankTransfer:BankId"]
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Dữ liệu không chính xác, vui lòng kiểm tra lại / liên hệ bộ phận chăm sóc để được hỗ trợ"
            });

        }

        [HttpPost("vnpay/redirect")]
        public async Task<ActionResult> VNPayRedirectURL([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //OrdersVNPAYRequestModel json_input = new OrdersVNPAYRequestModel()
                //{
                //    client_ip = "42.113.119.131",
                //    country="vn",
                //    id= "687b0eb2f2064cab1d21b6cd"
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(json_input), configuration["KEY:private_key"])
                //};
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersVNPAYRequestModel>(objParr[0].ToString());
                    if (request == null 
                        || request.id == null || request.id.Trim() == ""
                        || request.client_ip == null || request.client_ip.Trim() == ""
                        )
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid,
                        });

                    }
                    if (request.country == null || !new List<string>() { "en","vi"}.Contains(request.country)) request.country = "vn";
                    var model = await _orderMongodbService.FindById(request.id);
                    if (model != null && model._id != null && model._id.Trim() != "")
                    {
                        
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = ResponseMessages.Success,
                            data = await _vNPayService.BuildURL(model,request.client_ip,request.country)
                        });
                    }


                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });

        }
        [HttpPost("vnpay/validate")]
        public async Task<ActionResult> VNPayValidateResponse([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersVNPAYValidateRequestModel>(objParr[0].ToString());
                    if (request == null
                        || request.response_from_vnpay == null || request.response_from_vnpay.Trim() == ""
                        )
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid,
                        });

                    }
                    Dictionary<string, string> parameters = StringHelpers.GetQueryParams(request.response_from_vnpay);
                    string vnp_SecureHash = "";
                    if (parameters.ContainsKey("vnp_SecureHash"))
                    {
                        vnp_SecureHash = parameters["vnp_SecureHash"];
                    }
                    var request_input = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(parameters));
                    if (request_input.ContainsKey("vnp_SecureHashType"))
                    {
                        request_input.Remove("vnp_SecureHashType");
                    }
                    if (request_input.ContainsKey("vnp_SecureHash"))
                    {
                        request_input.Remove("vnp_SecureHash");
                    }
                    string url_part = "";
                    foreach (KeyValuePair<string, string> kv in request_input)
                    {
                        if (!string.IsNullOrEmpty(kv.Value))
                        {
                            url_part+= WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&";
                        }
                    }
                    //remove last '&'
                    if (url_part.Length > 0)
                    {
                        url_part.Remove(url_part.Length - 1, 1);
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = _vNPayService.ValidateURL(url_part, vnp_SecureHash)
                    });
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });

        }
    }
}
