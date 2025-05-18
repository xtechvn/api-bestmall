using Caching.Elasticsearch;
using Models.APIRequest;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using HuloToys_Service.MongoDb;
using Models.MongoDb;
using HuloToys_Service.Models.APP;
using HuloToys_Service.Utilities.constants.APP;
using HuloToys_Service.Controllers.Order.Business;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Controllers.Client.Business;
using App_Push_Consummer.Model.Comments;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Controllers.Shipping.Business;
using Entities.Models;
using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly WorkQueueClient workQueueClient;
        private readonly OrderESService orderESRepository;
        private readonly OrderMongodbService orderMongodbService;
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly AccountClientESService accountClientESService;
        private readonly CartMongodbService _cartMongodbService;
        private readonly WorkQueueClient work_queue;
        private readonly IdentiferService identiferService;
        private readonly RedisConn _redisService;
        private readonly ClientServices clientServices;
        private readonly ClientESService clientESService;
        private readonly RaitingESService raitingESService;
        private readonly ShippingBussinessSerice shippingBussinessSerice;

        public OrderController(IConfiguration _configuration, RedisConn redisService)
        {
            configuration = _configuration;

            workQueueClient = new WorkQueueClient(configuration);
            orderESRepository = new OrderESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            accountClientESService = new AccountClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            orderMongodbService = new OrderMongodbService( configuration);
            _productDetailMongoAccess = new ProductDetailMongoAccess( configuration);
            _cartMongodbService = new CartMongodbService(configuration);
            work_queue = new WorkQueueClient(configuration);
            identiferService = new IdentiferService(_configuration);
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            clientServices = new ClientServices(_configuration);
            clientESService = new ClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            shippingBussinessSerice = new ShippingBussinessSerice(_configuration);

        }

        [HttpPost("history")]
        public async Task<ActionResult> OrderHistory([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null )
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

                    var result=  orderESRepository.GetByClientID(client.Id);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data= result
                    });

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
        [HttpPost("fe-history")]
        public async Task<ActionResult> OrderFEHistory([FromBody] APIRequestGenericModel input)
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

                    var cache_name = CacheType.ORDER_DETAIL_FE + client.Id+request.status+request.page_index+request.page_size;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"]));
                    if (j_data != null && j_data.Trim() != "")
                    {
                        OrderFEResponseModel data = JsonConvert.DeserializeObject<OrderFEResponseModel>(j_data);
                        if (data != null && data.data != null&& data.data.Count>0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = ResponseMessages.Success,
                                data = data
                            });
                        }
                    }
                    var result = orderESRepository.GetFEByClientID((long)account_client.ClientId, request.status, (request.page_index <= 0 ? 1 : request.page_index), (request.page_size <= 0 ? 10 : request.page_size));
                    if(result!=null && result.data!=null && result.data.Count > 0)
                    {
                        result.data_order = await orderMongodbService.GetListByOrdersNo(result.data.Select(x => x.OrderNo).ToList());
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
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("history-lastest")]
        public async Task<ActionResult> OrderHistoryLastestItem([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null)
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

                    var result = orderESRepository.GetLastestClientID(client.Id);


                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data=result
                    });

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
        [HttpPost("history-find")]
        public async Task<ActionResult> OrderHistoryFind([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.order_no ==null || request.order_no.Trim()=="")
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
                    var order=orderESRepository.GetByOrderNo(request.order_no,client.Id);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data= order
                    });

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
        [HttpPost("detail")]
        public async Task<ActionResult> Detail([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersGeneralRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == ""
                        )
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var result = await orderMongodbService.FindById(request.id);
                   
                    if (result != null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = result
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
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("history-detail")]
        public async Task<ActionResult> HistoryDetail([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryDetailRequestModel>(objParr[0].ToString());
                    if (request == null || request.id <= 0 || request.token == null || request.token.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    OrderDetailResponseModel result = new OrderDetailResponseModel()
                    {
                        data = orderESRepository.GetByOrderId(request.id)
                    };
                    if (result.data == null)
                    {
                        LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"],
                            "HistoryDetail - OrderController orderESRepository.GetByOrderId("+request.id+") : NULL");

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    else
                    {
                        LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"],
                           "HistoryDetail - OrderController orderESRepository.GetByOrderId(" + request.id + ") : "+JsonConvert.SerializeObject(result.data));
                        result.data_order = await orderMongodbService.GetByOrderNo(result.data.OrderNo);
                    }

                    var provinces = _redisService.Get(CacheType.PROVINCE, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var district = _redisService.Get(CacheType.DISTRICT, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var ward = _redisService.Get(CacheType.WARD, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    if (result.data.ProvinceId>0&& provinces != null && provinces.Trim() != "")
                    {
                        var data = JsonConvert.DeserializeObject<List<Province>>(provinces);
                        result.province = data.FirstOrDefault(x => x.Id == result.data.ProvinceId);
                    }
                    if (result.data.DistrictId > 0 && district != null && district.Trim() != "")
                    {
                        var data = JsonConvert.DeserializeObject<List<District>>(district);
                        result.district = data.FirstOrDefault(x => x.Id == result.data.DistrictId);
                    }
                    if (result.data.WardId > 0 && ward != null && ward.Trim() != "")
                    {
                        var data = JsonConvert.DeserializeObject<List<Ward>>(ward);
                        result.ward = data.FirstOrDefault(x => x.Id == result.data.WardId);
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

                    var raiting_count = raitingESService.CountCommentByOrderID(request.id, (long)account_client.ClientId);
                    result.has_raiting=raiting_count> 0;
                    if (result != null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = result
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
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("confirm")]
        public async Task<ActionResult> Confirm([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<CartConfirmRequestModel>(objParr[0].ToString());
                    if (request == null 
                        || request.carts == null || request.carts.Count <= 0)
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

                    var count = await orderESRepository.CountOrderByYear();
                    if (count < 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.FunctionExcutionFailed
                        });
                    }
                    var order_no = await identiferService.buildOrderNo(count);
                    var model = new OrderDetailMongoDbModel()
                    {
                        account_client_id = account_client_id,
                        carts = new List<CartItemMongoDbModel>(),
                        payment_type = request.payment_type,
                        delivery_detail = request.delivery_detail,
                        order_no = order_no,
                        total_amount=0,
                        address=request.address.Address,
                        districtid=request.address.DistrictId,
                        provinceid=request.address.ProvinceId,
                        wardid=request.address.WardId,
                        address_id=request.address_id,
                        receivername=request.address.ReceiverName,
                        phone=request.address.Phone,
                    };
                    
                    foreach (var item in request.carts)
                    {
                        var cart = await _cartMongodbService.FindById(item.id);
                        if (cart == null
                            || cart._id == null || cart._id.Trim() == ""
                            || cart.account_client_id != account_client_id)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = ResponseMessages.DataInvalid
                            });
                        }
                        else
                        {
                            cart.product= await _productDetailMongoAccess.GetByID(cart.product._id);
                            cart.quanity = item.quanity;
                            cart.total_price = cart.product.price * item.quanity;
                            cart.total_profit = cart.product.profit * item.quanity;
                            cart.total_amount = cart.product.amount * item.quanity;
                            cart.total_discount = cart.product.discount * item.quanity;
                            model.total_price += cart.total_price;
                            model.total_profit += cart.total_profit;
                            model.total_amount += cart.total_amount;
                            model.total_discount += cart.total_discount;
                            model.carts.Add(cart);

                            await _cartMongodbService.Delete(item.id);
                        }

                    }
                    //-- Shipping fee
                    //var shipping_fee = await shippingBussinessSerice.GetShippingFeeResponse(request.delivery_detail);
                    //shipping_fee ??= new Models.NinjaVan.ShippingFeeResponseModel();
                    //if (shipping_fee.total_shipping_fee <= 0) shipping_fee.total_shipping_fee = 0;
                    //model.shipping_fee = shipping_fee.total_shipping_fee;
                    //model.total_amount += shipping_fee.total_shipping_fee;
                    model.shipping_fee = 0;
                    //-- Mongodb:
                    var result = await orderMongodbService.Insert(model);
                    //-- Insert Queue:
                    var queue_model = new CheckoutQueueModel() { event_id = (int)CheckoutEventID.CREATE_ORDER, order_mongo_id = result };
                   

                    var pushed_queue =work_queue.InsertQueueSimpleDurable(JsonConvert.SerializeObject(queue_model) , QueueName.QUEUE_CHECKOUT);
                    LogHelper.InsertLogTelegram(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "Push Queue: "
                       + QueueName.QUEUE_CHECKOUT
                       + "[" + JsonConvert.SerializeObject(queue_model) + "] ["+pushed_queue+"]");

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = new OrderConfirmResponseModel { order_no = order_no, id = result, pushed= pushed_queue }
                    });
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
                msg = ResponseMessages.FunctionExcutionFailed
            });
        } 
        
        [HttpPost("insert-raiting")]
        public async Task<ActionResult> InsertRaiting([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductInsertRaitingRequestModel>(objParr[0].ToString());
                    if (request == null 
                        || request.order_id <= 0
                        || request.token == null || request.token.Trim()=="")
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
                    string main_product_id = request.product_id;
                    var product = await _productDetailMongoAccess.GetByID(request.product_id);
                    if(product!=null && product.parent_product_id!=null && product.parent_product_id.Trim() != "")
                    {
                        main_product_id=product.parent_product_id;
                    }
                    ProductRaitingPushQueueModel model = new ProductRaitingPushQueueModel()
                    {
                         UserId= (long)account_client.ClientId,
                         Comment= request.comment,
                         CreatedDate=DateTime.UtcNow.ToLocalTime(),
                         ImgLink=request.img_link,
                         OrderId=request.order_id,
                         ProductId= main_product_id,
                         ProductDetailId=request.product_id,
                         VideoLink=request.video_link,
                         Star=request.star,
                        
                    };
                    var queue_model = new
                    {
                        type = QueueType.INSERT_PRODUCT_RATING,
                        data_push = JsonConvert.SerializeObject(model)
                    };
                    var pushed_queue=work_queue.InsertQueueSimple(JsonConvert.SerializeObject(queue_model) , QueueName.queue_app_push);
                   
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                    });
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
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
        
    }
}
