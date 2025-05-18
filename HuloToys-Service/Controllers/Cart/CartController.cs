using Models.MongoDb;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Models.Cart;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Controllers.Cart.Business;
using HuloToys_Service.Controllers.Client.Business;

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
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly OrderMongodbService orderMongodbService;
        private readonly ClientServices clientServices;

        public CartController(IConfiguration configuration)
        {
            _configuration  = configuration;
            orderMongodbService = new OrderMongodbService(configuration);
            workQueueClient = new WorkQueueClient(configuration);
            _cartMongodbService = new CartMongodbService(configuration);
            _productDetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _cartService = new CartService(configuration);
            clientServices = new ClientServices(configuration);

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
                    if (data == null || data.product == null)
                    {
                        var product = await _productDetailMongoAccess.GetByID(request.product_id);

                        await _cartMongodbService.Insert(new CartItemMongoDbModel()
                        {
                            account_client_id = account_client_id,
                            product = product,
                            quanity = request.quanity,
                            total_amount=product.amount * request.quanity,
                            created_date=DateTime.Now,
                           
                        });
                        id =1;
                    }
                    else
                    {
                        data.quanity += request.quanity;
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
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
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
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
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
