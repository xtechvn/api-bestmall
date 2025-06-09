using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Flashsale.Bussiness;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Flashsale;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Flashsale
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
                    var product_mongo = await productDetailService.ListByProducts(list.Select(x => x.productid).ToList());
                    //List<FlashSaleProductResposeModel> combinedList = [.. list
                    //    .Join(product_mongo, // List thứ hai để join
                    //          fsp => fsp.productid, // Khóa từ list đầu tiên
                    //          p => p._id,     // Khóa từ list thứ hai
                    //          (fsp, p) => flashsaleService.CombineModel(fsp,p)) // Tạo đối tượng mới
                    //    .OrderBy(fspc => fspc.position)];
                    
                    var list_output = new List<FlashSaleProductResposeModel>();
                    if(product_mongo != null && product_mongo.Count>0)
                    {
                        foreach (var order in list)
                        {
                            var selected = product_mongo.FirstOrDefault(x => x._id == order.productid);
                            if (selected == null) continue;
                            list_output.Add(new FlashSaleProductResposeModel()
                            {
                                amount = ((selected.old_price != null && selected.old_price > 0) ? (double)selected.old_price : (selected.amount_min!=null && selected.amount_min>0? (double)selected.amount_min:selected.amount )),
                                amount_after_flashsale = selected.amount_after_flashsale,
                                discountvalue = order.discountvalue,
                                position = order.position,
                                total_discount = selected.discount,
                                valuetype = order.valuetype,
                                _id = selected._id,
                                avatar = selected.avatar,
                                name = selected.name,
                                code = selected.code,
                                rating=selected.rating,
                                review_count=selected.review_count,
                                total_sold=selected.total_sold
                            });
                        }
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = list_output
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
    }
}
