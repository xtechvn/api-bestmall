using Azure.Core;
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Flashsale.Bussiness;
using HuloToys_Service.Controllers.News.Business;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.Models.Flashsale;
using HuloToys_Service.Models.Models;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using static Nest.JoinField;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class FlashsaleController : ControllerBase
    {
        //private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly ProductSpecificationMongoAccess _productSpecificationMongoAccess;
        private readonly CartMongodbService _cartMongodbService;
        private readonly RaitingESService _raitingESService;
        private readonly OrderDetailESService orderDetailESService;
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisService;
        private readonly GroupProductESService groupProductESService;
        private readonly ProductRaitingService productRaitingService;
        private readonly ProductDetailService productDetailService;
        private readonly ProductESRepository _productESRepository;
        private readonly AttachFileESModelESRepository attachFileESModelESRepository;
        private readonly ProductFavouritesMongoAccess _productFavouritesMongoAccess;
        private readonly ClientServices clientServices;
        private readonly FlashSaleESRepository flashSaleESRepository;
        private readonly FlashSaleProductESRepository flashSaleProductESRepository;
        private readonly FlashsaleService flashsaleService;

        public FlashsaleController(IConfiguration configuration, RedisConn redisService, ProductDetailService _productDetailService, CartMongodbService cartMongodbService
            , ProductFavouritesMongoAccess productFavouritesMongoAccess, ProductSpecificationMongoAccess productSpecificationMongoAccess/*,ProductDetailMongoAccess productDetailMongoAccess*/)
        {
           // _productDetailMongoAccess = productDetailMongoAccess;
            _productSpecificationMongoAccess = productSpecificationMongoAccess;
            _productFavouritesMongoAccess = productFavouritesMongoAccess;
            _cartMongodbService = cartMongodbService;
            productRaitingService = new ProductRaitingService(configuration);
            productDetailService = _productDetailService;
            orderDetailESService = new OrderDetailESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            groupProductESService = new GroupProductESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _productESRepository = new ProductESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            flashSaleESRepository = new FlashSaleESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            flashSaleProductESRepository = new FlashSaleProductESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            attachFileESModelESRepository = new AttachFileESModelESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            clientServices = new ClientServices(configuration);
            _configuration = configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            flashsaleService = new FlashsaleService(configuration);
        }
        [HttpPost("get-by-id")]

        public async Task<IActionResult> ListingByFlashSaleId([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //var model_input = new
                //{
                //    id = 2,
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(model_input), _configuration["KEY:private_key"])
                //};

                JArray objParr = null;

                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<FlashsaleListingRequestModel>(objParr[0].ToString());
                    if (request == null || request.id<=0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var item = await flashSaleESRepository.GetByIdAsync(request.id);
                    if(item==null || item.id<=0|| item.status!=1)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var list = await flashSaleProductESRepository.GetByFlashsaleId(request.id);
                    if(list == null|| list.Count <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "No Items"
                        });
                    }
                    list = list.OrderBy(x => x.position).ToList();
                    var list_products = await productDetailService.GetFlashSaleProductByProductIds(list);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = list_products
                    });

                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.DataInvalid
                });
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("get-list")]
        public async Task<IActionResult> Listing([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //input.token = "F081O1oSKR4nJktCB3d5ekEyMysRMQY0LBBoCGN6TgYGUTYtKygpBxF9Xn85";

                JArray objParr = null;

                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var list = await flashSaleESRepository.SearchActiveFlashSales();                   
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = new
                        {
                            items = list,
                            count = list.Count
                        }
                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.DataInvalid
                });
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("supersale")]
        public async Task<IActionResult> ListingSuperSale([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //var model_input = new 
                //{
                //   page_index=1,
                //   page_size=10
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(model_input), _configuration["KEY:private_key"])
                //};

                JArray objParr = null;

                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductFavouritesListRequestModel>(objParr[0].ToString());
                    if (request == null || request.page_index <=0 || request.page_size <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var list_fl = await flashSaleESRepository.SearchActiveFlashSales();
                    List<int> list_id = new List<int>();
                    if(list_fl!=null && list_fl.Count > 0)
                    {
                        list_id = list_fl.Select(x => x.flashsale_id).ToList();
                    }
                    var list = await flashSaleProductESRepository.GetListSuperSale(list_id, request.page_index, request.page_size);
                    if (list == null || list.Count <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "No Items"
                        });
                    }
                    var list_products = await productDetailService.GetFlashSaleProductByProductIds(list);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = list_products,
                        count = await flashSaleProductESRepository.CountListSuperSale(list_id)

                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.DataInvalid
                });
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("get-by-type")]
        public async Task<IActionResult> ListingByType([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //var model_input = new
                //{
                //    type = 3,
                //    page_index = 1,
                //    page_size = 10
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(model_input), _configuration["KEY:private_key"])
                //};

                JArray objParr = null;

                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductFlashSaleByTypeRequestModel>(objParr[0].ToString());
                    if (request == null || request.page_index <= 0 || request.page_size <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var list_fl = await flashSaleESRepository.SearchActiveFlashSales();
                    List<int> list_id = new List<int>();
                    if (list_fl != null && list_fl.Count > 0)
                    {
                        list_id = list_fl.Select(x => x.flashsale_id).ToList();
                    }
                    var list = await flashSaleProductESRepository.GetListFlashSaleProductByType(list_id,request.type, request.page_index, request.page_size, request.type);
                    if (list == null || list.Count <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "No Items"
                        });
                    }
                    var list_products = await productDetailService.GetFlashSaleProductByProductIds(list);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = list_products,
                        count = await flashSaleProductESRepository.CountListFlashSaleProductByType(list_id, request.type)

                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.DataInvalid
                });
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }

        [HttpPost("group-product")]
        public async Task<IActionResult> GroupProduct([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductFlashSaleByTypeRequestModel>(objParr[0].ToString());
                    if (request == null || request.group_id ==null || request.group_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    string cache_name = "GROUP_PRODUCT_FLASHSALE_"+request.group_id;
                    string j_data = null;
                    List<GroupProductESModel> data = null;

                    try
                    {
                        j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"],
                            "GroupProductController - Redis GET failed: " + ex);
                    }
                    if (j_data != null)
                    {
                        data = JsonConvert.DeserializeObject<List<GroupProductESModel>>(j_data);
                    }
                    else
                    {
                        data =  groupProductESService.GetFlashSaleGroupProduct((int)request.group_id);
                        if (data != null && data.Count > 0)
                        {
                            try
                            {
                                _redisService.Set(cache_name, JsonConvert.SerializeObject(data), Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                            }
                            catch (Exception ex)
                            {
                                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"],
                                    "GroupProductController - Redis SET failed: " + ex);
                            }
                        }
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = data
                    });
                }


            }
            catch
            {

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
    }
}
