using Caching.Elasticsearch.FlashSale;
using Entities.ViewModels.ElasticSearch;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Cart.Business;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Cart;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.MongoDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient workQueueClient;
        private readonly CartMongodbService _cartMongodbService;
        private readonly CartService _cartService;
       // private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly OrderMongodbService orderMongodbService;
        private readonly ClientServices clientServices;
        private readonly ProductDetailService productDetailService;
        private readonly SupplierESRepository _supplierESRepository;

        public CartController(IConfiguration configuration,/* ProductDetailMongoAccess productDetailMongoAccess,*/ ProductDetailService _productDetailService
            , CartMongodbService cartMongodbService, OrderMongodbService _orderMongodbService, SupplierESRepository supplierESRepository)
        {
            _configuration  = configuration;
            orderMongodbService = _orderMongodbService;
            workQueueClient = new WorkQueueClient(configuration);
            _cartMongodbService = cartMongodbService;
           // _productDetailMongoAccess = productDetailMongoAccess;
            clientServices = new ClientServices(configuration);
            productDetailService = _productDetailService;
            _supplierESRepository=supplierESRepository;
            _cartService = new CartService(configuration, _productDetailService, cartMongodbService, _supplierESRepository);

        }
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductAddToCartRequestModel>(objParr[0].ToString());
                    if (request == null || request.product_id == null)
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
                    var data = await _cartMongodbService.FindByProductId(request.product_id, account_client_id);
                    int id = 0;
                    var product = await productDetailService.GetByID(request.product_id);
                    if (product ==null || product._id==null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (data == null || data.product == null)
                    {
                        data = new CartItemMongoDbModel()
                        {
                            account_client_id = account_client_id,
                            product = product,
                            quanity = request.quanity,
                            total_amount = product.amount * request.quanity,
                            created_date = DateTime.Now,

                        };
                        double amount_product = data.product.amount_after_flashsale == null ? 0 : (double)data.product.amount_after_flashsale;
                        if (data.product.amount_after_flashsale == null || data.product.amount_after_flashsale <= 0)
                        {
                            amount_product = data.product.amount;

                        }
                        data.total_amount = amount_product * data.quanity;
                        data.total_price = data.product.price * data.quanity;
                        data.total_profit = data.product.profit * data.quanity;
                        await _cartMongodbService.Insert(data);
                        id =1;
                    }
                    else
                    {
                        data.product = product;
                        data.quanity += request.quanity;
                        double amount_product = data.product.amount_after_flashsale == null ? 0 : (double)data.product.amount_after_flashsale;
                        if (data.product.amount_after_flashsale == null || data.product.amount_after_flashsale <= 0)
                        {
                            amount_product = data.product.amount;

                        }
                        data.total_amount = amount_product * data.quanity;
                        data.total_price = data.product.price * data.quanity;
                        data.total_profit = data.product.profit * data.quanity;
                        data.created_date = DateTime.Now;
                         await _cartMongodbService.UpdateCartQuanity(data);
                        id = 2;
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = id
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
        [HttpPost("count")]
        public async Task<IActionResult> CountCart([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductCartCountRequestModel>(objParr[0].ToString());
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
                    var data = await _cartMongodbService.Count(account_client_id);

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
        [HttpPost("delete")]
        public async Task<ActionResult> Delete([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<CartDeleteRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var result = await _cartMongodbService.Delete(request.id);
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
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
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
        [HttpPost("list")]
        public async Task<IActionResult> Listing([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductCartCountRequestModel>(objParr[0].ToString());
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (request == null || account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    //var data = await _cartMongodbService.GetList(request.account_client_id);
                    var data = await _cartService.GetList(account_client_id);
                    List<SupplierESModel> list = new List<SupplierESModel>();
                    if(data != null && data.Count > 0)
                    {
                        var list_supplier = _supplierESRepository.GetByIds(data.Select(x => x.product.supplier_id).ToList());
                        if (list_supplier != null && list_supplier.Count > 0) list = list_supplier;
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = data,
                        supplier=list.Select(x=>new
                        {
                            x.supplierid,
                            x.suppliercode,
                            x.fullname
                        })
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
        [HttpPost("quanity-change")]
        public async Task<IActionResult> ChangeQuanity([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductAddToCartRequestModel>(objParr[0].ToString());
                    if (request == null || request.product_id == null)
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
                    var data = await _cartMongodbService.FindByProductId(request.product_id, account_client_id);
                    if (data != null && data.product != null)
                    {
                        data.quanity = request.quanity;
                        data.created_date = DateTime.Now;
                        await _cartMongodbService.UpdateCartQuanity(data);
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = data!=null && data._id!=null? data._id:""
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
        [HttpPost("delete-by-order")]
        public async Task<ActionResult> DeleteByOrder([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<CartDeleteRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var order = await orderMongodbService.FindById(request.id);
                    if(order!=null && order.carts != null)
                    {
                        foreach(var item in order.carts)
                        {
                            await _cartMongodbService.Delete(item._id);
                        }
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = request.id
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
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
