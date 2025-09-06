using Caching.Elasticsearch;
using DAL;
using Entities.Models;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Order.Business;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.IRepositories;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Article;
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
using StackExchange.Redis;
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
        private readonly IAllotmentUseRepository _allotmentUseRepository;
        private readonly IAllotmentFundRepository _allotmentFundRepository;
        private readonly OrderMergeESService orderMergeESService;
        private readonly OrderMongodbService orderMongodbService;

        public ClientAffiliateController(IConfiguration _configuration, RedisConn redisService, IClientRepository clientRepository,
                    IAccountClientRepository accountClientRepository, IBankingAccountRepository bankingAccountRepository,
                    OrderMongodbService orderMongodbService, IAllotmentUseRepository allotmentUseRepository, IAllotmentFundRepository allotmentFundRepository)
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
            _allotmentUseRepository = allotmentUseRepository;
            _allotmentFundRepository = allotmentFundRepository;
        }

        [HttpPost("get")]
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
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    if (client == null || client.Id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (client != null && client.IsRegisterAffiliate == true && client.ReferralId != null && client.ReferralId.Trim() != "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = new
                            {
                                utm_source = "bestmall",
                                utm_medium = client.ReferralId
                            }
                        });

                    }
                    var client_sql = await _clientRepository.GetClientDetailByClientId((long)account_client.ClientId);
                    if (client_sql != null && client_sql.Id > 0)
                    {
                        if (client_sql.IsRegisterAffiliate == true && client_sql.ReferralId != null && client_sql.ReferralId.Trim() != "")
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
                        //client_sql.IsRegisterAffiliate = true;
                        //client_sql.ReferralId = await clientServices.GenerateRefferalID(client_sql.Id, DateTime.Now);
                        //_clientRepository.SetUpClient(client_sql);
                        //var j_param = new Dictionary<string, object>
                        //{
                        //    { "store_name", "sp_GetClient" },
                        //    { "index_es", "hulotoys_sp_getclient"},
                        //    { "project_type", 1 },
                        //    { "id", client_sql.Id}
                        //};

                        //var _data_push = JsonConvert.SerializeObject(j_param);
                        //var response_queue = workQueueClient.InsertQueueSimpleSyncES(_data_push);
                        //return Ok(new
                        //{
                        //    status = (int)ResponseType.SUCCESS,
                        //    msg = "Success",
                        //    data = new
                        //    {
                        //        utm_source = "bestmall",
                        //        utm_medium = client_sql.ReferralId
                        //    }
                        //});
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tài khoản chưa được đăng ký Affiliate"
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
                status = (int)ResponseType.ERROR,
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

                    List<BankingAccount> accounts = new List<BankingAccount>();
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

                        if (accounts != null && accounts.Count > 0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Success [Cached]",
                                data = accounts[0],
                                client = detailclient
                            });
                        }
                    }
                    accounts = bankingAccountRepository.GetBankAccountByClientId(client.Id);

                    if (accounts != null && accounts.Count > 0)
                    {
                        try
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(accounts), Convert.ToInt32(configuration["Redis:Database:db_search_result"]));

                        }
                        catch { }
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success [Direct]",
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
            //var model_input = new ClientAffiliatePaymentRequestModel
            //{
            //    token = "F08nOlAVBi8vLwxaDGMgagRjYX97aVlkfFt7AmJnTlpFXyNQYmNiUgBpXnt3Q1BJUlZ0WE5BcCxNFysoPCdLQhRzZWoEfmR2Y2hRBHlQcABqbFxBSQZqRm15alppZRI=",
            //    detail=new BankingAccount()
            //    {
            //        Id=0,
            //        BankId="VIETINBANK",
            //        AccountName="NGUYEN VAN A",
            //        AccountNumber= "0123456789",
            //        Branch= "HOI SO"
            //    },
            //};
            //input = new APIRequestGenericModel()
            //{
            //    token = CommonHelper.Encode(JsonConvert.SerializeObject(model_input), configuration["KEY:private_key"])
            //};
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientAffiliatePaymentRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == ""
                        || request.detail == null
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

                    var client_sql = await _clientRepository.GetClientDetailByClientId((long)account_client.ClientId);
                    if (client_sql != null && client_sql.Id > 0)
                    {
                        int id = 0;
                        if (client_sql.IsRegisterAffiliate == true && client_sql.ReferralId != null && client_sql.ReferralId.Trim() != "")
                        {
                            var banking_payment = await bankingAccountRepository.GetByClientId(client.Id);
                            if (banking_payment == null || banking_payment.Count <= 0)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.FAILED,
                                    msg = ResponseMessages.DataInvalid
                                });

                            }
                            else
                            {
                                var exists = banking_payment[0];
                                exists.Branch = request.detail.Branch;
                                exists.AccountName = request.detail.AccountName;
                                exists.AccountNumber = request.detail.AccountNumber;
                                exists.BankId = request.detail.BankId;
                                exists.UpdatedBy = (int)client.Id;
                                exists.UpdatedDate = DateTime.Now;
                                id = bankingAccountRepository.Update(exists);
                            }

                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Success",
                                data = new
                                {
                                    id = id,
                                    utm_source = "bestmall",
                                    utm_medium = client_sql.ReferralId
                                }
                            });
                        }
                        else
                        {
                            request.detail.CreatedBy = (int)client.Id;
                            request.detail.CreatedDate = DateTime.Now;
                            request.detail.UpdatedBy = (int)client.Id;
                            request.detail.UpdatedDate = DateTime.Now;
                            request.detail.ClientId = (int)client.Id;
                            request.detail.SupplierId = (int)client.Id;
                            id = bankingAccountRepository.Insert(request.detail);

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
                                    id = id,
                                    utm_source = "bestmall",
                                    utm_medium = client_sql.ReferralId
                                }
                            });
                        }
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
        [HttpPost("order/listing")]
        public async Task<ActionResult> AffiliateOrderListing([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
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
                    if (request.status == "-1") request.status = "";
                    if (request.order_no == null) request.order_no = "";
                    if (request.page_index <= 0) request.page_index = 1;
                    if (request.page_size <= 0) request.page_size = 10;

                    if (client.IsRegisterAffiliate == null || client.IsRegisterAffiliate == false
                        || client.ReferralId == null || client.ReferralId.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Tài khoản khách hàng chưa được đăng ký Affiliate"
                        });
                    }
                    var result = orderMergeESService.GetFEAffiliateByClientID((long)account_client.ClientId, request.status, request.order_no, request.page_index, request.page_size, new List<string>() { client.ReferralId });
                    if (result != null && result.data != null && result.data.Count > 0)
                    {
                        var list_order_no = result.data.Select(x => x.OrderNo).ToList();
                        result.data_order = await orderMongodbService.GetListByOrdersNo(list_order_no);
                        if (result.data_order == null) result.data_order = new List<OrderDetailMongoDbModel>();
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
        [HttpPost("payment/detail")]
        public async Task<ActionResult> AffiliatePaymentDetail([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    if (client == null || client.Id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (request.status == null || request.status == "-1") request.status = "";
                    if (request.order_no == null) request.order_no = "";
                    if (request.page_size <= 0) request.page_size = 10;
                    if (request.page_index <= 0) request.page_index = 1;
                    if (client != null && client.IsRegisterAffiliate == true && client.ReferralId != null && client.ReferralId.Trim() != "")
                    {
                        var cache_name = CacheType.ALLOTMENT_FUND + client.Id;
                        AllotmentFund result = new AllotmentFund();
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
                                result = JsonConvert.DeserializeObject<AllotmentFund>(j_data);
                            }
                            catch { }
                        }
                        if (result == null || result.Id <= 0)
                        {
                            result = _allotmentFundRepository.GetByAccountClientId(account_client_id);
                        }
                        if (result == null || result.Id <= 0)
                        {
                            result = new HuloToys_Service.Models.Models.AllotmentFund()
                            {
                                UpdateTime = DateTime.Now,
                                AccountBalance = 0,
                                AccountClientId = account_client_id,
                                CreateDate = DateTime.Now,
                                FundType = 1,

                            };
                            result.Id = _allotmentFundRepository.Insert(result);
                        }
                        var (totalCount, totalAmount) = orderMergeESService.GetOrderStatsByUtmMedium(new List<string>() { client.ReferralId });
                        if(result!=null && result.Id > 0)
                        {
                            try
                            {
                                _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(configuration["Redis:Database:db_search_result"]));

                            }
                            catch { }
                        }
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = result.AccountBalance,
                            total_amount = totalAmount,
                            count = totalCount,
                        });
                    }

                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tài khoản chưa được đăng ký Affiliate"
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
                status = (int)ResponseType.ERROR,
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("payment/listing")]
        public async Task<ActionResult> AffiliatePaymentListing([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    if (client == null || client.Id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (request.status == null || request.status == "-1") request.status = "";
                    if (request.order_no == null) request.order_no = "";
                    if (request.page_size <= 0) request.page_size = 10;
                    if (request.page_index <= 0) request.page_index = 1;
                    if (client != null && client.IsRegisterAffiliate == true && client.ReferralId != null && client.ReferralId.Trim() != "")
                    {

                        var cache_name = CacheType.ALLOTMENT_USE + client.Id+ request.page_index+ request.page_size;
                        GenericViewModel<AllotmentUse> result = new GenericViewModel<AllotmentUse>();
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
                                result = JsonConvert.DeserializeObject<GenericViewModel<AllotmentUse>>(j_data);
                            }
                            catch { }
                        }
                        if (result == null || result.ListData == null || result.ListData.Count <= 0)
                        {
                            result = _allotmentUseRepository.GetByAccountClientId(account_client_id);
                            if (result != null && result.ListData.Count > 0)
                            {
                                try
                                {
                                    _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(configuration["Redis:Database:db_search_result"]));

                                }
                                catch { }
                            }
                        }                        
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = result,
                        });

                    }

                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tài khoản chưa được đăng ký Affiliate"
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
                status = (int)ResponseType.ERROR,
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("payment/checkout")]
        public async Task<ActionResult> AffiliatePaymentCheckout([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientAffiliateRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == ""|| request.Amount<=0)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    if (client == null || client.Id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (client != null && client.IsRegisterAffiliate == true && client.ReferralId != null && client.ReferralId.Trim() != "")
                    {
                        var result = _allotmentFundRepository.GetByAccountClientId(account_client_id);
                        if (result == null || result.Id <= 0 || result.AccountBalance <= 5000)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = "Tài khoản chưa được đăng ký Affiliate / Số dư tài khoản không đủ để rút tiền"
                            });
                        }
                        var fund_use = new HuloToys_Service.Models.Models.AllotmentUse()
                        {
                            AllotmentFundId = result.Id,
                            AccountClientId = (long)result.AccountClientId,
                            AmountUse = request.Amount,
                            ClientId = client.Id,
                            CreateDate = DateTime.Now,
                            DataId = 0,
                            ServiceType = 1,
                            PaymentStatus=0
                        };
                        var id = _allotmentUseRepository.Insert(fund_use);
                        result.AccountBalance = request.Amount * -1;
                        _allotmentFundRepository.Update(result);
                        var cache_name = CacheType.ALLOTMENT_USE + client.Id;
                        try
                        {
                           await _redisService.DeleteCacheByKeyword(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"]));
                        }
                        catch { }
                        cache_name = CacheType.ALLOTMENT_FUND + client.Id;
                        try
                        {
                             _redisService.clear(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"]));
                        }
                        catch { }
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = new
                            {
                                id_payment = id,
                                payment_amount = request.Amount
                            }
                        });

                    }

                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tài khoản chưa được đăng ký Affiliate"
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
                status = (int)ResponseType.ERROR,
                msg = ResponseMessages.DataInvalid
            });

        }
    }
}
