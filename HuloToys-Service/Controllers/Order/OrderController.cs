using App_Push_Consummer.Model.Comments;
using Azure;
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Entities.Models;
using ENTITIES.ViewModels.Voucher;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Order.Business;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.Controllers.Shipping.Business;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.APP;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Models.Shipping.ViettelPost;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.constants.APP;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Models.APIRequest;
using Models.MongoDb;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using REPOSITORIES.IRepositories;
using StackExchange.Redis;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using static MongoDB.Driver.WriteConcern;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class OrderController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly WorkQueueClient workQueueClient;
       // private readonly OrderESService orderESRepository;
        private readonly OrderMergeESService orderMergeESService;
        private readonly OrderMongodbService orderMongodbService;
        // private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly AccountClientESService accountClientESService;
        private readonly CartMongodbService _cartMongodbService;
        private readonly WorkQueueClient work_queue;
        private readonly IdentiferService identiferService;
        private readonly RedisConn _redisService;
        private readonly ClientServices clientServices;
        private readonly ClientESService clientESService;
        private readonly RaitingESService raitingESService;
        private readonly ShippingBussinessSerice shippingBussinessSerice;
        private readonly ProductDetailService productDetailService;
        private readonly IVoucherRepository _voucherRepository;
        private readonly LocationESService locationESService;
        private readonly ViettelPostService _viettelPostService;
        private readonly SupplierESRepository _supplierESRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderController(IConfiguration _configuration, RedisConn redisService, IVoucherRepository voucherRepository, ViettelPostService viettelPostService,
            ProductDetailService _productDetailService, CartMongodbService cartMongodbService, OrderMongodbService _orderMongodbService, SupplierESRepository supplierESRepository,
            IOrderRepository orderRepository)
        {
            configuration = _configuration;

            workQueueClient = new WorkQueueClient(configuration);
           // orderESRepository = new OrderESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            orderMergeESService = new OrderMergeESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            locationESService = new LocationESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            accountClientESService = new AccountClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            orderMongodbService = _orderMongodbService;
            _viettelPostService = viettelPostService;
            work_queue = new WorkQueueClient(configuration);
            identiferService = new IdentiferService(_configuration);
            _redisService = redisService;
            try
            {
                _redisService.Connect();
            }
            catch { }
            clientServices = new ClientServices(_configuration);
            clientESService = new ClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            shippingBussinessSerice = new ShippingBussinessSerice(_configuration);
            _voucherRepository = voucherRepository;
            productDetailService = _productDetailService;
            _cartMongodbService = cartMongodbService;
            _supplierESRepository = supplierESRepository;
            _orderRepository= orderRepository;
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

                    var result = orderMergeESService.GetByClientID(client.Id);

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
                    if (request.order_no == null) request.order_no = "";
                   
                    var result = orderMergeESService.GetFEByClientID((long)account_client.ClientId, request.status, request.order_no, (request.page_index <= 0 ? 1 : request.page_index), (request.page_size <= 0 ? 10 : request.page_size));
                    if (result != null && result.data != null && result.data.Count > 0)
                    {
                        var list_order_no = result.data.Select(x => x.OrderNo).ToList();
                        LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], " orderMongodbService.GetListByOrdersNo:"+JsonConvert.SerializeObject(list_order_no));

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

                    var result = orderMergeESService.GetLastestClientID(client.Id);


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
        [HttpPost("history-find")]
        public async Task<ActionResult> OrderHistoryFind([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.order_no == null || request.order_no.Trim() == "")
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
                    var order = orderMergeESService.GetByOrderNo(request.order_no, client.Id);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = order
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
                    OrderMergeESModel order_es = new OrderMergeESModel();
                    Province province = new Province();
                    District district = new District();
                    Ward ward = new Ward();
                    if (result != null && result.order_id > 0)
                    {
                        order_es = orderMergeESService.GetByOrderId(result.order_id);
                        if (result.provinceid != null)
                        {
                            province = locationESService.GetProvincesByProvinceId(result.provinceid);
                        }
                        if (result.districtid != null)
                        {
                            district = locationESService.GetDistrictByDistrictId(result.districtid);
                        }
                        if (result.wardid != null)
                        {
                            ward = locationESService.GetWardsByWardId(result.wardid);
                        }
                    }
                   
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = result,
                        //data_order = order_es,
                        province,
                        district,
                        ward
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
                    OrderMergeDetailResponseModel result = new OrderMergeDetailResponseModel()
                    {
                        data = orderMergeESService.GetByOrderId(request.id)
                    };
                    if (result.data == null)
                    {
                        LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"],
                            "HistoryDetail - OrderController orderESRepository.GetByOrderId(" + request.id + ") : NULL");

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    else
                    {
                        LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"],
                           "HistoryDetail - OrderController orderESRepository.GetByOrderId(" + request.id + ") : " + JsonConvert.SerializeObject(result.data));
                        result.data_order = await orderMongodbService.GetByOrderNo(result.data.OrderNo);
                    }

                    var provinces = _redisService.Get(CacheType.PROVINCE, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var district = _redisService.Get(CacheType.DISTRICT, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var ward = _redisService.Get(CacheType.WARD, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    if (result.data.ProvinceId > 0 && provinces != null && provinces.Trim() != "")
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
                    result.has_raiting = raiting_count > 0;
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
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

                    var count = await orderMergeESService.CountOrderByYear();
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
                        total_amount = 0,
                        address = request.address.Address,
                        districtid = request.address.DistrictId,
                        provinceid = request.address.ProvinceId,
                        wardid = request.address.WardId,
                        address_id = request.address_id,
                        receivername = request.address.ReceiverName,
                        phone = request.address.Phone,
                        list_voucher_id = request.voucher_id,
                        list_voucher_code = request.voucher_code,
                         shipping_fee=0,
                         total_discount=0,
                         total_price=0,
                         total_profit=0,
                         delivery_type=request.delivery_detail.shipping_type
                         
                         
                    };
                    List<VoucherFEModel> voucher_apply = new List<VoucherFEModel>();
                    if (request.voucher_code != null && request.voucher_code.Count > 0)
                    {
                        model.list_voucher_code = request.voucher_code.Select(x => x.ToUpper().Trim()).ToList();
                        string cache_name = CacheType.VOUCHER + account_client_id;
                        var str = _redisService.Get(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"]));
                        if (str != null && str.Trim() != "")
                        {
                            try
                            {
                                List<VoucherFEModel> list = JsonConvert.DeserializeObject<List<VoucherFEModel>>(str);
                                if (list != null && list.Count > 0)
                                {
                                    var selected_list = list.Where(x => model.voucher_code.Contains(x.code.Trim().ToUpper()));
                                    if (selected_list != null && selected_list.Count() > 0)
                                    {
                                        voucher_apply = selected_list.ToList();
                                    }
                                }
                            }
                            catch { }

                        }
                        if (voucher_apply == null || voucher_apply.Count <= 0)
                        {
                            var data = await _voucherRepository.GetListVoucher(model.list_voucher_code);
                            if(data!=null && data.Count > 0)
                            {
                                voucher_apply = JsonConvert.DeserializeObject<List<VoucherFEModel>>(JsonConvert.SerializeObject(data));
                            }
                        }
                       
                    }
                    var list_cart =new List<CartItemMongoDbModel>();
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
                            list_cart.Add(cart);

                            cart.product = await productDetailService.GetByID(cart.product._id);
                            var amount = cart.product.amount;
                            var price = cart.product.price;
                            var profit = cart.product.profit;
                            var discount = cart.product.discount;

                            if (cart.product.amount_after_flashsale != null && cart.product.amount_after_flashsale > 0)
                            {
                                amount = (double)cart.product.amount_after_flashsale;
                                profit = amount - price;
                                discount = cart.product.amount - (double)cart.product.amount_after_flashsale;
                            }
                            cart.quanity = item.quanity;
                            cart.total_price = price * item.quanity;
                            cart.total_profit = profit * item.quanity;
                            cart.total_profit = cart.product.profit * item.quanity;
                            cart.total_amount = amount * item.quanity;
                            cart.total_discount = cart.product.discount / cart.quanity * item.quanity;
                            model.total_price += cart.total_price;
                            model.total_profit += cart.total_profit;
                            model.total_amount += cart.total_amount;
                            model.total_amount_product += cart.total_amount;
                            
                            model.carts.Add(cart);
                            await _cartMongodbService.Delete(item.id);
                        }
                    }
                    
                    if(model.delivery_detail.carrier_id>1)
                    {
                        model.delivery_order = new List<OrderDetailMongoDbDelivery>();
                        switch (model.delivery_detail.carrier_id) {
                            case 1:
                                {

                                }break;
                            case 2:
                                {

                                }
                                break;
                            case 3:
                                {
                                    if (model.delivery_detail.shipping_service_code == null || model.delivery_detail.shipping_service_code.Trim() == "")
                                    {
                                        break;
                                    }
                                    var list_supplier = list_cart.Select(x => x.product.supplier_id).Distinct();
                                    foreach (var supplier in list_supplier)
                                    {
                                        var cart_belong_to_supplier = list_cart.Where(x => x.product.supplier_id == supplier);
                                        var detail_supplier = await _supplierESRepository.GetById(supplier);
                                        int package_weight = 0;

                                        double amount = 0;
                                        foreach (var c in cart_belong_to_supplier)
                                        {
                                            var selected = list_cart.First(x => x._id == c._id);
                                            package_weight += Convert.ToInt32(((c.product.weight <= 0 ? 0 : c.product.weight) * selected.quanity));

                                            amount += Convert.ToInt32(((c.product.amount_after_flashsale == null ? c.product.amount : c.product.amount_after_flashsale) * selected.quanity));
                                        }
                                        var response_item = await _viettelPostService.GetShippingMethods(new VTPGetPriceAllRequest()
                                        {
                                            MoneyCollection = 0,
                                            ProductHeight = 0,
                                            ProductLength = 0,
                                            ProductPrice = Convert.ToInt64(amount),
                                            ProductType = "HH",
                                            ProductWeight = package_weight,
                                            ProductWidth = 0,
                                            SenderDistrict = detail_supplier.districtid == null ? 4 : (int)detail_supplier.districtid,
                                            SenderProvince = (int)detail_supplier.provinceid == null ? 1 : (int)detail_supplier.provinceid,
                                            ReceiverDistrict = Convert.ToInt32(request.address.DistrictId),
                                            ReceiverProvince = Convert.ToInt32(request.address.ProvinceId),
                                            Type = 1
                                        });
                                        if (response_item != null && response_item.Count > 0)
                                        {
                                            var selected_delivery = response_item.Where(x => x.MaDvChinh.Trim().ToUpper() == model.delivery_detail.shipping_service_code.Trim().ToUpper());

                                            if (selected_delivery != null && selected_delivery.Count() > 0)
                                            {
                                                LogHelper.InsertLogTelegram("Order selected_delivery: [" + string.Join(",", selected_delivery.Select(x => x.MaDvChinh)) + "]" +
                                                  "[" + string.Join(", ", selected_delivery.Select(x => x.GiaCuoc)) + "]"
                                                  );
                                                model.shipping_fee = selected_delivery.Sum(x => x.GiaCuoc);
                                                model.total_amount += selected_delivery.Sum(x => x.GiaCuoc);
                                            }
                                            model.delivery_order.Add(new OrderDetailMongoDbDelivery()
                                            {
                                                package_weight=package_weight,
                                                shipping_fee= selected_delivery.Sum(x => x.GiaCuoc),
                                                SupplierId=supplier
                                            });
                                        }
                                    }

                                  
                                }
                                break;
                        }
                    }
                    //--apply voucher to 
                    if (voucher_apply != null && voucher_apply.Count > 0)
                    {
                        model.voucher_apply = new List<OrderDetailMongoDbVoucherApply>();
                        foreach (var voucher in voucher_apply)
                        {
                            double total_discount = 0;
                            double percent = Convert.ToDouble(voucher.price_sales);
                            double total_amount_calculate = 0;
                            switch (voucher.rule_type)
                            {
                                case 0: // Giảm giá trên tiền hàng
                                    {
                                        total_amount_calculate = model.total_amount;
                                    }
                                    break;
                                case 1: // Giảm giá trên phí ship
                                    {
                                        total_amount_calculate = model.shipping_fee==null?0:(double)model.shipping_fee;
                                    }
                                    break;
                                case 2: // Giảm giá trên NCC
                                    {
                                        if(voucher.campaign_id!=null && voucher.campaign_id > 0)
                                        {
                                            total_amount_calculate = model.carts.Where(x => x.product.supplier_id == (int)voucher.campaign_id).Sum(x => x.total_amount);
                                        }
                                    }
                                    break;
                            }
                            if (total_amount_calculate > 0)
                            {
                                switch (voucher.unit)
                                {
                                    case "percent":
                                        total_discount += (total_amount_calculate * Convert.ToDouble(percent / 100));
                                        break;
                                    case "vnd":
                                        total_discount += percent;
                                        break;

                                    default: break;
                                }
                            }
                            model.total_discount = total_discount;
                            model.total_amount -= total_discount;
                            //model.total_profit -= total_discount;
                            model.voucher_apply.Add(new OrderDetailMongoDbVoucherApply()
                            {
                                PriceSales=voucher.price_sales,
                                RuleType=voucher.rule_type,
                                SupplierId=voucher.campaign_id,
                                TotalDiscount=total_discount,
                                Unit=voucher.unit,  
                                voucher_code=voucher.code,
                                voucher_id=voucher.Id,
                            });
                        }

                    }
                    //-- Mongodb:
                    LogHelper.InsertLogTelegram("Order orderMongodbService.Insert: [" + model.total_price + "]" +
                                                  "[" + model.total_profit + "]"+
                                                  "[" + model.shipping_fee + "]"+
                                                  "[" + model.total_discount + "]"+
                                                  "[" + model.total_amount + "]"
                                                  );
                    var result = await orderMongodbService.Insert(model);
                   
                    //-- Insert Queue:
                    var queue_model = new CheckoutQueueModel() { event_id = (int)CheckoutEventID.CREATE_ORDER, order_mongo_id = result };


                    var pushed_queue = work_queue.InsertQueueSimpleDurable(JsonConvert.SerializeObject(queue_model), QueueName.QUEUE_CHECKOUT);
                    //LogHelper.InsertLogTelegram(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], "Push Queue: "
                    //   + QueueName.QUEUE_CHECKOUT
                    //   + "[" + JsonConvert.SerializeObject(queue_model) + "] [" + pushed_queue + "]");

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = new OrderConfirmResponseModel { order_no = order_no, id = result, pushed = pushed_queue }
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
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
                        || request.token == null || request.token.Trim() == "")
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
                    var product = await productDetailService.GetByID(request.product_id);
                    if (product != null && product.parent_product_id != null && product.parent_product_id.Trim() != "")
                    {
                        main_product_id = product.parent_product_id;
                    }
                    ProductRaitingPushQueueModel model = new ProductRaitingPushQueueModel()
                    {
                        UserId = (long)account_client.ClientId,
                        Comment = request.comment,
                        CreatedDate = DateTime.UtcNow.ToLocalTime(),
                        ImgLink = request.img_link,
                        OrderId = request.order_id,
                        ProductId = main_product_id,
                        ProductDetailId = request.product_id,
                        VideoLink = request.video_link,
                        Star = request.star,

                    };
                    var queue_model = new
                    {
                        type = QueueType.INSERT_PRODUCT_RATING,
                        data_push = JsonConvert.SerializeObject(model)
                    };
                    var pushed_queue = work_queue.InsertQueueSimple(JsonConvert.SerializeObject(queue_model), QueueName.queue_app_push);

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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
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
        [HttpPost("fe-history-count")]
        public async Task<ActionResult> CountOrderFEHistory([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == ""
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

                    var (all, waiting, delvering, finish, refund, cancel, processing) = orderMergeESService.CountOrdersByStatus((long)account_client.ClientId);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = new
                        {
                            all = all,
                            waiting_payment = waiting,
                            on_delivery = delvering,
                            success = finish,
                            cancel = cancel ,
                            refund= refund,
                            processing= processing
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
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("update-address")]
        public async Task<ActionResult> UpdateAddress([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersUpdateAddressRequestModel>(objParr[0].ToString());
                    if (request == null
                        || request.order_id <= 0
                        || request.province_id == null || request.province_id.Trim() == ""
                        || request.district_id == null || request.district_id.Trim() == ""
                        || request.ward_id == null || request.ward_id.Trim() == ""
                        || request.address == null || request.address.Trim() == ""
                        || request.phone == null || request.phone.Trim() == ""
                        || request.token == null || request.token.Trim() == ""
                        || request.id == null || request.id.Trim() == ""
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
                    Province province = locationESService.GetProvincesByProvinceId(request.province_id);
                    District district = locationESService.GetDistrictByDistrictId(request.district_id);
                    Ward ward = locationESService.GetWardsByWardId(request.ward_id);
                    var result = await orderMongodbService.FindById(request.id);
                    if (result != null && result._id != null)
                    {
                        result.provinceid = request.province_id;
                        result.districtid = request.district_id;
                        result.wardid = request.ward_id;
                        result.phone = request.phone;
                        result.receivername = request.receiver_name;
                        result.address = request.address;
                        result.address_id = request.address_id;
                        await orderMongodbService.UpdateAddress(result);
                    }
                    var account_client = accountClientESService.GetById(account_client_id);
                    var client = clientESService.GetById((long)account_client.ClientId);
                    var model = new
                    {
                        OrderId = request.order_id,
                        ClientId = (long)account_client.ClientId,
                        ProvinceId = province == null ? (int?)null : province.Id,
                        DistrictId = district == null ? (int?)null : district.Id,
                        WardId = ward == null ? (int?)null : ward.Id,
                        Address = request.address,
                        Phone = request.phone
                    };
                    var queue_model = new
                    {
                        type = QueueType.UPDATE_ORDER,
                        data_push = JsonConvert.SerializeObject(model)
                    };
                    var pushed_queue = work_queue.InsertQueueSimple(JsonConvert.SerializeObject(queue_model), QueueName.queue_app_push);

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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
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
        [HttpPost("refund")]
        public async Task<ActionResult> Refund([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //var input_model = new
                //{
                //    id = 10446,
                //    reason = "test",
                //    token = "F08nOlAVBi8vLwxaDGMgagRjYX97aVlkfFt7AmJnTlpFXyNQYmNiUgBpXnt3Q1BJUlZ0WE5BcCxNFysoPCdLQhRzZWoEfmR5Y2tYBHlQcABrbFhOSQVqS2t3ZlFpZRI="
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(input_model), configuration["KEY:private_key"])
                //};
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersRefundRequestModel>(objParr[0].ToString());
                    if (request == null
                        || request.id <= 0
                        || request.reason == null || request.reason.Trim() == ""
                        || request.token == null || request.token.Trim() == ""
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
                    var model = new
                    {
                        OrderId = request.id,
                        ClientId = (long)account_client.ClientId,
                        OrderStatus=(int)OrderStatus.REFUND,
                        RefundStatus = 1,
                        RefundReason = request.reason,
                        RefundDate = DateTime.Now
                    };
                    var queue_model = new
                    {
                        type = QueueType.UPDATE_ORDER,
                        data_push = JsonConvert.SerializeObject(model)
                    };
                    var pushed_queue = work_queue.InsertQueueSimple(JsonConvert.SerializeObject(queue_model), QueueName.queue_app_push);

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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
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
        [HttpPost("cancel")]
        public async Task<ActionResult> CancelOrder([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //var input_model = new
                //{
                //    id = 10624,
                //    token = "F08nOlAVBi8vLwxaDGMgagRjYX97aVlkfFt7AmJnTlpFXyNQYmNiUgBpXnt3Q1BJUlZ0WE5BcCxNFysoPCdLQhRzZWoEfmR5Y2pZBHhfcAFnbFhPSQNlQ21wYFppZRI=",
                //    reason = "Test Cancel Order"
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(input_model), configuration["KEY:private_key"])
                //};
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersCancelRequestModel>(objParr[0].ToString());
                    if (request == null
                        || request.id == null || request.id.Trim() == ""
                        || request.token == null || request.token.Trim() == ""
                        || request.reason == null || request.reason.Trim() == ""
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
                    try
                    {
                        long order_id = Convert.ToInt64(request.id);
                    }
                    catch
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var order = await _orderRepository.GetDetailOrderByOrderId(Convert.ToInt64(request.id));
                    if(order == null || order.OrderId!= Convert.ToInt64(request.id)||order.ClientId!= (long)account_client.ClientId)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    try
                    {
                        
                        var result = await _orderRepository.UpdateOrderStatus(new Models.Models.Order()
                        {
                            OrderId = Convert.ToInt64(request.id),
                            OrderStatus = (int)OrderStatus.CANCEL,
                            UserUpdateId = 1,
                            RefundStatus=1,
                            RefundReason= request.reason
                        });
                        work_queue.SyncES(Convert.ToInt64(request.id), "SP_GetOrder", "hulotoys_sp_getorder", 1);

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                        });
                    }
                    catch
                    {

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
        [HttpPost("received-order")]
        public async Task<ActionResult> ReceivedOrder([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //var input_model = new
                //{
                //    id = 10446,
                //    reason = "test",
                //    token = "F08nOlAVBi8vLwxaDGMgagRjYX97aVlkfFt7AmJnTlpFXyNQYmNiUgBpXnt3Q1BJUlZ0WE5BcCxNFysoPCdLQhRzZWoEfmR5Y2tYBHlQcABrbFhOSQVqS2t3ZlFpZRI="
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(input_model), configuration["KEY:private_key"])
                //};
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersReceivedPackageRequestModel>(objParr[0].ToString());
                    if (request == null
                        || request.id <= 0
                        || request.token == null || request.token.Trim() == ""
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
                    var model = new
                    {
                        OrderId = request.id,
                        ClientId = (long)account_client.ClientId,
                        OrderStatus=(int)OrderStatus.FINISHED_DELIVERY,
                       
                    };
                    var queue_model = new
                    {
                        type = QueueType.UPDATE_ORDER,
                        data_push = JsonConvert.SerializeObject(model)
                    };
                    var pushed_queue = work_queue.InsertQueueSimple(JsonConvert.SerializeObject(queue_model), QueueName.queue_app_push);

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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
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
