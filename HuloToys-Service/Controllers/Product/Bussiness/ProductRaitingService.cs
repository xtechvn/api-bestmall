using Azure.Core;
using Caching.Elasticsearch;
using API_CORE.Controllers.ElasticSearch;
using API_CORE.Controllers.Models;
using API_CORE.Controllers.Models.Raiting;
using API_CORE.Controllers.MongoDb;
using API_CORE.Controllers.RedisWorker;
using API_CORE.Controllers.Utilities.Lib;
using Newtonsoft.Json;
using System.Reflection;

namespace API_CORE.Controllers.Controllers.Product.Bussiness
{
    public class ProductRaitingService
    {
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly CartMongodbService _cartMongodbService;
        private readonly RaitingESService _raitingESService;
        private readonly ClientESService _clientESService;
        private readonly IConfiguration _configuration;
        private readonly GroupProductESService groupProductESService;

        public ProductRaitingService(IConfiguration configuration)
        {
            _productDetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _cartMongodbService = new CartMongodbService(configuration);
            groupProductESService = new GroupProductESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _clientESService = new ClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _configuration = configuration;
        }
       
        public async Task<List<RatingESResponseModel>> GetListByFilter(ProductRaitingRequestModel request)
        {
            try
            {
                var data = _raitingESService.GetListByFilter(request);
                if (data != null && data.Count > 0)
                {
                    var result = JsonConvert.DeserializeObject<List<RatingESResponseModel>>(JsonConvert.SerializeObject(data));
                    foreach (var r in result)
                    {
                        try
                        {
                            var client = _clientESService.GetById((int)r.UserId);
                            var product = await _productDetailMongoAccess.GetByID(r.ProductDetailId);
                            r.client_avatar = client.Avartar;
                            r.client_name = client.ClientName;
                            r.variation_detail = "";
                            if (product.variation_detail != null && product.variation_detail.Count > 0)
                            {
                                foreach (var variation in product.variation_detail)
                                {
                                    var type = product.attributes.FirstOrDefault(x => x._id == variation.id);
                                    if (type != null && type._id != null)
                                    {
                                        if (r.variation_detail.Trim() != "") r.variation_detail += ", ";
                                        r.variation_detail += type.name + ": " + variation.name;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
    }
}
