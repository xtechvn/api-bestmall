using Models.MongoDb;
using HuloToys_Service.Utilities.Lib;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Reflection;
using Nest;

namespace HuloToys_Service.MongoDb
{
    public class CartMongodbService
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<CartItemMongoDbModel> bookingCollection;

        public CartMongodbService(IConfiguration configuration) {

            _configuration= configuration;
            //mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=HoanBds
            string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] +
                ":" + configuration["DataBaseConfig:MongoServer:pwd"] +
                "@" + configuration["DataBaseConfig:MongoServer:Host"] +
                ":" + configuration["DataBaseConfig:MongoServer:Port"] +
                "/?authSource=" + configuration["DataBaseConfig:MongoServer:catalog_core"] + "";

            var client = new MongoClient(url);
            IMongoDatabase db = client.GetDatabase(_configuration["DataBaseConfig:MongoServer:catalog_core"]);
            bookingCollection = db.GetCollection<CartItemMongoDbModel>("Cart");
        }
        public async Task<string> Insert(CartItemMongoDbModel item)
        {
            try
            {
                item.GenID();
                await bookingCollection.InsertOneAsync(item);
                return item._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
        public async Task<string> UpdateCartQuanity(CartItemMongoDbModel data)
        {
            try
            {
                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x._id, data._id);
                var update  = Builders<CartItemMongoDbModel>.Update
                    .Set(x => x.created_date, data.created_date)
                    .Set(x => x.quanity, data.quanity);
                var updated_item =await bookingCollection.UpdateOneAsync(filterDefinition, update);

                return data._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
        public async Task<CartItemMongoDbModel> FindById(string id)
        {
            try
            {
                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x._id, id);

                var model = await bookingCollection.Find(filterDefinition).ToListAsync();
                if (model != null)
                {
                    return model.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
        public async Task<CartItemMongoDbModel> FindByProductId(string product_id,long account_client_id)
        {
            try
            {
                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x.account_client_id, account_client_id);
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x.product._id, product_id);

                var model = await bookingCollection.Find(filterDefinition).ToListAsync();
                if (model != null)
                {
                    return model.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;

        }
        public async Task<int> Count(long client_id)
        {
            try
            {

                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x.account_client_id, client_id);

                var model = await bookingCollection.Find(filterDefinition).ToListAsync();
                if (model != null)
                {
                    return model.Count;
                }
                   
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return 0;
        }
        public async Task<List<CartItemMongoDbModel>> GetList(long client_id)
        {
            try
            {

                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x.account_client_id, client_id);


                var model = await bookingCollection.Find(filterDefinition)
                    .Sort(Builders<CartItemMongoDbModel>.Sort.Descending(c => c.created_date))
                    .ToListAsync();
                if (model != null)
                {
                    return model;
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public async Task<bool> Delete(string id)
        {
            try
            {

                var filter = Builders<CartItemMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<CartItemMongoDbModel>.Filter.Eq(x => x._id, id);

                var model = await bookingCollection.DeleteOneAsync(filterDefinition);
                return model.DeletedCount > 0;

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return false;
        }
    }
}
