using Caching.Elasticsearch.FlashSale;
using Entities.ViewModels.Products;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Models.MongoDb;
using MongoDB.Driver;
using Newtonsoft.Json;
using Pipelines.Sockets.Unofficial.Buffers;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HuloToys_Service.Controllers.Cart.Business
{
    public class CartService
    {
        private readonly IConfiguration _configuration;
        private readonly CartMongodbService _cartMongodbService;
        private readonly ProductDetailService productDetailService;
        private readonly SupplierESRepository _supplierESRepository;
        public CartService(IConfiguration configuration,ProductDetailService _productDetailService, CartMongodbService cartMongodbService, SupplierESRepository supplierESRepository)
        {
            _configuration = configuration;

            _cartMongodbService = cartMongodbService;
            productDetailService = _productDetailService;
            _supplierESRepository = supplierESRepository;
        }
        public async Task<List<CartItemMongoDbViewModel>> GetList(long account_client_id)
        {
            List<CartItemMongoDbViewModel> model = new List<CartItemMongoDbViewModel>();
            try
            {
               var model_core = await _cartMongodbService.GetList(account_client_id);
                if (model != null && model.Count > 0)
                {
                    foreach (var item in model_core)
                    {
                        try
                        {
                            var expand_model = JsonConvert.DeserializeObject<CartItemMongoDbViewModel>(JsonConvert.SerializeObject(item));
                            item.product = await productDetailService.GetByID(item.product._id);
                            var sup = await _supplierESRepository.GetById(item.product.supplier_id);
                            expand_model.supplier_name = sup.fullname;
                            model.Add(expand_model);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return model;
        }
    }
}
