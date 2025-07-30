using Entities.ViewModels;
using HuloToys_Service.Controllers.Payment.Bussiness;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Models.Payment;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Globalization;
using System.Net;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using static Utilities.Contants.DepositHistoryConstant;

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
        private readonly IOrderRepository _orderRepository;
        private readonly VNPayService _vNPayService;
        private readonly IContractPayRepository _contractPayRepository;
        private readonly IIdentifierServiceRepository identifierServiceRepository;

        public PaymentController(IConfiguration _configuration, OrderMongodbService orderMongodbService, VNPayService vNPayService, IOrderRepository orderRepository, IContractPayRepository contractPayRepository, IIdentifierServiceRepository identifierServiceRepository)
        {
            configuration = _configuration;
            workQueueClient = new WorkQueueClient(configuration);
            _vietQRServices = new VietQRServices(configuration);
            _orderMongodbService = orderMongodbService;
            _vNPayService = vNPayService;
            _orderRepository = orderRepository;
            _contractPayRepository = contractPayRepository;
            this.identifierServiceRepository = identifierServiceRepository;
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
                //    country = "vn",
                //    id = "6879f240342fee95f056deec"
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

            //var model_con = new
            //{
            //    response_from_vnpay = "https://bestmall.com.vn/order/payment/6879f240342fee95f056deec?vnp_Amount=60100000&vnp_BankCode=NCB&vnp_BankTranNo=VNP15106591&vnp_CardType=ATM&vnp_OrderInfo=thanh+toan+don+hang+25071800611+ID+6879f240342fee95f056deec&vnp_PayDate=20250729232627&vnp_ResponseCode=00&vnp_TmnCode=BMTES1TT&vnp_TransactionNo=15106591&vnp_TransactionStatus=00&vnp_TxnRef=25071800611&vnp_SecureHash=3ea6b7f545638b30a0bb33ed816c9cb81301a96854e4240056774d5b70afca6a03a9a421b07507904b07344d04b5913a6014b2c9125f3c7516e88327c05bdaf9"
            //};
            //input.token = CommonHelper.Encode(JsonConvert.SerializeObject(model_con), configuration["KEY:private_key"]);
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
                    //foreach (KeyValuePair<string, string> kv in request_input)
                    //{
                    //    if (!string.IsNullOrEmpty(kv.Value))
                    //    {
                    //        url_part+= WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&";
                    //    }
                    //}
                    ////remove last '&'
                    //if (url_part.Length > 0)
                    //{
                    //    url_part.Remove(url_part.Length - 1, 1);
                    //}
                    List<string> queryParams=new List<string>();
                    foreach (var prop in request_input)
                    {
                        var value = prop.Value;
                        queryParams.Add($"{prop.Key}={WebUtility.UrlEncode(value)}");
                    }
                    url_part= string.Join("&", queryParams);
                    var validate  =await _vNPayService.ValidateURL(url_part, vnp_SecureHash);
                    if (validate)
                    {
                        if (
                            parameters.ContainsKey("vnp_ResponseCode") && parameters["vnp_ResponseCode"].Trim()=="00"
                           && parameters.ContainsKey("vnp_TransactionStatus") && parameters["vnp_TransactionStatus"].Trim()=="00"

                            )
                        {
                            long vnp_Amount = Convert.ToInt64(parameters["vnp_Amount"]) / 100;
                            string order_no = parameters["vnp_TxnRef"];
                            string vnpayTranId = parameters["vnp_TransactionNo"];
                            string vnp_ResponseCode = parameters["vnp_ResponseCode"];
                            string vnp_TransactionStatus = parameters["vnp_TransactionStatus"];
                            string vnp_CardType = parameters["vnp_CardType"];
                            string vnp_BankTranNo = parameters["vnp_BankTranNo"];
                            string vnp_BankCode = parameters["vnp_BankCode"];
                            string vnp_OrderInfo = parameters["vnp_OrderInfo"];
                            string vnp_PayDate_str = parameters["vnp_PayDate"];
                            DateTime vnp_PayDate = DateTime.ParseExact(vnp_PayDate_str, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                            //-- Check exists contractpay_detail
                            var exists_contractpay_detail = _contractPayRepository.ContractPayDetailByServiceCode(vnpayTranId.Trim() + vnp_BankTranNo.Trim());
                            if (exists_contractpay_detail != null && exists_contractpay_detail.Id > 0)
                            {

                                return Ok(new
                                {
                                    status = (int)ResponseType.SUCCESS,
                                    msg = "Giao dịch đã tồn tại",
                                    data = new
                                    {
                                        amount = vnp_Amount,
                                        order_no = order_no,
                                        created_date = vnp_PayDate.ToString("dd/MM/yyyy HH:mm:ss")
                                    }
                                });

                            }
                            //-- Check order
                            var order=_orderRepository.GetByOrderNo(order_no);
                            if(order==null || order.OrderId <= 0)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.FAILED,
                                    msg = "Thanh toán qua cổng VNPAY thất bại, vui lòng thử thanh toán lại hoặc liên hệ CSKH",
                                    data = new
                                    {
                                        amount = 0,
                                        order_no = "",
                                        created_date = ""
                                    }
                                });
                            }
                            //-- create contract pay:
                            var contractpay_create = new ContractPayViewModel() {
                                Type = 1,
                                PayType = (int)CONTRACT_PAYMENT_TYPE.VNPAY,
                                BankingAccountId = 1,
                                Description = "Thu tiền thanh toán thông qua VNPAY cho đơn hàng "+order_no,

                                Note = "Ngày giao dịch: "+ vnp_PayDate.ToString("dd/MM/yyyy HH:mm:ss")
                                + ". Mã giao dịch ngân hàng" + vnp_BankTranNo
                                + ". Ngày giao dịch VNPAY: " + vnpayTranId
                                + ". Ngày giao dịch: " + vnp_PayDate.ToString("dd/MM/yyyy HH:mm:ss")
                                ,
                                ClientId = (int)order.ClientId,
                                SupplierId = null,
                                ObjectType = 1,
                                EmployeeId = 1,
                                ExportDate= vnp_PayDate,
                                CreatedDate=DateTime.Now,
                                CreatedBy=1,
                                UpdatedBy=1,
                                UpdatedDate=DateTime.Now,
                                Amount = vnp_Amount,
                                ContractPayDetails = new List<ContractPayDetailViewModel>()
                                {
                                    new ContractPayDetailViewModel()
                                    {
                                        Amount = vnp_Amount,
                                        CreatedBy=1,
                                        ServiceId=1,
                                        OrderId=(int)order.OrderId,
                                        AmountOrder=(double)order.Amount,
                                        TotalNeedPayment=(double)order.Amount,
                                        ServiceCode=vnpayTranId.Trim()+vnp_BankTranNo.Trim(),
                                        ServiceType=1,
                                         
                                    }
                                },
                            };
                            contractpay_create.BillNo = await identifierServiceRepository.buildContractPay();
                            var contractPayId = _contractPayRepository.CreateContractPay(contractpay_create);
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Thanh toán qua cổng VNPAY thành công",
                                data = new
                                {
                                    amount=vnp_Amount,
                                    order_no= order.OrderNo,
                                    created_date= vnp_PayDate.ToString("dd/MM/yyyy HH:mm:ss")
                                }
                            });
                        }
                      
                           
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Thanh toán qua cổng VNPAY thất bại, vui lòng thử thanh toán lại hoặc liên hệ CSKH",
                        data = new
                        {
                            amount = 0,
                            order_no = "",
                            created_date = ""
                        }
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
