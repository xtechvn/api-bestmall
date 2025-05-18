using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.Entities;
using HuloToys_Service.Utilities.Lib;
using Nest;
using System.Reflection;

namespace HuloToys_Service.ElasticSearch
{
    public class ArticleRelatedESService : ESRepository<ArticleRelatedViewModel>
    {
        public string index = "article_related_hulotoys_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;

        public ArticleRelatedESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:ArticleRelated"];

        }
        public List<ArticleRelatedESmodel> GetListArticleRelatedByArticleId(long articleid)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<ArticleRelatedESmodel>(sd => sd
                               .Index(index)
                               .Size(4000)
                               .Query(q => q
                                   .Match(m => m.Field("ArticleId").Query(articleid.ToString())
                               )));

                if (query.IsValid)
                {
                    var data = query.Documents as List<ArticleRelatedESmodel>;
                    //var result = data.Select(a => new ArticleRelatedViewModel
                    //{

                    //    Id = a.id,
                    //    ArticleId = a.articleid,
                    //    ArticleRelatedId = a.articleRelatedid,


                    //}).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
    }
}
