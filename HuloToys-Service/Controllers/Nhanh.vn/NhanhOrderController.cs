using Caching.Elasticsearch;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Utilities.Contants;
using Utilities;
using Microsoft.AspNetCore.Authorization;

namespace HuloToys_Service.Controllers.Nhanh.vn
{
    [ApiController]
    [Route("api/nhanhvn")]
    public class NhanhOrderController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn _redisService;

        public NhanhOrderController(IConfiguration _configuration, RedisConn redisService)
        {
            configuration = _configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
        }
        [HttpPost("receiver")]
        public async Task<IActionResult> ReceiveData([FromBody] APIRequestGenericModel input)
        {
            try
            {
               
                
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
