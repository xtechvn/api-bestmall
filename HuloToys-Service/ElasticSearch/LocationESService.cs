using Elasticsearch.Net;
using HuloToys_Service.Elasticsearch;
using HuloToys_Service.Utilities.Lib;
using Nest;
using System.Reflection;
using Entities.Models;
using HuloToys_Service.Models.Models;

namespace Caching.Elasticsearch
{
    public class LocationESService : ESRepository<Province>
    {
        public string index_province = "provinces_store";
        public string index_district = "districts_store";
        public string index_wards = "wards_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;

        public LocationESService(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index_province = _configuration["DataBaseConfig:Elastic:Index:Provinces"];
            index_district = _configuration["DataBaseConfig:Elastic:Index:Districts"];
            index_wards = _configuration["DataBaseConfig:Elastic:Index:Wards"];

        }
        public List<Province> GetAllProvinces()
        {
            List<Province> result = new List<Province>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<Province>(sd => sd
                            .Index(index_province)
                            .Size(4000)
                            .Query(q => q
                                .MatchAll()
                                )
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<Province>;
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
        public Province GetProvincesByProvinceId(string provinces_id)
        {
            List<Province> result = new List<Province>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<Province>(sd => sd
                            .Index(index_province)
                            .Size(4000)
                           .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("ProvinceId").Query(provinces_id)
                                   )

                                )))
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    result = query.Documents as List<Province>;
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
        public List<District> GetAllDistrict()
        {
            List<District> result = new List<District>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<District>(sd => sd
                            .Index(index_district)
                             .Size(4000)
                            .Query(q => q
                                .MatchAll()
                                )
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<District>;
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
        public List<District> GetAllDistrictByProvinces(string provinces_id)
        {
            List<District> result = new List<District>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<District>(sd => sd
                            .Index(index_district)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("ProvinceId").Query(provinces_id)
                                   )

                                )))
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<District>;

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
        public District GetDistrictByDistrictId(string district_id)
        {
            List<District> result = new List<District>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<District> (sd => sd
                            .Index(index_district)
                            .Size(4000)
                           .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("DistrictId").Query(district_id)
                                   )

                                ))
                                )
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    result = query.Documents as List<District>;

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
        public List<Ward> GetAllWards()
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<Ward>(sd => sd
                            .Index(index_wards)
                            .Size(4000)
                            .Query(q => q
                                .MatchAll()
                                )
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<Ward>;

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
        public List<Ward> GetAllWardsByDistrictId(string district_id)
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<Ward>(sd => sd
                            .Index(index_wards)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("DistrictId").Query(district_id)
                                   )

                                ))
                                )
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<Ward>;

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
        public Ward GetWardsByWardId(string ward_id)
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<Ward>(sd => sd
                            .Index(index_wards)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("WardId").Query(ward_id)
                                   )

                                ))
                                )
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    result = query.Documents as List<Ward>;

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
    }
}
