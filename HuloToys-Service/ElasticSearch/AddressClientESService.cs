using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Utilities.Lib;
using Nest;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using Utilities;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.Models.Orders;
using Azure.Core;
using Entities.Models;

namespace Caching.Elasticsearch
{
    public class AddressClientESService : ESRepository<AddressClientESModel>
    {
        public string index = "address_client_hulotoys_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;

        public AddressClientESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:AddressClient"];


        }
        public List<AddressClientESModel> GetByClientID(long client_id)
        {
            List<AddressClientESModel> result = new List<AddressClientESModel>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<AddressClientESModel>(sd => sd
                            .Index(index)
                            .Query(q => q.Term(m => m.ClientId,client_id)
                                )
                            .Size(100)

                            );
                if (query.IsValid)
                {
                    var data = query.Documents as List<AddressClientESModel>;
                   // LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetByClientID - AddressClientESService [" + client_id + "][" + JsonConvert.SerializeObject(result) + "]");

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
        public AddressClientESModel GetById(long id,long client_id)
        {
            AddressClientESModel result = new AddressClientESModel();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<AddressClientESModel>(sd => sd
                            .Index(index)
                            .Query(q => q.Term(m => m.id, id) 
                                &&
                                q.Term(m => m.ClientId, client_id)
                                )
                            .Size(100)

                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {

                    var data = query.Documents as List<AddressClientESModel>;
                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = "AddressClientESService" + "->" + "InsertOrUpdateAddress" + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
    }
}
