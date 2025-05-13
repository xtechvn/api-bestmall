using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Utilities.Lib;
using Nest;
using System.Reflection;
using HuloToys_Service.Models.Account;
using HuloToys_Service.Models.Client;
using HuloToys_Service.Utilities.Common;
using Newtonsoft.Json;

namespace Caching.Elasticsearch
{
    public class AccountClientESService : ESRepository<AccountESModel>
    {
        public string index = "account_client_hulotoys_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;

        public AccountClientESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:AccountClient"];

        }
        public AccountESModel GetByUsername(string user_name)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                               .Query(q => q
                                   .Match(m => m.Field(y=>y.UserName).Query(user_name)
                               )));

                if (query.IsValid)
                {
                    var result = query.Documents as List<AccountESModel>;
                   // LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetByUsername - AccountClientESService ["+user_name+"]["+ JsonConvert.SerializeObject(result) + "]" );
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
      
        public AccountESModel GetById(long id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                               .Query(q => q
                                   .Term(m => m.Id, id)
                               ));

                if (query.IsValid)
                {
                    var result = query.Documents as List<AccountESModel>;
                    //LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetById - AccountClientESService [" + id + "][" + JsonConvert.SerializeObject(result) + "]");
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public AccountESModel GetByUsernameAndPassword(string user_name, string password)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                             .Query(q =>
                               q.Bool(
                                   qb => qb.Must( q=>
                                      q.Match(qs => qs
                                        .Field(s => s.UserName)
                                        .Query(user_name)
                                       )
                                     && q.Match(qs => qs
                                        .Field(s => s.Password)
                                        .Query(password)
                                       )

                                    )
                               )
                            ));


                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;
                    if (data != null && data.Count>0) { data = data.Where(x => x.UserName.Trim().ToLower()== user_name.ToLower() && x.Password == password).ToList(); }
                    if(data!=null && data.Count > 0)
                    {
                        return data.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public AccountESModel GetByUsernameAndGoogleToken(string user_name, string token)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                             .Query(q =>
                               q.Bool(
                                   qb => qb.Must(
                                      qb => qb.Match(qs => qs
                                        .Field(s => s.UserName)
                                        .Query(user_name)
                                       )

                                    )
                               )
                            ));


                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;

                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public AccountESModel GetByClientIdAndPassword(long client_id, string password)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                             .Query(q =>
                               q.Bool(
                                   qb => qb.Must(
                                      qb => qb.Term(x=>x.ClientId, client_id) 
                                      &&
                                       qb.Match(qs => qs
                                        .Field(s => s.Password)
                                        .Query(password)
                                       )

                                    )
                               )
                            ));


                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;

                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public AccountESModel GetByClientID(long client_id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);


                var query = elasticClient.Search<AccountESModel>(sd => sd
                             .Index(index)
                           .Query(q =>
                             q.Bool(
                                 qb => qb.Must(
                                    qb => qb.Term(x=>x.ClientId, client_id)


                                  )
                             )
                          ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;

                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public AccountESModel GetByClientIdAndGoogleToken(long client_id, string token)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var queryContainer = new BoolQuery
                {
                    Must = new List<QueryContainer>
                    {
                        new MatchQuery
                        {
                            Field = "GoogleToken",
                            Query = token
                        },
                        new TermQuery
                        {
                            Field = "ClientId",
                            Value = client_id
                        }
                    }
                };
                var query = elasticClient.Search<AccountESModel>(sd => sd
                               .Index(index)
                             .Query(q=>queryContainer));


                if (query.IsValid)
                {
                    var data = query.Documents as List<AccountESModel>;

                    return data.FirstOrDefault();
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
