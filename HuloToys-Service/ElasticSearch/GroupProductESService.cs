using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.Models.Entities;
using HuloToys_Service.Models.Products;
using HuloToys_Service.Utilities.Lib;
using Nest;
using System.Linq;
using System.Reflection;
using Utilities.Contants;

namespace HuloToys_Service.ElasticSearch
{
    public class GroupProductESService : ESRepository<GroupProduct>
    {
        public string index = "group_product_hulotoys_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;

        public GroupProductESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:GroupProduct"];

        }
        public List<GroupProductESModel> GetListGroupProductByParentId(long parent_id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<GroupProductESModel>(sd => sd
                               .Index(index)
                               .Size(4000)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                  
                                  q => q.Term(m => m.Field("Status").Value(ArticleStatus.PUBLISH))
                                  ,
                                   sh => sh.Term(m => m.Field("ParentId").Value(parent_id.ToString())
                                   )
                                   )
                               )
                          ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<GroupProductESModel>;
                    //var result = data.Select(a => new GroupProduct
                    //{
                    //    Id = a.id,
                    //    ParentId = a.parentid,
                    //    PositionId = a.positionid,
                    //    Name = a.name,
                    //    ImagePath = a.imagepath,
                    //    OrderNo = a.orderno,
                    //    Path = a.path,
                    //    Status = a.status,
                    //    CreatedOn = a.createdon,
                    //    ModifiedOn = a.modifiedon,
                    //    Description = a.description,
                    //    IsShowHeader = a.isshowheader,
                    //    IsShowFooter = a.isshowfooter,

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
        public GroupProductESModel GetDetailGroupProductById(long id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<GroupProductESModel>(sd => sd
                               .Index(index)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                  q => q.Term(m => m.Field("Status").Value(ArticleStatus.PUBLISH)),
                                   sh => sh.Term(m => m.Field("id").Value(id)
                                   )
                                   )
                               )
                          ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<GroupProductESModel>;
                    //var result = data.Select(a => new GroupProduct
                    //{
                    //    Id = a.id,
                    //    ParentId = a.parentid,
                    //    PositionId = a.positionid,
                    //    Name = a.name,
                    //    ImagePath = a.imagepath,
                    //    OrderNo = a.orderno,
                    //    Path = a.path,
                    //    Status = a.status,
                    //    CreatedOn = a.createdon,
                    //    ModifiedOn = a.modifiedon,
                    //    Description = a.description,
                    //    IsShowHeader = a.isshowheader,
                    //    IsShowFooter = a.isshowfooter,

                    //}).ToList();
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
        public List<GroupProductESModel> GetGroupProductByIDs(List<long> ids)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var query = elasticClient.Search<GroupProductESModel>(sd => sd
                              .Index(index)
                         .Query(q =>
                          q.Bool(
                              qb => qb.Must(
                                 q => q.Match(m => m.Field(x=>x.Status).Query(ArticleStatus.PUBLISH.ToString())),
                                  sh => sh.Terms(t => t
                                    .Field(f => f.Id)
                                    .Terms(ids)  // List of IDs to match
                                   )
                                  
                                  )
                              )
                         ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<GroupProductESModel>;
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
    }
}
