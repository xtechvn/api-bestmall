using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using HuloToys_Service.Utilities.Lib;
using Utilities;
using Utilities.Contants;
using Models.Queue;
using HuloToys_Service.RabitMQ;
using Caching.Elasticsearch;
using HuloToys_Service.Models.Queue;
using HuloToys_Service.Utilities.constants;
using HuloToys_Service.Controllers.Order.Business;
using HuloToys_Service.Models.Client;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using Entities.Models;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Controllers.Client.Business;
using Nest;
using HuloToys_Service.IRepositories;
using Repositories.IRepositories;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ClientController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly WorkQueueClient workQueueClient;
        private readonly AccountClientESService accountClientESService;
        private readonly ClientESService clientESService;
        private readonly IdentiferService _identifierServiceRepository;
        private readonly ClientServices clientServices;
        private readonly RedisConn _redisService;
        private readonly EmailService _emailService;
        private readonly IClientRepository _clientRepository;
        private readonly IAccountClientRepository _accountClientRepository;

        public ClientController(IConfiguration _configuration, RedisConn redisService, IClientRepository clientRepository, IAccountClientRepository accountClientRepository) {
            configuration= _configuration;
            workQueueClient=new WorkQueueClient(configuration);
            accountClientESService = new AccountClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            clientESService = new ClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            _identifierServiceRepository = new IdentiferService(_configuration);
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            clientServices = new ClientServices(configuration);
            _emailService = new EmailService(configuration);
            _clientRepository = clientRepository;
            _accountClientRepository = accountClientRepository;
        }
        [HttpPost("login")]
        public async Task<ActionResult> ClientLogin([FromBody] APIRequestGenericModel input)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientLoginRequestModel>(objParr[0].ToString());
                    if (request == null 
                        || request.user_name == null || request.user_name.Trim() == ""
                        || request.password == null || request.password.Trim() == ""
                        || request.type<0)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Tài khoản / Mật khẩu không chính xác, vui lòng thử lại"
                        });
                    }
                    switch (request.type)
                    {
                        case (int)AccountLoginType.Password:
                            {
                                string user_name = StringHelper.RemoveSpecialCharacterUsername(request.user_name.Trim());
                                string password = StringHelper.RemoveSpecialCharacter(request.password.Trim());
                                //-- By Username 
                                var account = accountClientESService.GetByUsernameAndPassword(user_name, password);
                                if (account != null && account.Id > 0)
                                {
                                    var client = clientESService.GetById((long)account.ClientId);
                                    if (client != null && client.Id > 0)
                                    {
                                        var token = await clientServices.GenerateToken(account.UserName, ipAddress);
                                        return Ok(new
                                        {
                                            status = (int)ResponseType.SUCCESS,
                                            msg = "Success",
                                            data = new ClientLoginResponseModel()
                                            {
                                                //account_client_id = account_client_exists.id,
                                                user_name = account.UserName,
                                                name = client.ClientName,
                                                token = token,
                                                ip = ipAddress,
                                                time_expire = clientServices.GetExpiredTimeFromToken(token)
                                            }
                                        });
                                    }
                                }
                                //-- By Email  || Phone:
                                var email_part = user_name.Split("@")[0].Trim();
                                var clients = clientESService.GetByEmail(email_part);
                                clients.AddRange(clientESService.GetByPhone(user_name));
                                if (clients != null && clients.Count > 0)
                                {
                                    foreach(var client in clients)
                                    {
                                        var account_client = accountClientESService.GetByClientIdAndPassword(client.Id, password);
                                        if (account_client != null && account_client.Id > 0)
                                        {
                                            var token = await clientServices.GenerateToken(account_client.UserName, ipAddress);
                                            return Ok(new
                                            {
                                                status = (int)ResponseType.SUCCESS,
                                                msg = "Success",
                                                data = new ClientLoginResponseModel()
                                                {
                                                    //account_client_id = account_client_exists.id,
                                                    user_name = account_client.UserName,
                                                    name = client.ClientName,
                                                    token = token,
                                                    ip = ipAddress,
                                                    time_expire = clientServices.GetExpiredTimeFromToken(token)
                                                }
                                            });
                                        }
                                    }
                                }
                            }
                            break;
                        case (int)AccountLoginType.Google:
                            {
                                var email_part = request.user_name.Split("@")[0].Trim();
                                var clients = clientESService.GetByEmail(email_part);
                                if (clients != null && clients.Count > 0)
                                {
                                    clients = clients.Where(x => x.Email.Trim().ToLower() == request.user_name.Trim().ToLower()).ToList();
                                    if (clients == null || clients.Count <= 0)
                                    {
                                        return Ok(new
                                        {
                                            status = (int)ResponseType.FAILED,
                                            msg = "Không tìm thấy tài khoản nào tương ứng với thông tin đăng nhập này, vui lòng thử lại"
                                        });
                                    }
                                    foreach (var client in clients)
                                    {
                                        var account_client = accountClientESService.GetByClientID(client.Id);

                                        if (account_client != null && account_client.Id > 0 && account_client.ClientId > 0)
                                        {
                                            var token = await clientServices.GenerateToken(account_client.UserName, ipAddress);
                                            return Ok(new
                                            {
                                                status = (int)ResponseType.SUCCESS,
                                                msg = "Success",
                                                data = new ClientLoginResponseModel()
                                                {
                                                    //account_client_id = account_client_exists.id,
                                                    user_name = account_client.UserName,
                                                    name = client.ClientName,
                                                    token = token,
                                                    ip = ipAddress,
                                                    time_expire = clientServices.GetExpiredTimeFromToken(token)
                                                }
                                            });
                                        }
                                    }
                                }
                                   
                            }
                            break;
                        default:
                            {

                            }break;
                    }
                   
                   

                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }

        [HttpPost("register")]
        public async Task<ActionResult> ClientRegister([FromBody] APIRequestGenericModel input)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientRegisterRequestModel>(objParr[0].ToString());
                    if (request == null || request.user_name==null || request.user_name.Trim()==""
                        || request.phone == null || request.phone.Trim() == ""
                        || request.password == null || request.password.Trim() == ""
                        || request.confirm_password == null || request.confirm_password.Trim() == ""
                        || request.password.Trim() != request.confirm_password.Trim() ) {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if(request.email != null && request.email.Trim() != "")
                    {
                        var exists_client=clientESService.GetByEmail(request.email.Trim());
                        if (exists_client != null && exists_client.Count>0) {
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = ResponseMessages.ClientEmailExists
                            });

                        }

                    }
                    string username_generate = "u" + DateTime.Now.ToString("yyMMddHHmmss");
                    for (int i = 1; i < 999; i++)
                    {
                        var value = username_generate + i.ToString().PadLeft(3, '0');
                        var exists = accountClientESService.GetByUsername(value);
                        if (exists != null) { continue; }
                        else
                        {
                            username_generate = value; 
                            break;
                        }
                    }

                    AccountClientViewModel model = new AccountClientViewModel()
                    {
                        ClientId = -1,
                        ClientType = 0,
                        Email = request.email == null || request.email.Trim() == "" ? "" : request.email.Trim(),
                        Id = -1,
                        isReceiverInfoEmail = request.is_receive_email == true ? (byte)1 : (byte)0,
                        Name = request.user_name.Trim(),
                        ClientName = request.user_name.Trim(),
                        Password = request.password,
                        Phone = request.phone,
                        Status = 0,
                        UserName = username_generate,
                        GoogleToken = request.token,
                        ClientCode =await _identifierServiceRepository.buildClientNo(0)
                    };
                    var queue_model = new ClientConsumerQueueModel()
                    {
                        data_push = JsonConvert.SerializeObject(model),
                        type = QueueType.ADD_USER
                    };
                    bool result= workQueueClient.InsertQueueSimple(JsonConvert.SerializeObject(queue_model),QueueName.queue_app_push);
                    if (result)
                    {

                        var token = await clientServices.GenerateToken(username_generate, ipAddress);
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = new ClientLoginResponseModel()
                            {
                                //account_client_id = account_client_exists.id,
                                user_name = username_generate,
                                name = request.user_name.Trim(),
                                token = token,
                                ip = ipAddress,
                                time_expire = clientServices.GetExpiredTimeFromToken(token)
                            }
                        });
                    }
                    
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientForgotPasswordRequestModel>(objParr[0].ToString());
                    if (request == null || request.name == null || request.name.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var client = clientESService.GetExactByEmail(request.name);
                    if (client != null&& client.Id > 0) {
                        var account_client = accountClientESService.GetByClientID((long)client.Id);
                        if (client != null && client.Id > 0 && account_client != null && account_client.Id > 0)
                        {
                            var forgot_password_object = new ClientForgotPasswordTokenModel()
                            {
                                account_client_id=account_client.Id,
                                client_id=client.Id,
                                email=client.Email,
                                user_name=account_client.UserName,
                                created_time=DateTime.Now,
                                exprire_time=DateTime.Now.AddMinutes(30)
                            };
                            string forgot_password_token = CommonHelper.Encode(JsonConvert.SerializeObject(forgot_password_object), configuration["KEY:private_key"]);
                            if (forgot_password_token != null && forgot_password_token.Trim()!="") {
                                //Generate new Forgot password token:
                                AccountClientViewModel model = new AccountClientViewModel()
                                {
                                    ClientId = client.Id,
                                    ClientType = 0,
                                    Email = null,
                                    Id = account_client.Id,
                                    isReceiverInfoEmail = null,
                                    Name = null,
                                    Password = null,
                                    Phone = null,
                                    Status = 0,
                                    UserName = null,
                                    ForgotPasswordToken = forgot_password_token
                                };
                                var queue_model = new ClientConsumerQueueModel()
                                {
                                    data_push = JsonConvert.SerializeObject(model),
                                    type = QueueType.UPDATE_USER
                                };
                                bool result = workQueueClient.InsertQueueSimple(JsonConvert.SerializeObject(queue_model), QueueName.queue_app_push);
                                _emailService.SendEmailChangePassword(forgot_password_token,account_client, client);
                                if (result)
                                {
                                    return Ok(new
                                    {
                                        status = (int)ResponseType.SUCCESS,
                                        msg = "Success"
                                    });
                                }


                            }
                        }
                    }
                   

                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }
       
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientChangePasswordRequestModel>(objParr[0].ToString());
                    if (request == null || request.id<=0
                        || request.password == null || request.password.Trim() == ""
                        || request.confirm_password == null || request.confirm_password.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }

                    var account_client = accountClientESService.GetById(request.id);
                    if (account_client != null && account_client.Id > 0 && account_client.ClientId > 0)
                    {
                        var client = clientESService.GetById((long)account_client.ClientId);
                        if (client != null && client.Id > 0)
                        {
                            string new_password = CommonHelper.MD5Hash(request.password);
                            //Generate new Forgot password token:


                            AccountClientViewModel model = new AccountClientViewModel()
                            {
                                ClientId = client.Id,
                                ClientType = 0,
                                Email = null,
                                Id = account_client.Id,
                                isReceiverInfoEmail = null,
                                Name = null,
                                Password = new_password,
                                Phone = null,
                                Status = 0,
                                UserName = null,
                                ForgotPasswordToken = ""
                            };
                            var queue_model = new ClientConsumerQueueModel()
                            {
                                data_push = JsonConvert.SerializeObject(model),
                                type = QueueType.UPDATE_USER
                            };
                            bool result = workQueueClient.InsertQueueSimple( JsonConvert.SerializeObject(queue_model), QueueName.queue_app_push);
                            if (result)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.SUCCESS,
                                    msg = "Success"
                                });
                            }
                        }
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
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("validate-forgot-password")]
        public async Task<ActionResult> ValidateForgotPassword([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientForgotPasswordRequestModel>(objParr[0].ToString());
                    if (request == null || request.name == null || request.name.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (string.IsNullOrEmpty(request.name) || request.name.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    string forgot = CommonHelper.Decode(request.name.Replace("-", "+").Replace("_", "/"), configuration["KEY:private_key"]);
                    if (forgot == null || forgot.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var model = JsonConvert.DeserializeObject<ClientForgotPasswordTokenModel>(forgot);
                    if (model == null || model.account_client_id <= 0 || model.exprire_time < DateTime.Now || model.created_time > DateTime.Now)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account = accountClientESService.GetById(model.account_client_id);
                    if(account!=null &&request.name.Trim()== account.ForgotPasswordToken)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }
    }
}
