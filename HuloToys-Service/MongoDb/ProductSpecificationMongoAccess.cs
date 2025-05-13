using Azure.Core;
using Entities.ViewModels.Products;
using HuloToys_Service.Utilities.Lib;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HuloToys_Service.MongoDb
{
    public class ProductSpecificationMongoAccess
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<ProductSpecificationMongoDbModel> _product_specification_collection;

        public ProductSpecificationMongoAccess(IConfiguration configuration)
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
            _product_specification_collection = db.GetCollection<ProductSpecificationMongoDbModel>("ProductSpecification");
        }
        public async Task<string> AddNewAsync(ProductSpecificationMongoDbModel model)
        {
            try
            {
                model.GenID();
                await _product_specification_collection.InsertOneAsync(model);
                return model._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                return null;
            }
        }
        public async Task<string> UpdateAsync(ProductSpecificationMongoDbModel model)
        {
            try
            {
                var filter = Builders<ProductSpecificationMongoDbModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id));
                await _product_specification_collection.FindOneAndReplaceAsync(filterDefinition, model);
                return model._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                return null;
            }
        }


        public async Task<ProductSpecificationMongoDbModel> GetByID(string id)
        {
            try
            {
                var filter = Builders<ProductSpecificationMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductSpecificationMongoDbModel>.Filter.Eq(x => x._id, id); ;
                var model = await _product_specification_collection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                return null;
            }
        }
        public async Task<ProductSpecificationMongoDbModel> GetByNameAndType(int type, string name)
        {
            try
            {
                var filter = Builders<ProductSpecificationMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductSpecificationMongoDbModel>.Filter.Eq(x => x.attribute_name, name); ;
                filterDefinition &= Builders<ProductSpecificationMongoDbModel>.Filter.Eq(x => x.attribute_type, type); ;
                var model = await _product_specification_collection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                return null;
            }
        }

        public async Task<List<ProductSpecificationMongoDbModel>> Listing(int type, string name)
        {
            try
            {
                var filter = Builders<ProductSpecificationMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductSpecificationMongoDbModel>.Filter.Eq(x => x.attribute_type, type);
                if (name != null && name.Trim() != "")
                {
                    filterDefinition &= Builders<ProductSpecificationMongoDbModel>.Filter.Regex(x => x.attribute_name, new Regex(name, RegexOptions.IgnoreCase));

                }
                var model = _product_specification_collection.Find(filterDefinition);
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                return null;
            }
        }



    }
}
