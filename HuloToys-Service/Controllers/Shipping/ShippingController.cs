using Caching.Elasticsearch;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Models.Address;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities.Contants;
using Utilities;
using Newtonsoft.Json;
using HuloToys_Service.Utilities.Lib;
using HuloToys_Service.Models.NinjaVan;
using HuloToys_Service.Utilities.constants.Shipping;
using HuloToys_Service.Controllers.Shipping.Business;

namespace HuloToys_Service.Controllers.Shipping
{
    [ApiController]
    [Route("api/shipping")]
    public class ShippingController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn _redisService;
        private readonly ShippingBussinessSerice shippingBussinessSerice;

        public ShippingController(IConfiguration _configuration, RedisConn redisService)
        {
            configuration = _configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            shippingBussinessSerice=new ShippingBussinessSerice(_configuration);
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
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });
        }
    }
}
