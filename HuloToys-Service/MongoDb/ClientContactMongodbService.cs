using Models.MongoDb;
using HuloToys_Service.Utilities.Lib;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Reflection;
using Nest;
using MongoDB.Bson;
using HuloToys_Service.Utilities.lib;

namespace HuloToys_Service.MongoDb
{
    public class ClientContactMongodbService
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<ClientContactMongoDbModel> bookingCollection;

        public ClientContactMongodbService(IConfiguration configuration) {

            _configuration= configuration;
            //mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=HoanBds
            string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] +
                ":" + configuration["DataBaseConfig:MongoServer:pwd"] +
                "@" + configuration["DataBaseConfig:MongoServer:Host"] +
                ":" + configuration["DataBaseConfig:MongoServer:Port"] +
                "/?authSource=" + configuration["DataBaseConfig:MongoServer:catalog_core"] + "";

            var client = new MongoClient(url);
            IMongoDatabase db = client.GetDatabase(_configuration["DataBaseConfig:MongoServer:catalog_core"]);
            bookingCollection = db.GetCollection<ClientContactMongoDbModel>("ClientContact");
        }
        public async Task<string> Insert(ClientContactMongoDbModel item)
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
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;

        }
        public async Task<List<ClientContactMongoDbModel>> GetList(string keyword)
        {
            try
            {
                var keyword_nonunicode = StringHelper.RemoveUnicode(keyword);
                var filter = Builders<ClientContactMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ClientContactMongoDbModel>.Filter.Or(
                  //Unicode
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.email, new BsonRegularExpression($"{keyword}", "i")),
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.phone, new BsonRegularExpression($"{keyword}", "i")),
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.message, new BsonRegularExpression($"{keyword}", "i")),
                  //Non-unicode
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.email, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                   Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.phone, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.message, new BsonRegularExpression($"{keyword_nonunicode}", "i"))
                );


                var model = await bookingCollection.Find(filterDefinition)
                    .Sort(Builders<ClientContactMongoDbModel>.Sort.Descending(c => c.created_date))
                    .ToListAsync();
                if (model != null)
                {
                    return model;
                }

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
