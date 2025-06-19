using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Utilities.Lib;
using Nest;
using System.Reflection;

namespace HuloToys_Service.ElasticSearch
{
    public class ArticleCategoryESService : ESRepository<ArticleCategoryViewModel>
    {
        public string index = "article_category_hulotoys_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;
        private static ElasticClient elasticClient;

        public ArticleCategoryESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:ArticleCategory"];
            var nodes = new Uri[] { new Uri(_ElasticHost) };
            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index);
            elasticClient = new ElasticClient(connectionSettings);
        }
        public List<ArticleCategoryESModel> GetByArticleId(long id)
        {
            try
            {
               
                var query = elasticClient.Search<ArticleCategoryESModel>(sd => sd
                               .Size(4000)
                               .Query(q => q
                                   .Match(m => m.Field("ArticleId").Query(id.ToString())
                               )));

                if (query.IsValid)
                {
                    var data = query.Documents as List<ArticleCategoryESModel>;

                    return data;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<ArticleCategoryESModel> GetListArticleCategory()
        {
            try
            {
              
                var query = elasticClient.Search<ArticleCategoryESModel>(sd => sd
                               .Size(4000)
                               .Query(q => q.MatchAll()
                               ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<ArticleCategoryESModel>;
           
                    return data;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<ArticleCategoryESModel> GetByCategoryId(long CategoryId)
        {
            try
            {
              

                var query = elasticClient.Search<ArticleCategoryESModel>(sd => sd
                               .Query(q => q
                                   .Term(m => m.Field("CategoryId").Value(CategoryId)
                               )));

                if (query.IsValid)
                {

                    var data = query.Documents as List<ArticleCategoryESModel>;
                    //var result = data.Select(a => new ArticleCategoryViewModel
                    //{

                    //    Id = a.id,
                    //    ArticleId = a.articleid,
                    //    CategoryId = a.categoryid,


                    //}).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
    }
}
