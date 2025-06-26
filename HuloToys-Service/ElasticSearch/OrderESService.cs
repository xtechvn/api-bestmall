using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Utilities.Lib;
using Nest;
using System.Reflection;
using HuloToys_Service.Models.Orders;
using Utilities.Contants;

namespace Caching.Elasticsearch
{
    public class OrderESService : ESRepository<OrderESModel>
    {
        public string index = "order_hulotoys_store";
        private readonly IConfiguration configuration;
        private readonly ElasticClient elasticClient;
        private static string _ElasticHost;

        public OrderESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:Order"];
            var nodes = new Uri[] { new Uri(_ElasticHost) };
            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index);
            elasticClient = new ElasticClient(connectionSettings);

        }
        public List<OrderESModel> GetByClientID(long client_id)
        {
            List<OrderESModel> result = new List<OrderESModel>();
            try
            {

                var query = elasticClient.Search<OrderESModel>(sd => sd
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public OrderFEResponseModel GetFEByClientID(long client_id, string status, int page_index, int page_size)
        {
            OrderFEResponseModel result = new OrderFEResponseModel();
            try
            {

                if (status == null || status.Trim() == "")
                {
                    Func<QueryContainerDescriptor<OrderESModel>, QueryContainer> query_container = q => q
                                  .Term(m => m.ClientId, client_id);
                    var query = elasticClient.Search<OrderESModel>(sd => sd
                              .Query(query_container)
                              .From((page_index - 1) * page_size)
                              .Size(page_size)
                              .Sort(ss => ss.Descending(o => o.CreatedDate)) // Add sorting by CreatedDate descending

                              );
                    var query_count = elasticClient.Count<OrderESModel>(sd => sd
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
                             .Query(query_container)
                              .From((page_index - 1) * page_size)
                              .Size(page_size)
                              .Sort(ss => ss.Descending(o => o.CreatedDate)) // Add sorting by CreatedDate descending

                             );
                    var query_count = elasticClient.Count<OrderESModel>(sd => sd
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public OrderESModel GetLastestClientID(long client_id)
        {
            OrderESModel result = new OrderESModel();
            try
            {


                var query = elasticClient.Search<OrderESModel>(sd => sd
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<OrderESModel> GetByOrderNo(string text, long client_id)
        {
            List<OrderESModel> result = new List<OrderESModel>();
            try
            {

                var search_response = elasticClient.Search<OrderESModel>(s => s
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return new List<OrderESModel>();
        }
        public async Task<long> CountOrderByYear()
        {

            try
            {

                var query = elasticClient.Count<OrderESModel>(sd => sd
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
        public OrderESModel GetByOrderId(long order_id)
        {
            OrderESModel result = new OrderESModel();
            try
            {
                var query = elasticClient.Search<OrderESModel>(sd => sd
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
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public long CountOrdersByVoucherIdAndClientId(int voucher_id, long client_id)
        {
            try
            {

                var searchResponse = elasticClient.Search<OrderESModel>(sd => sd
                    .Query(q =>
                    {
                        // Khởi tạo một Container cho các điều kiện query
                        QueryContainer queryContainer = q.Term(m => m.VoucherId, voucher_id);

                        // Thêm điều kiện ClientId nếu client_id > 0
                        if (client_id > 0)
                        {
                            queryContainer &= q.Term(m => m.ClientId, client_id);
                        }
                        return queryContainer;
                    })
                    .Size(0) // Chỉ quan tâm đến tổng số, không cần trả về tài liệu
                );

                if (!searchResponse.IsValid)
                {
                    string error_msg = $"Elasticsearch query failed: {searchResponse.DebugInformation ?? searchResponse.ServerError?.Error.ToString()}";
                    LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
                    return 0;
                }
                else
                {
                    return searchResponse.Total; // Lấy tổng số lượng khớp
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
                return 0; // Trả về 0 nếu có lỗi
            }
        }
        public (long allOrdersCount, long status016Count, long status25Count, long status3Count, long status4Count) CountOrdersByStatus(long client_id)
        {
            Func<QueryContainerDescriptor<OrderESModel>, QueryContainer> baseClientQuery = q =>
                q.Match(m => m.Field(x => x.ClientId).Query(client_id.ToString()));

            var allOrdersCountResponse = elasticClient.Count<OrderESModel>(c => c
                .Index(index)
                .Query(baseClientQuery)
            );
            long allOrdersCount = allOrdersCountResponse.IsValid ? allOrdersCountResponse.Count : 0;

            var status016CountResponse = elasticClient.Count<OrderESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Terms(t => t.Field(f => f.OrderStatus).Terms(new[] { 0, 1, 6 })))
            );
            long status016Count = status016CountResponse.IsValid ? status016CountResponse.Count : 0;

            var status25CountResponse = elasticClient.Count<OrderESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Terms(t => t.Field(f => f.OrderStatus).Terms(new[] { 2, 5 })))
            );
            long status25Count = status25CountResponse.IsValid ? status25CountResponse.Count : 0;

            var status3CountResponse = elasticClient.Count<OrderESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Match(m => m.Field(f => f.OrderStatus).Query("3")))
            );
            long status3Count = status3CountResponse.IsValid ? status3CountResponse.Count : 0;

            var status4CountResponse = elasticClient.Count<OrderESModel>(c => c
                .Index(index)
                .Query(q => baseClientQuery(q) && q.Match(m => m.Field(f => f.OrderStatus).Query("4")))
            );
            long status4Count = status4CountResponse.IsValid ? status4CountResponse.Count : 0;
            return (allOrdersCount, status016Count, status25Count, status3Count, status4Count);
        }
    }
}
