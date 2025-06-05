using HuloToys_Service.Elasticsearch;
using Nest;
using Utilities;

namespace Caching.Elasticsearch.FlashSale
{
    
    public class FlashSaleESRepository : ESRepository<FlashSaleESModel>
    {
        public string index = "hulotoys_sp_getflashsale";
        private static string _ElasticHost;
        private static IConfiguration configuration;
        private readonly ElasticClient _client;

        public FlashSaleESRepository(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:FlashSale"];
            var settings = new ConnectionSettings(new Uri(_ElasticHost))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }

        // 1. Function tìm kiếm data theo flashsale_id
        public async Task<FlashSaleESModel> GetByIdAsync(int flashsaleId)
        {
            var response = await _client.SearchAsync<FlashSaleESModel>(s => s
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.flashsale_id)
                        .Value(flashsaleId)
                    )
                )
            );

            return response.Documents.FirstOrDefault();
        }

        // 2. Function xóa theo flashsale_id
        public async Task<bool> DeleteByFlashsaleId(int flashsaleId)
        {
            var response = await _client.DeleteByQueryAsync<FlashSaleESModel>(q => q
                .Query(rq => rq
                    .Term(t => t
                        .Field(f => f.flashsale_id)
                        .Value(flashsaleId)
                    )
                )
            );

            return response.IsValid && response.Deleted > 0;
        }

        // 3. Function insert vào index
        public async Task<bool> InsertAsync(FlashSaleESModel product)
        {
            var response = await _client.IndexDocumentAsync(product);
            return response.IsValid;
        }
        public async Task<List<FlashSaleESModel>> SearchActiveFlashSales()
        {
            var now = DateTime.Now; // It's generally recommended to use UTC for date comparisons in databases/Elasticsearch

            var response = await _client.SearchAsync<FlashSaleESModel>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Filter(
                            bs => bs.DateRange(r => r
                                .Field(f => f.fromdate)
                                .LessThanOrEquals(now)
                            ),
                            bs => bs.DateRange(r => r
                                .Field(f => f.todate)
                                .GreaterThanOrEquals(now)
                            ),
                            bs => bs.Term(t => t
                                .Field(f => f.status)
                                .Value(1)
                            )
                        )
                    )
                )
            );

            if (response.IsValid)
            {
                return response.Documents.ToList();
            }
            else
            {
                return new List<FlashSaleESModel>();
            }
        }
        public async Task<bool> DeleteByIds(List<int> flashsaleIds)
        {
            if (flashsaleIds == null || flashsaleIds.Count == 0)
            {
                return false;
            }

            var response = await _client.DeleteByQueryAsync<FlashSaleESModel>(q => q
                .Query(rq => rq
                    .Terms(t => t // Use .Terms for multiple values
                        .Field(f => f.flashsale_id)
                        .Terms(flashsaleIds)
                    )
                )
            );

            if (response.IsValid)
            {
                return true;
            }
            return false;

        }
        public async Task<bool> IndexMany(List<FlashSaleESModel> flashSales)
        {
            if (flashSales == null || flashSales.Count == 0)
            {
                return false;
            }
            foreach(var item in flashSales)
            {
                await _client.IndexDocumentAsync(item);
            }
            return true;


        }

    }



}
