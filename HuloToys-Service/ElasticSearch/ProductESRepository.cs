using HuloToys_Service.Elasticsearch;
using Nest;
using Utilities;

namespace Caching.Elasticsearch
{
    //https://www.steps2code.com/post/how-to-use-elasticsearch-in-csharp
    public class ProductESRepository : ESRepository<ProductESModel>
    {
        public string index = "hulotoys_mongodb_product";
        private static string _ElasticHost;
        private static IConfiguration configuration;
        private readonly ElasticClient _client;

        public ProductESRepository(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:Product"];

            var settings = new ConnectionSettings(new Uri(_ElasticHost))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }

        // 1. Function tìm kiếm data theo product_id
        public async Task<ProductESModel> GetByProductIdAsync(string productId)
        {
            var response = await _client.SearchAsync<ProductESModel>(s => s
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.product_id)
                        .Value(productId)
                    )
                )
            );

            return response.Documents.FirstOrDefault();
        }

        // 2. Function xóa theo product_id
        public async Task<bool> DeleteByProductIdAsync(string productId)
        {
            var response = await _client.DeleteByQueryAsync<ProductESModel>(q => q
                .Query(rq => rq
                    .Term(t => t
                        .Field(f => f.product_id)
                        .Value(productId)
                    )
                )
            );

            return response.IsValid && response.Deleted > 0;
        }

        // 3. Function insert vào index
        public async Task<bool> InsertAsync(ProductESModel product)
        {
            var response = await _client.IndexDocumentAsync(product);
            return response.IsValid;
        }
        // Tìm kiếm theo keyword trên trường name và product_code
        public async Task<List<ProductESModel>> SearchByKeywordAsync(string keyword, string keywordNoSpace)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<ProductESModel>();

            Console.WriteLine($"[ElasticSearch] Exact search for keyword: '{keyword}'");

            var shouldQueries = new List<Func<QueryContainerDescriptor<ProductESModel>, QueryContainer>>();

            // 1. Exact phrase match (ưu tiên cao nhất)
            shouldQueries.Add(sh => sh.MatchPhrase(mp => mp
                .Field(f => f.name)
                .Query(keyword)
                .Boost(20)
            ));

            // 2. Prefix match (từ bắt đầu bằng keyword, ví dụ "đăng" -> "đăng ký", "đăng nhập")
            shouldQueries.Add(sh => sh.MatchPhrasePrefix(mpp => mpp
                .Field(f => f.name)
                .Query(keyword)
                .Boost(10)
            ));

            // 3. Wildcard match trên trường không dấu và không khoảng trắng (đã normalize sẵn)
            shouldQueries.Add(sh => sh.Wildcard(w => w
                .Field(f => f.no_space_name)
                .Value($"*{keywordNoSpace.ToLower()}*")
                .Boost(8)
            ));

            // 4. Match all terms (tất cả từ trong keyword phải có trong tên sản phẩm)
            shouldQueries.Add(sh => sh.Match(m => m
                .Field(f => f.name)
                .Query(keyword)
                .Operator(Operator.And)
                .Boost(5)
            ));

            // 5. Fuzzy match (chỉ dùng khi keyword đủ dài để tránh match bậy như "đăng" ra "dán")
            if (keyword.Length >= 5)
            {
                shouldQueries.Add(sh => sh.Match(m => m
                    .Field(f => f.name)
                    .Query(keyword)
                    .Fuzziness(Fuzziness.Auto)
                    .Operator(Operator.And)
                    .Boost(3)
                ));
            }

            // Thực thi truy vấn
            var response = await _client.SearchAsync<ProductESModel>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Should(shouldQueries)
                        .MinimumShouldMatch(1)
                    )
                )
                .Sort(so => so
                    .Descending(SortSpecialField.Score)
                )
                .Size(10)
            );

            return response.Documents.ToList();
        }





    }




}
