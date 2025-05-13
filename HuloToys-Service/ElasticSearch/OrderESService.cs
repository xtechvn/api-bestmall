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
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Caching.Elasticsearch
{
    public class OrderESService : ESRepository<OrderESModel>
    {
        public string index = "order_hulotoys_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;

        public OrderESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:Order"];


        }
        public List<OrderESModel> GetByClientID(long client_id)
        {
            List<OrderESModel> result = new List<OrderESModel>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<OrderESModel>(sd => sd
                            .Index(index)
                            .Query(q => q
                                .Term(m => m.ClientId, client_id)
                            )

                            .Size(100)

                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<OrderESModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public OrderFEResponseModel GetFEByClientID(long client_id, string status, int page_index, int page_size)
        {
            OrderFEResponseModel result = new OrderFEResponseModel();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                if (status == null || status.Trim() == "")
                {
                    Func<QueryContainerDescriptor<OrderESModel>, QueryContainer> query_container = q => q
                                  .Term(m => m.ClientId, client_id);
                    var query = elasticClient.Search<OrderESModel>(sd => sd
                              .Index(index)
                              .Query(query_container)
                              .From((page_index - 1) * page_size)
                              .Size(page_size)

                              );
                    var query_count = elasticClient.Count<OrderESModel>(sd => sd
                              .Index(index)
                              .Query(query_container)
                              );
                    if (!query.IsValid && !query_count.IsValid)
                    {
                        return result;
                    }
                    else
                    {
                        result.data = query.Documents as List<OrderESModel>;
                        result.total = query_count.Count;
                        result.page_index = page_index;
                        result.page_size = page_size;
                        return result;
                    }
                   
                }
                else
                {
                    Func<QueryContainerDescriptor<OrderESModel>, QueryContainer> query_container = q =>
                                q.Match(m => m.Field(x => x.ClientId).Query(client_id.ToString()))
                                 &&
                                q.Terms(t => t.Field(x => x.OrderStatus).Terms(status.Split(",")))
                                ;
                    var query = elasticClient.Search<OrderESModel>(sd => sd
                             .Index(index)
                             .Query(query_container)
                              .From((page_index - 1) * page_size)
                              .Size(page_size)
                             );
                    var query_count = elasticClient.Count<OrderESModel>(sd => sd
                             .Index(index)
                             .Query(query_container)
                             );
                    if (!query.IsValid)
                    {
                        return result;
                    }
                    else
                    {
                        result.data = query.Documents as List<OrderESModel>;
                        result.total = query_count.Count;
                        result.page_index = page_index;
                        result.page_size = page_size;
                        return result;
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
        public OrderESModel GetLastestClientID(long client_id)
        {
            OrderESModel result = new OrderESModel();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<OrderESModel>(sd => sd
                               .Index(index)

                               .Query(q => q
                                   .Term(m => m.ClientId, client_id)
                                   )
                                .Sort(q => q.Descending(u => u.CreatedDate))); ;

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    var rs = query.Documents as List<OrderESModel>;
                    return rs.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public List<OrderESModel> GetByOrderNo(string text, long client_id)
        {
            List<OrderESModel> result = new List<OrderESModel>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<OrderESModel>(s => s
                        .Index(index)
                        .Size(4000)
                        .Query(q =>
                         q.Bool(
                             qb => qb.Must(
                                 q => q.Term("ClientId", client_id.ToString()),
                                 q => q.QueryString(qs => qs
                                 .Fields(new[] { "OrderNo" })
                                 .Query("*" + text.ToUpper() + "*")
                                 .Analyzer("standard")

                          )
                         )
                        )));

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<OrderESModel>;
                    return result ?? new List<OrderESModel>();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return new List<OrderESModel>();
        }
        public async Task<long> CountOrderByYear()
        {

            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Count<OrderESModel>(sd => sd
                                   .Index(index)
                                  .Query(q =>
                                   q.Bool(
                                       qb => qb.Must(
                                          q => q.DateRange(m => m
                                          .Name("CreatedDate")
                                          .GreaterThanOrEquals(new DateTime(DateTime.Now.Year, 01, 01, 0, 0, 0).ToString("dd/MM/yyyy"))
                                          .Format("dd/MM/yyyy")
                                          .TimeZone("+07:00")
                                           )
                                           )
                                       )
                                  ));
                if (query.IsValid)
                {
                    return query.Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return -1;
        }
        public OrderESModel GetByOrderId(long order_id)
        {
            OrderESModel result = new OrderESModel();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<OrderESModel>(sd => sd
                               .Index(index)

                               .Query(q => q
                                    .Term(m => m.Id, order_id)
                                   ));

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    var data = query.Documents as List<OrderESModel>;
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
