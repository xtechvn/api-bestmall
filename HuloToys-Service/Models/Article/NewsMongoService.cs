using Entities.ViewModels.Products;
using ENTITIES.ViewModels.ArticleViewModels;
using HuloToys_Service.Utilities.Lib;
using MongoDB.Driver;
using System.Drawing.Printing;

namespace HuloToys_Service.Models.Article
{
    public class NewsMongoService
    {

        private readonly IConfiguration _configuration;
        private IMongoCollection<NewsViewCount> _news_collection;

        public NewsMongoService(IConfiguration _configuration)
        {
            _configuration = _configuration;
            //mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=HoanBds
            string url = "mongodb://" + _configuration["DataBaseConfig:MongoServer:user"] +
                ":" + _configuration["DataBaseConfig:MongoServer:pwd"] +
                "@" + _configuration["DataBaseConfig:MongoServer:Host"] +
                ":" + _configuration["DataBaseConfig:MongoServer:Port"] +
                "/?authSource=" + _configuration["DataBaseConfig:MongoServer:catalog_core"] + "";

            var client = new MongoClient(url);

            IMongoDatabase db = client.GetDatabase(_configuration["DataBaseConfig:MongoServer:catalog_core"]);
            _news_collection = db.GetCollection<NewsViewCount>("ArticlePageView");
        }
        public async Task<string> AddNewOrReplace(NewsViewCount model)
        {
            try
            {
                var filter = Builders<NewsViewCount>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<NewsViewCount>.Filter.Eq(x => x.articleID, model.articleID);
                var exists_model = await _news_collection.Find(filterDefinition).FirstOrDefaultAsync();
                if (exists_model != null && exists_model.articleID == model.articleID)
                {
                    exists_model.pageview = exists_model.pageview + model.pageview;
                    await _news_collection.FindOneAndReplaceAsync(filterDefinition, exists_model);
                    return exists_model._id;
                }
                else
                {
                    model.GenID();
                    await _news_collection.InsertOneAsync(model);
                    return model._id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], "AddNewOrReplace - NewsMongoService: " + ex);
                return null;
            }
        }
        public async Task<List<NewsViewCount>> GetMostViewedArticle()
        {
            try
            {
                var filter = Builders<NewsViewCount>.Filter;
                var filterDefinition = filter.Empty;
                var model = _news_collection.Find(filterDefinition);
                model.Options.Skip =0;
                model.Options.Limit = 5;
                return await model.ToListAsync();

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], "GetMostViewedArticle - NewsMongoService: " + ex);
            }
            return null;
        }

    }
}
