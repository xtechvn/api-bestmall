using Entities.ViewModels.Products;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Models.MongoDb;
using MongoDB.Driver;
using Pipelines.Sockets.Unofficial.Buffers;
using System.Reflection;

namespace HuloToys_Service.Controllers.Cart.Business
{
    public class CartService
    {
        private readonly IConfiguration _configuration;
        private readonly CartMongodbService _cartMongodbService;
        private readonly ProductDetailService productDetailService;
        public CartService(IConfiguration configuration,ProductDetailService _productDetailService)
        {
            _configuration = configuration;

            _cartMongodbService = new CartMongodbService(configuration);
            productDetailService = _productDetailService;
        }
        public async Task<List<CartItemMongoDbModel>> GetList(long account_client_id)
        {
            try
            {
                var model = await _cartMongodbService.GetList(account_client_id);
                if (model != null)
                {
                    if (model.Count > 0)
                    {
                        foreach(var item in model)
                        {
                            item.product= await productDetailService.GetByID(item.product._id);
                        }
                    }
                    return model;
                }


                //var carts= cartCollection.Aggregate()
                //                .Match(filterDefinition)
                //                .Lookup<CartItemMongoDbModel, ProductMongoDbModel, CartItemMongoDbModel>(
                //                    _productDetailCollection,
                //                    localField => localField.product._id,
                //                    foreignField => foreignField._id,
                //                    output => output.products)
                //                .ToList();
                //if (carts != null)
                //{
                //    return carts;
                //}
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
    }
}
