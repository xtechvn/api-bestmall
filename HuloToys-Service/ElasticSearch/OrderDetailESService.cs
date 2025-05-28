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
using entities.models;
using HuloToys_Service.Models;
using Azure;
using static System.Net.Mime.MediaTypeNames;

namespace Caching.Elasticsearch
{
    public class OrderDetailESService : ESRepository<OrderDetailESModel>
    {
        public string index = "order_detail_hulotoys_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;
        private static ElasticClient elasticClient;

        public OrderDetailESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:OrderDetail"];
            var nodes = new Uri[] { new Uri(_ElasticHost) };
            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index);
            elasticClient = new ElasticClient(connectionSettings);

        }
        public long CountByProductId(List<string> product_id)
        {
            try
            {

                var searchRequest = new SearchRequest<OrderDetailESModel>
                {
                    Query = new TermsQuery
                    {
                        Field = Infer.Field<OrderDetailESModel>(p => p.ProductId), // Field selector for ProductId
                        Terms = product_id // The list of product IDs
                    },
                    Size = 0, // No hits needed, just aggregations
                    Aggregations = new AggregationDictionary
                        {
                             {
                               "total_product_count", new FiltersAggregation("total_product_count")
                               {
                                    Filters = new List<QueryContainer>()
                                    {
                                       new ExistsQuery { Field = Infer.Field<OrderDetailESModel>(x => x.ProductId) },

                                    }
                               }
                            }
                        }
                };
                var response = elasticClient.Search<OrderDetailESModel>(searchRequest);
                if (response.IsValid)
                {
                    // Process the field data counts (description and information)
                    var total_product_count = response.Aggregations.Filters("total_product_count");
                    return total_product_count.Buckets.First().DocCount; // Products with 'description' field
                }
              
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return 0;
        }
        public long SumQuantityByProductId(List<string> product_id)
        {
            try
            {
                var searchRequest = new SearchRequest<OrderDetailESModel>
                {
                    Query = new TermsQuery
                    {
                        Field = Infer.Field<OrderDetailESModel>(p => p.ParentProductId), // Field selector for ProductId
                        Terms = product_id // The list of product IDs
                    },
                    Size = 0, // No hits needed, just aggregations
                    Aggregations = new AggregationDictionary
                    {
                        {
                            "total_quantity", new SumAggregation("total_quantity", Infer.Field<OrderDetailESModel>(p => p.Quantity))
                        }
                    }
                };

                var response = elasticClient.Search<OrderDetailESModel>(searchRequest);

                if (response.IsValid)
                {
                    // Process the sum aggregation
                    var totalQuantityAggregation = response.Aggregations.Sum("total_quantity");
                    if (totalQuantityAggregation != null && totalQuantityAggregation.Value.HasValue)
                    {
                        return (long)totalQuantityAggregation.Value.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return 0;
        }
    }
}
