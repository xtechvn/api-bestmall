using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Models.Account;
using HuloToys_Service.Utilities.Lib;
using Nest;
using Newtonsoft.Json;
using System.Reflection;
using Telegram.Bot.Types;

namespace HuloToys_Service.ElasticSearch
{
    public class AccountApiESService : ESRepository<AccountApiESModel>
    {
        public string index = "hulotoys_sp_getaccountaccessapi";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;

        public AccountApiESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:Authentication"];

        }
        public AccountApiESModel GetByUsername(string user_name)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                //var query = elasticClient.Search<AccountApiESModel>(sd => sd
                //               .Index(index)
                //               .Query(q => q
                //                   .Match(m => m.Field("username").Query(user_name)
                //               )));
                var searchResponse = elasticClient.Search<AccountApiESModel>(s => s
                    .Index(index) // Specify the index
                    .From(0) // Starting point for results
                    .Size(40) // Number of results to return
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .Match(ma => ma
                                    .Field("UserName") // Field to match
                                    .Query(user_name) // Value to match
                                )
                            )
                        )
                    )
                );
                if (searchResponse.IsValid)
                {
                    var result = searchResponse.Documents as List<AccountApiESModel>;
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetByUsername - AccountApiESService Error" + ex.ToString());
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegram(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
    }
}
