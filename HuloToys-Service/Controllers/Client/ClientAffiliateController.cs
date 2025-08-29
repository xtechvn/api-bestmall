using Caching.Elasticsearch;
using Entities.Models;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Order.Business;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.IRepositories;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Client;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Models.Queue;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.constants;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Queue;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/client/affiliate")]

    public class ClientAffiliateController : ControllerBase
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
        private readonly IBankingAccountRepository bankingAccountRepository;
        private readonly OrderMergeESService orderMergeESService;
        private readonly OrderMongodbService orderMongodbService;

        public ClientAffiliateController(IConfiguration _configuration, RedisConn redisService, IClientRepository clientRepository, 
            IAccountClientRepository accountClientRepository, IBankingAccountRepository bankingAccountRepository,  OrderMongodbService orderMongodbService)
        {
            configuration = _configuration;
            workQueueClient = new WorkQueueClient(configuration);
            accountClientESService = new AccountClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            clientESService = new ClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            _identifierServiceRepository = new IdentiferService(_configuration);
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            clientServices = new ClientServices(configuration);
            _emailService = new EmailService(configuration);
            _clientRepository = clientRepository;
            _accountClientRepository = accountClientRepository;
            this.bankingAccountRepository = bankingAccountRepository;
            orderMergeESService = new OrderMergeESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            this.orderMongodbService = orderMongodbService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> AffiliateRegister([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientAddressGeneralRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    if(client == null || client.Id<=0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (client != null && client.IsRegisterAffiliate==true && client.ReferralId!=null && client.ReferralId.Trim()!="") {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = new {
                                utm_source="bestmall",
                                utm_medium= client.ReferralId
                            }
                        });

                    }
                    var client_sql = await _clientRepository.GetClientDetailByClientId((long)account_client.ClientId);
                    if (client_sql != null && client_sql.Id>0) { 
                        if(client_sql.IsRegisterAffiliate == true && client_sql.ReferralId!=null && client_sql.ReferralId.Trim() != "")
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Success",
                                data = new
                                {
                                    utm_source = "bestmall",
                                    utm_medium = client_sql.ReferralId
                                }
                            });
                        }
                        client_sql.IsRegisterAffiliate = true;
                        client_sql.ReferralId = await clientServices.GenerateRefferalID(client_sql.Id, DateTime.Now);
                        _clientRepository.SetUpClient(client_sql);
                        var j_param = new Dictionary<string, object>
                        {
                            { "store_name", "sp_GetClient" },
                            { "index_es", "hulotoys_sp_getclient"},
                            { "project_type", 1 },
                            { "id", client_sql.Id}
                        };

                        var _data_push = JsonConvert.SerializeObject(j_param);
                        var response_queue = workQueueClient.InsertQueueSimpleSyncES(_data_push);
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = new
                            {
                                utm_source = "bestmall",
                                utm_medium = client_sql.ReferralId
                            }
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
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("detail")]
        public async Task<ActionResult> AffiliatePayment([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientAddressGeneralRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    if (client == null || client.Id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var detailclient = await clientServices.GetDetailClientIdFromToken(account_client_id);

                    List<BankingAccount > accounts = new List<BankingAccount>();
                    var cache_name = CacheType.BANK_ACCOUNT + client.Id;
                    string j_data = "";
                    try
                    {
                        j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"]));
                    }
                    catch { }
                    if (j_data != null && j_data.Trim() != "")
                    {
                        try
                        {
                            accounts = JsonConvert.DeserializeObject<List<BankingAccount>>(j_data);
                        }
                        catch { }

                        if (accounts != null && accounts.Count>0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Success",
                                data = accounts[0],
                                client=detailclient
                            });
                        }
                    }
                    accounts = bankingAccountRepository.GetBankAccountByClientId(client.Id);

                    if (accounts != null && accounts.Count > 0)
                    {
                        _redisService.Set(cache_name,JsonConvert.SerializeObject(accounts), Convert.ToInt32(configuration["Redis:Database:db_search_result"]));

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = accounts[0],
                            client = detailclient

                        });
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = new BankingAccount(),
                        client = detailclient

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
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("update")]
        public async Task<ActionResult> AffiliatePaymentUpdate([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientAffiliatePaymentRequestModel>(objParr[0].ToString());
                    if (request == null || request.token==null || request.token.Trim()==""
                        ||request.detail==null 
                        || request.detail.AccountNumber == null || request.detail.AccountNumber.Trim() == ""
                        || request.detail.BankId == null || request.detail.BankId.Trim() == ""
                        || request.detail.Branch == null || request.detail.Branch.Trim() == ""
                        )
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    if (client == null || client.Id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var cache_name = CacheType.BANK_ACCOUNT + client.Id;

                    var banking_payment = bankingAccountRepository.GetBankAccountByClientId(request.detail.Id);
                    int id = 0;
                    if (banking_payment != null && banking_payment.Count > 0 && request.detail.Id > 0 && (banking_payment[0].Id !=request.detail.Id || banking_payment[0].ClientId != client.Id))
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (banking_payment != null && banking_payment.Count > 0)
                    {
                        var exists = banking_payment[0];
                        exists.Branch = request.detail.Branch;
                        exists.AccountName = request.detail.AccountName;
                        exists.AccountNumber = request.detail.AccountNumber;
                        exists.BankId = request.detail.BankId;
                        exists.UpdatedBy = (int)client.Id;
                        exists.UpdatedDate = DateTime.Now;
                        id=bankingAccountRepository.UpsertBankingAccount(exists);
                    }
                    else
                    {
                        request.detail.CreatedBy= (int)client.Id;
                        request.detail.CreatedDate=DateTime.Now;
                        request.detail.UpdatedBy = (int)client.Id;
                        request.detail.UpdatedDate = DateTime.Now;
                        request.detail.ClientId= (int)client.Id;
                        request.detail.SupplierId= (int)client.Id;
                        id = bankingAccountRepository.UpsertBankingAccount(request.detail);
                    }
                    _redisService.clear(cache_name,  Convert.ToInt32(configuration["Redis:Database:db_search_result"]));

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = id
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
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("order/listing")]
        public async Task<ActionResult> AffiliateOrderListing([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.page_index <= 0 || request.page_size <= 0
                       )
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    if (request.status == "-1") request.status = "";
                    if (request.order_no == null) request.order_no = "";
                    if(client.IsRegisterAffiliate==null || client.IsRegisterAffiliate == false 
                        ||client.ReferralId==null ||client.ReferralId.Trim()=="")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Tài khoản khách hàng chưa được đăng ký Affiliate"
                        });
                    }
                    var result = orderMergeESService.GetFEAffiliateByClientID((long)account_client.ClientId, request.status, request.order_no, (request.page_index <= 0 ? 1 : request.page_index), (request.page_size <= 0 ? 10 : request.page_size),client.ReferralId);
                    if (result != null && result.data != null && result.data.Count > 0)
                    {
                        var list_order_no = result.data.Select(x => x.OrderNo).ToList();
                        result.data_order = await orderMongodbService.GetListByOrdersNo(list_order_no);
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = result
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
                msg = ResponseMessages.DataInvalid
            });

        }
      
    }
}
