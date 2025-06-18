using Caching.Elasticsearch;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Flashsale.Bussiness;
using HuloToys_Service.Controllers.News.Business;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.MongoDb;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities.Contants;
using Utilities;
using Models.MongoDb;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ClientContactController : ControllerBase
    {
        private readonly ClientContactMongodbService clientContactMongodbService;
        private readonly IConfiguration _configuration;

        public ClientContactController(IConfiguration configuration, ClientContactMongodbService _clientContactMongodbService)
        {
            clientContactMongodbService = _clientContactMongodbService;
            _configuration = configuration;
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddClientContact([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ClientContactMongoDbModel>(objParr[0].ToString());
                    if (request == null || request.phone == null|| request.phone.Trim() == ""
                        || request.email == null || request.email.Trim() == ""
                        || request.message == null || request.message.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    request.created_date = DateTime.Now;
                    await clientContactMongodbService.Insert(request);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = request._id ??""
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
