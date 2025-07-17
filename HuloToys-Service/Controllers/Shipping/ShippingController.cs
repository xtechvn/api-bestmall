using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Shipping.Business;
using HuloToys_Service.Models.Address;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.NinjaVan;
using HuloToys_Service.Models.Shipping.ViettelPost;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.constants.Shipping;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Shipping
{
    [ApiController]
    [Route("api/shipping")]
    public class ShippingController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn _redisService;
        private readonly ShippingBussinessSerice shippingBussinessSerice;
        private readonly ViettelPostService _viettelPostService;
        private readonly SupplierESRepository _supplierESRepository;
        private readonly CartMongodbService _cartMongodbService;

        public ShippingController(IConfiguration _configuration, RedisConn redisService, ViettelPostService viettelPostService, SupplierESRepository supplierESRepository,
            CartMongodbService cartMongodbService)
        {
            configuration = _configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            shippingBussinessSerice=new ShippingBussinessSerice(_configuration);
            _viettelPostService = viettelPostService;
            _supplierESRepository=supplierESRepository;
            _cartMongodbService=cartMongodbService;
        }
        [HttpPost("get-fee")]

        public async Task<IActionResult> GetShippingFee([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ShippingFeeRequestModel>(objParr[0].ToString());
                    if(request.to_province_id<=0|| request.carrier_id<=0|| request.shipping_type<=0|| request.carts==null || request.carts.Count <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }

                    var data = await shippingBussinessSerice.GetShippingFeeResponse(request);
                    if(data!=null && data.from_province_id > 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = ResponseMessages.Success,
                            data=data
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
        [HttpPost("viettelpost/listing")]

        public async Task<IActionResult> GetVTPServiceListing([FromBody] APIRequestGenericModel input)
        {
            //var json_input = new VTPServiceListingRequestModel()
            //{
            //    carts = new List<VTPServiceListingRequestCart>()
            //            {
            //                new VTPServiceListingRequestCart(){quanity=1, _id="6877b3ef81052c9b6afcc58c"},
            //                new VTPServiceListingRequestCart(){quanity=1, _id="6877b3f381052c9b6afcc58e"},
            //                new VTPServiceListingRequestCart(){quanity=1, _id="6877b3f181052c9b6afcc58d"},
            //            },
            //    receiver_district_id = 39,
            //    receiver_provinces_id = 02
            //};
            //input = new APIRequestGenericModel()
            //{
            //    token = CommonHelper.Encode(JsonConvert.SerializeObject(json_input), configuration["KEY:private_key"])
            //};
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<VTPServiceListingRequestModel>(objParr[0].ToString());
                    
                    if (request==null|| request.carts == null || request.carts.Count <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    List<VTPServiceListingResponseModel> response = new List<VTPServiceListingResponseModel>();
                    var carts = await _cartMongodbService.GetByIds(request.carts.Select(x => x._id).ToList());
                    if(carts!=null && carts.Count > 0)
                    {
                        var list_supplier = carts.Select(x => x.product.supplier_id).Distinct();
                        foreach(var supplier in list_supplier)
                        {
                            var detail_supplier = await _supplierESRepository.GetByIdAsync(supplier);
                            int package_weight = 0;
                            int package_width = 0;
                            int package_height = 0;
                            int package_depth = 0;
                            double amount = 0;
                            foreach (var c in carts)
                            {
                                var selected = request.carts.First(x => x._id == c._id);
                                package_weight += Convert.ToInt32(((c.product.weight <= 0 ? 0 : c.product.weight) * selected.quanity));
                                package_width += Convert.ToInt32(((c.product.package_width <= 0 ? 0 : c.product.package_width) * selected.quanity));
                                package_height += Convert.ToInt32(((c.product.package_height <= 0 ? 0 : c.product.package_height) * selected.quanity));
                                package_depth += Convert.ToInt32(((c.product.package_depth <= 0 ? 0 : c.product.package_depth) * selected.quanity));
                                amount += Convert.ToInt32(((c.product.amount_after_flashsale == null ? c.product.amount : c.product.amount_after_flashsale) * selected.quanity));
                            }
                            var response_item = await _viettelPostService.GetShippingMethods(new VTPGetPriceAllRequest()
                            {
                                MoneyCollection = 0,
                                ProductHeight = package_height,
                                ProductLength = package_depth,
                                ProductPrice = Convert.ToInt64(amount),
                                ProductType = "HH",
                                ProductWeight = package_weight,
                                ProductWidth = package_width,
                                SenderDistrict   = detail_supplier.districtid==null? 4: (int)detail_supplier.districtid,
                                SenderProvince = (int)detail_supplier.provinceid == null ? 1: (int)detail_supplier.provinceid,
                                ReceiverDistrict = request.receiver_district_id,
                                ReceiverProvince = request.receiver_provinces_id,
                                 Type=1
                            });
                            if(response_item!=null && response_item.Count > 0)
                            {
                                response.Add(new VTPServiceListingResponseModel()
                                {
                                    supplier_id = supplier,
                                    services=response_item.Select(x=> new VTPServiceListingResponseMethod()
                                    {
                                        name=x.TenDichVu,
                                        service_code=x.MaDvChinh,
                                        total_amount=x.GiaCuoc,
                                        time=x.ThoiGian
                                    }).ToList()
                                });
                            }
                        }
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data= response
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
    }
}
