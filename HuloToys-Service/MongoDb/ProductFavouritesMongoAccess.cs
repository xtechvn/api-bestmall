using Entities.ViewModels.Products;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Models.ProductsFavourites;
using HuloToys_Service.Utilities.constants.Product;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace HuloToys_Service.MongoDb
{
    public class ProductFavouritesMongoAccess
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<ProductsFavouritesMongoDbModel> _productDetailCollection;

        public ProductFavouritesMongoAccess(IConfiguration configuration)
        {
            _configuration = configuration;
            //mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=HoanBds
            string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] +
                ":" + configuration["DataBaseConfig:MongoServer:pwd"] +
                "@" + configuration["DataBaseConfig:MongoServer:Host"] +
                ":" + configuration["DataBaseConfig:MongoServer:Port"] +
                "/?authSource=" + configuration["DataBaseConfig:MongoServer:catalog_core"] + "";

            var client = new MongoClient(url);

            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog_core"]);
            _productDetailCollection = db.GetCollection<ProductsFavouritesMongoDbModel>("ProductFavourites");
        }
        public async Task<string> AddNewAsync(ProductsFavouritesMongoDbModel model)
        {
            try
            {
                model.GenID();
                await _productDetailCollection.InsertOneAsync(model);
                return model._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }
        public async Task<string> UpdateAsync(ProductsFavouritesMongoDbModel model)
        {
            try
            {
                var filter = Builders<ProductsFavouritesMongoDbModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id));
                await _productDetailCollection.FindOneAndReplaceAsync(filterDefinition, model);
                return model._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }
        public async Task<string> DeleteAsync(long account_client_id, string product_id)
        {
            try
            {
                var filter = Builders<ProductsFavouritesMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductsFavouritesMongoDbModel>.Filter.Eq(x => x.account_client_id, account_client_id);
                filterDefinition &= Builders<ProductsFavouritesMongoDbModel>.Filter.Eq(x => x.product_id, product_id);
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                if(model!=null && model._id != null)
                {
                    await _productDetailCollection.DeleteOneAsync(filterDefinition);
                    return model._id;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;

        }


        public async Task<ProductsFavouritesMongoDbModel> GetByID(string id)
        {
            try
            {
                var filter = Builders<ProductsFavouritesMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductsFavouritesMongoDbModel>.Filter.Eq(x => x._id, id);
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }

        public async Task<ProductsFavouritesMongoDbModel> GetByAccountAndProduct(string product_id, long account_client_id)
        {
            try
            {
                var filter = Builders<ProductsFavouritesMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductsFavouritesMongoDbModel>.Filter.Eq(x => x.product_id, product_id);
                filterDefinition &= Builders<ProductsFavouritesMongoDbModel>.Filter.Eq(x => x.account_client_id, account_client_id);
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }
        public async Task<ProductsFavouritesListingResponseModel> Listing(long account_client_id)
        {
            try
            {
                var filter = Builders<ProductsFavouritesMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductsFavouritesMongoDbModel>.Filter.Eq(x => x.account_client_id, account_client_id);

                var sort_filter = Builders<ProductsFavouritesMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filterDefinition).Sort(sort_filter_definition);
                long count = await model.CountDocumentsAsync();
                var items = await model.ToListAsync();
                return new ProductsFavouritesListingResponseModel()
                {
                    items = items,
                    count = count
                };
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return null;
            }
        }
     
    }
}
