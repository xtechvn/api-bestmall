using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Utilities.Lib;
using Nest;
using System.Reflection;

namespace HuloToys_Service.ElasticSearch
{
    public class ArticleESService : ESRepository<ArticleViewModel>
    {
        public string index = "hulotoys_sp_getarticle";
        private readonly IConfiguration configuration;
        private static ElasticClient elasticClient;
        private static string _ElasticHost;
        private ISearchResponse<CategoryArticleModel> search_response;
        public ArticleESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:Article"];
        }

        public List<CategoryArticleModel> getListNews(int category_id, int top)
        {
            var data = new List<CategoryArticleModel>();
            try
            {
                if (elasticClient == null)
                {
                    var nodes = new Uri[] { new Uri(_ElasticHost) };
                    var connectionPool = new SniffingConnectionPool(nodes); // Sử dụng Sniffing để khám phá nút khác trong cụm
                    var connectionSettings = new ConnectionSettings(connectionPool)
                        .RequestTimeout(TimeSpan.FromMinutes(2))  // Tăng thời gian chờ nếu cần
                        .SniffOnStartup(true)                     // Khám phá các nút khi khởi động
                        .SniffOnConnectionFault(true)             // Khám phá lại các nút khi có lỗi kết nối
                        .EnableHttpCompression();                // Bật nén HTTP để truyền tải nhanh hơn
                                                                 //.DisableDirectStreaming()  // Kích hoạt ghi lại luồng request/response
                                                                 //.PrettyJson();              // Định dạng kết quả JSON cho dễ đọc

                    elasticClient = new ElasticClient(connectionSettings);
                }
                if (category_id <= 0)
                {
                    // Lấy ra toàn bộ các bài viết của các chuyên mục theo thời gian bài nào mới nhất lên đầu
                    search_response = elasticClient.Search<CategoryArticleModel>(s => s
                   .Size(top) // Lấy ra số lượng bản ghi (ví dụ 100)
                   .Index(configuration["DataBaseConfig:Elastic:Index:Article"])  // Chỉ mục bạn muốn tìm kiếm
                       .Sort(sort => sort
                           .Descending(f => f.publish_date) // Sắp xếp giảm dần theo publishdate
                       )
                   );
                }
                else
                {
                    search_response = elasticClient.Search<CategoryArticleModel>(s => s
                        .Size(top)
                        .Index(configuration["DataBaseConfig:Elastic:Index:Article"])  // Chỉ mục muốn tìm kiếm
                        .Sort(sort => sort
                            .Descending(f => f.publish_date)
                        )
                       .Query(q => q
                            .Bool(b => b
                                .Must(m => m
                                    .Wildcard(w => w
                                        .Field(f => f.list_category_id)
                                        .Value("*" + category_id.ToString() + "*") // Tìm các chuỗi chứa ký tự liên quan
                                    )
                                )
                            )
                        )
                    );
                }

                if (search_response.IsValid)
                {
                    data = search_response.Documents.ToList();
                }

                return data;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return data;
            }
        }

        /// <summary>
        /// Lấy ra chi tiết bài viết
        /// </summary>
        /// <param name="id">articleID</param>
        /// <returns></returns>
        public ArticleModel2 GetArticleDetailById(long id)
        {
            try
            {
                if (elasticClient == null)
                {
                    var nodes = new Uri[] { new Uri(_ElasticHost) };
                    var connectionPool = new SniffingConnectionPool(nodes); // Sử dụng Sniffing để khám phá nút khác trong cụm
                    var connectionSettings = new ConnectionSettings(connectionPool)
                        .RequestTimeout(TimeSpan.FromMinutes(2))  // Tăng thời gian chờ nếu cần
                        .SniffOnStartup(true)                     // Khám phá các nút khi khởi động
                        .SniffOnConnectionFault(true)             // Khám phá lại các nút khi có lỗi kết nối
                        .EnableHttpCompression();                 // Bật nén HTTP để truyền tải nhanh hơn

                    elasticClient = new ElasticClient(connectionSettings);
                }

                var query = elasticClient.Search<ArticleModel2>(sd => sd
                 .Index(configuration["DataBaseConfig:Elastic:Index:Article"])  // Chỉ mục bạn muốn tìm kiếm
               .Query(q => q
                   .Term(t => t.Field(f => f.id).Value(id))  // Tìm kiếm chính xác theo giá trị id (dạng int)
               ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<ArticleModel2>;
                    return data.FirstOrDefault();
                }


            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }

        public int getTotalItemNewsByCategoryId(int category_id)
        {
            try
            {
                int totalCount = 0;
                if (elasticClient == null)
                {
                    var nodes = new Uri[] { new Uri(_ElasticHost) };
                    var connectionPool = new SniffingConnectionPool(nodes); // Sử dụng Sniffing để khám phá nút khác trong cụm
                    var connectionSettings = new ConnectionSettings(connectionPool)
                        .RequestTimeout(TimeSpan.FromMinutes(2))  // Tăng thời gian chờ nếu cần
                        .SniffOnStartup(true)                     // Khám phá các nút khi khởi động
                        .SniffOnConnectionFault(true)             // Khám phá lại các nút khi có lỗi kết nối
                        .EnableHttpCompression();                 // Bật nén HTTP để truyền tải nhanh hơn


                    elasticClient = new ElasticClient(connectionSettings);

                }
                if (category_id > 0)
                {
                    var countResponse = elasticClient.Count<CategoryArticleModel>(c => c
                    .Index(configuration["DataBaseConfig:Elastic:Index:Article"])  // Chỉ mục bạn muốn tìm kiếm
                    .Query(q => q
                        .Term(t => t.Field("categoryid").Value(category_id))  // Tìm theo category_id
                    ));
                    totalCount = Convert.ToInt32(countResponse.Count);
                }
                else
                {
                    var countResponse = elasticClient.Count<CategoryArticleModel>(c => c.Index(configuration["DataBaseConfig:Elastic:Index:Article"]));
                    totalCount = Convert.ToInt32(countResponse.Count);
                }

                return Convert.ToInt32(totalCount);
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }

            return 0;
        }
        public ArticleESModel GetDetailById(long id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<ArticleESModel>(sd => sd
                               .Index(index)
                               .Query(q => q
                                   .Term(m => m.Field("Id").Value(id)
                               )));

                if (query.IsValid)
                {

                    var data = query.Documents as List<ArticleESModel>;

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
        public List<ArticleESModel> GetListArticle()
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<ArticleESModel>(sd => sd
                               .Index(index)
                               .Size(100)
                               .Query(q => q.MatchAll()
                               ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<ArticleESModel>;
                    //var result = data.Select(a => new ArticleViewModel
                    //{

                    //    Id = a.id,
                    //    Title = a.title,
                    //    Lead = a.lead,
                    //    Body = a.body,
                    //    Status = a.status,
                    //    ArticleType = a.articletype,
                    //    PageView = a.pageview,
                    //    PublishDate = a.publishdate,
                    //    AuthorId = a.authorid,
                    //    Image169 = a.image169,
                    //    Image43 = a.image43,
                    //    Image11 = a.image11,
                    //    CreatedOn = a.createdon,
                    //    ModifiedOn = a.modifiedon,
                    //    DownTime = a.downtime,
                    //    UpTime = a.uptime,
                    //    Position = a.position,

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
        public List<ArticleESModel> GetListArticlePosition()
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<ArticleESModel>(sd => sd
                               .Index(index)
                               .Query(q => q
                                   .Range(m => m.Field("Position").GreaterThanOrEquals(1).LessThanOrEquals(7)
                               )));
                if (query.IsValid)
                {
                    var data = query.Documents as List<ArticleESModel>;
                    //var result = data.Select(a => new ArticleViewModel
                    //{

                    //    Id = a.id,
                    //    Title = a.title,
                    //    Lead = a.lead,
                    //    Body = a.body,
                    //    Status = a.status,
                    //    ArticleType = a.articletype,
                    //    PageView = a.pageview,
                    //    PublishDate = a.publishdate,
                    //    AuthorId = a.authorid,
                    //    Image169 = a.image169,
                    //    Image43 = a.image43,
                    //    Image11 = a.image11,
                    //    CreatedOn = a.createdon,
                    //    ModifiedOn = a.modifiedon,
                    //    DownTime = a.downtime,
                    //    UpTime = a.uptime,
                    //    Position = a.position,

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
        public List<ArticleRelationModel> GetListArticleByBody(string txt_search)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<ArticleESModel>(sd => sd
                               .Index(index)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                  //q => q.Term("id", id),
                                   sh => sh.QueryString(qs => qs
                                   .Fields(new[] { "Title", "Lead", "Body" })
                                   .Query("*" + txt_search + "*")
                                   .Analyzer("standard")

                            )
                           )
                               )));

                if (query.IsValid)
                {

                    var data = query.Documents as List<ArticleESModel>;
                    var result = data.Select(a => new ArticleRelationModel
                    {
                        Id = a.Id,
                        Lead = a.Lead,
                        Image = a.Image169 ?? a.Image43 ?? a.Image11,
                        Title = a.Title,
                        publish_date = a.PublishDate ?? DateTime.Now,
                    }).ToList();
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
    }
}
