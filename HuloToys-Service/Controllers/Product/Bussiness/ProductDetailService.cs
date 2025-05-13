using Azure.Core;
using Caching.Elasticsearch;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.MongoDb;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Product.Bussiness
{
    public class ProductDetailService
    {
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly CartMongodbService _cartMongodbService;
        private readonly RaitingESService _raitingESService;
        private readonly ClientESService _clientESService;
        private readonly IConfiguration _configuration;
        private readonly GroupProductESService groupProductESService;
        private readonly OrderDetailESService orderDetailESService;

        public ProductDetailService(IConfiguration configuration)
        {
            _productDetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _cartMongodbService = new CartMongodbService(configuration);
            groupProductESService = new GroupProductESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _clientESService = new ClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _configuration = configuration;
            orderDetailESService = new OrderDetailESService(configuration["DataBaseConfig:Elastic:Host"], configuration);

        }
        public async Task<ProductListFEResponseModel> ProductListing(ProductListRequestModel request)
        {
            ProductListFEResponseModel result = new ProductListFEResponseModel();
            try
            {
                // Chuẩn hóa từ khóa tìm kiếm
                request.keyword = StringHelper.ValidateTextForSearch(request.keyword);


                var data = await _productDetailMongoAccess.ResponseListing(request.keyword, request.group_id,request.page_index,request.page_size , request.price_from,request.price_to, request.rating);
                result = JsonConvert.DeserializeObject<ProductListFEResponseModel>(JsonConvert.SerializeObject(data));
                if(result!=null && result.items!=null && result.items.Count > 0)
                {
                    foreach (var i in result.items)
                    {
                        var raiting = _raitingESService.GetListByFilter(new Models.Raiting.ProductRaitingRequestModel()
                        {
                            id = i._id,
                            has_comment = false,
                            has_media = false,
                            page_index = 1,
                            page_size = 500,
                            stars = 0
                        });
                        if(raiting!=null && raiting.Count > 0)
                        {
                            i.review_count = raiting.Count;
                            i.rating = raiting.Sum(x => x.Star == null ? 0 : (float)x.Star) / (float)raiting.Count;
                            i.total_sold = orderDetailESService.CountByProductId(new List<string>() { i._id });

                        }

                    }
                }
            }
            catch(Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return result;
        }
    }
}
