using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Utilities.Lib;
using Nest;
using System.Reflection;
using HuloToys_Service.Models.Orders;
using Utilities.Contants;
using Newtonsoft.Json;

namespace Caching.Elasticsearch
{
    public class OrderMergeESService : ESRepository<OrderMergeESModel>
    {
        public string index = "hulotoys_sp_getordermerge";
        private readonly IConfiguration configuration;
        private readonly ElasticClient elasticClient;
        private static string _ElasticHost;

        public OrderMergeESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:OrderMerge"];
            var nodes = new Uri[] { new Uri(_ElasticHost) };
            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index);
            elasticClient = new ElasticClient(connectionSettings);

        }
        public List<OrderMergeESModel> GetByClientID(long client_id)
        {
            List<OrderMergeESModel> result = new List<OrderMergeESModel>();
            try
            {

                var query = elasticClient.Search<OrderMergeESModel>(sd => sd
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
                    result = query.Documents as List<OrderMergeESModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public OrderMergeFEResponseModel GetFEByClientID(long client_id, string status, string order_no, int page_index, int page_size)
        {
            OrderMergeFEResponseModel result = new OrderMergeFEResponseModel();

            try
            {
                // Build a list of QueryContainer predicates
                var mustQueries = new List<Func<QueryContainerDescriptor<OrderMergeESModel>, QueryContainer>>
                {
                    // Always add ClientId filter
                    q => q.Term(m => m.ClientId, client_id)
                };

                // Add OrderNo containment filter if order_no is provided
                if (order_no != null && order_no.Trim() != "")
                {
                    mustQueries.Add(q => q.Wildcard(w => w.Field(f => f.OrderNo).Value($"*{order_no}*")));
                }

                // Add OrderStatus filter if status is provided
                if (!string.IsNullOrWhiteSpace(status))
                {
                    List<int> status_value = new List<int>();
                    try
                    {
                        var split = status.Split(",");
                        if (split.Length > 0)
                        {
                            foreach (var item in split)
                            {
                                status_value.Add(Convert.ToInt32(item));
                            }

                        }
                    }
                    catch { }
                    mustQueries.Add(q => q.Terms(t => t.Field(x => x.OrderStatus).Terms(status_value)));
                }

                // Combine all 'must' queries using Bool.Must
                Func<QueryContainerDescriptor<OrderMergeESModel>, QueryContainer> finalQueryContainer = q => q
                    .Bool(b => b.Must(mustQueries.ToArray())); // Convert list to array for Must method

                var searchRequest = new SearchDescriptor<OrderMergeESModel>()
                    .Query(finalQueryContainer)
                    .From((page_index - 1) * page_size)
                    .Size(page_size)
                    .Sort(ss => ss.Descending(o => o.CreatedDate));

                var query = elasticClient.Search<OrderMergeESModel>(searchRequest);

                var countRequest = new CountDescriptor<OrderMergeESModel>().Query(finalQueryContainer);

                var query_count = elasticClient.Count(countRequest);

                if (!query.IsValid || !query_count.IsValid)
                {
                    // Trả về kết quả rỗng nếu query hoặc count không hợp lệ.
                    return result;
                }
                else
                {
                    // Thay đổi cách deserialize để bắt lỗi cụ thể
                    try
                    {
                        result.data = query.Documents.ToList();
                    }
                    catch (Exception deserializeEx)
                    {
                        string error_msg = "Deserialize error: " + deserializeEx.Message;
                        LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
                        // Trả về kết quả rỗng hoặc throw exception tùy theo business logic
                        return result;
                    }

                    result.total = query_count.Count;
                    result.page_index = page_index;
                    result.page_size = page_size;
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], "General error"+ error_msg);
            }

            return null;
        }
        public OrderMergeESModel GetLastestClientID(long client_id)
        {
            OrderMergeESModel result = new OrderMergeESModel();
            try
            {


                var query = elasticClient.Search<OrderMergeESModel>(sd => sd
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
                    var rs = query.Documents as List<OrderMergeESModel>;
                    return rs.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<OrderMergeESModel> GetByOrderNo(string text, long client_id)
        {
            List<OrderMergeESModel> result = new List<OrderMergeESModel>();
            try
            {

                var search_response = elasticClient.Search<OrderMergeESModel>(s => s
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
                    result = search_response.Documents as List<OrderMergeESModel>;
                    return result ?? new List<OrderMergeESModel>();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return new List<OrderMergeESModel>();
        }
        public async Task<long> CountOrderByYear()
        {

            try
            {

                var query = elasticClient.Count<OrderMergeESModel>(sd => sd
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return -1;
        }
        public OrderMergeESModel GetByOrderId(long order_id)
        {
            OrderMergeESModel result = new OrderMergeESModel();
            try
            {
                var query = elasticClient.Search<OrderMergeESModel>(sd => sd
                               .Query(q => q
                                    .Term(m => m.Id, order_id)
                                   ));

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    var data = query.Documents as List<OrderMergeESModel>;
                    return data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public (long all, long waiting, long delvering, long finish, long refund, long cancel, long processing) CountOrdersByStatus(long client_id)
        {
            Func<QueryContainerDescriptor<OrderMergeESModel>, QueryContainer> baseClientQuery = q =>
                q.Match(m => m.Field(x => x.ClientId).Query(client_id.ToString()));

            var allCountResponse = elasticClient.Count<OrderMergeESModel>(c => c
                .Index(index)
                .Query(baseClientQuery)
            );
            long all = allCountResponse.IsValid ? allCountResponse.Count : 0;

            var waitingResponse = elasticClient.Count<OrderMergeESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Terms(t => t.Field(f => f.OrderStatus).Terms(new[] { 0 })))
            );
            long waiting = allCountResponse.IsValid ? allCountResponse.Count : 0;

            var delveringResponse = elasticClient.Count<OrderMergeESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Terms(t => t.Field(f => f.OrderStatus).Terms(new[] { 2,5})))
            );
            long delvering = allCountResponse.IsValid ? allCountResponse.Count : 0;

            var finishResponse = elasticClient.Count<OrderMergeESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Terms(m => m.Field(f => f.OrderStatus).Terms(new[] { 3 })))
            );
            long finish = finishResponse.IsValid ? finishResponse.Count : 0;

            var refundResponse = elasticClient.Count<OrderMergeESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Terms(m => m.Field(f => f.OrderStatus).Terms(new[] { 7 })))
            );
            long refund = refundResponse.IsValid ? refundResponse.Count : 0;

            var cancelResponse = elasticClient.Count<OrderMergeESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Terms(m => m.Field(f => f.OrderStatus).Terms(new[] { 4 })))
            );
            long cancel = cancelResponse.IsValid ? cancelResponse.Count : 0;

            var processingResponse = elasticClient.Count<OrderMergeESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Terms(m => m.Field(f => f.OrderStatus).Terms(new[] { 1,6 })))
            );
            long processing = processingResponse.IsValid ? processingResponse.Count : 0;
            return (all, waiting, delvering, finish, refund, cancel, processing);
        }
        public OrderMergeFEResponseModel GetFEAffiliateByClientID(long client_id, string status, string order_no, int page_index, int page_size,string utm_medium="")
        {
            OrderMergeFEResponseModel result = new OrderMergeFEResponseModel();

            try
            {
                // Build a list of QueryContainer predicates
                var mustQueries = new List<Func<QueryContainerDescriptor<OrderMergeESModel>, QueryContainer>>
                {
                    // Always add ClientId filter
                    q => q.Term(m => m.ClientId, client_id)
                };

                // Add OrderNo containment filter if order_no is provided
                if (order_no != null && order_no.Trim() != "")
                {
                    mustQueries.Add(q => q.Wildcard(w => w.Field(f => f.OrderNo).Value($"*{order_no}*")));
                }

                // Add OrderStatus filter if status is provided
                if (!string.IsNullOrWhiteSpace(status))
                {
                    List<int> status_value = new List<int>();
                    try
                    {
                        var split = status.Split(",");
                        if (split.Length > 0)
                        {
                            foreach (var item in split)
                            {
                                status_value.Add(Convert.ToInt32(item));
                            }

                        }
                    }
                    catch { }
                    mustQueries.Add(q => q.Terms(t => t.Field(x => x.OrderStatus).Terms(status_value)));
                }
                mustQueries.Add(q => q.Terms(t => t.Field(x => x.UtmMedium).Terms(utm_medium)));

                // Combine all 'must' queries using Bool.Must
                Func<QueryContainerDescriptor<OrderMergeESModel>, QueryContainer> finalQueryContainer = q => q
                    .Bool(b => b.Must(mustQueries.ToArray())); // Convert list to array for Must method

                var searchRequest = new SearchDescriptor<OrderMergeESModel>()
                    .Query(finalQueryContainer)
                    .From((page_index - 1) * page_size)
                    .Size(page_size)
                    .Sort(ss => ss.Descending(o => o.CreatedDate));

                var query = elasticClient.Search<OrderMergeESModel>(searchRequest);

                var countRequest = new CountDescriptor<OrderMergeESModel>().Query(finalQueryContainer);

                var query_count = elasticClient.Count(countRequest);

                if (!query.IsValid || !query_count.IsValid)
                {
                    // Trả về kết quả rỗng nếu query hoặc count không hợp lệ.
                    return result;
                }
                else
                {
                    // Thay đổi cách deserialize để bắt lỗi cụ thể
                    try
                    {
                        result.data = query.Documents.ToList();
                    }
                    catch (Exception deserializeEx)
                    {
                        string error_msg = "Deserialize error: " + deserializeEx.Message;
                        LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
                        // Trả về kết quả rỗng hoặc throw exception tùy theo business logic
                        return result;
                    }

                    result.total = query_count.Count;
                    result.page_index = page_index;
                    result.page_size = page_size;
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], "General error" + error_msg);
            }

            return null;
        }
    }
}
