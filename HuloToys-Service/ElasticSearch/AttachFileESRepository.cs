using Elasticsearch.Net;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.Products;
using HuloToys_Service.Elasticsearch;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Caching.Elasticsearch
{
    //https://www.steps2code.com/post/how-to-use-elasticsearch-in-csharp
    public class AttachFileESModelESRepository : ESRepository<AttachFileESModel>
    {
        public string index = "hulotoys_sp_getattachfile";
        private static string _ElasticHost;
        private static IConfiguration configuration;
        private readonly ElasticClient _client;

        public AttachFileESModelESRepository(string Host, IConfiguration _configuration) : base(Host, _configuration)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:AttachFile"];
            var settings = new ConnectionSettings(new Uri(_ElasticHost))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }

        // 1. Function tìm kiếm data theo product_id
        public async Task<AttachFileESModel> GetByid(long id)
        {
            var response = await _client.SearchAsync<AttachFileESModel>(s => s
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.Id)
                        .Value(id)
                    )
                )
            );

            return response.Documents.FirstOrDefault();
        }

        // 2. Function xóa theo product_id
        public async Task<bool> DeleteByProductidAsync(long id)
        {
            var response = await _client.DeleteByQueryAsync<AttachFileESModel>(q => q
                .Query(rq => rq
                    .Term(t => t
                        .Field(f => f.Id)
                        .Value(id)
                    )
                )
            );

            return response.IsValid && response.Deleted > 0;
        }

        // 3. Function insert vào index
        public async Task<bool> InsertAsync(AttachFileESModel product)
        {
            var response = await _client.IndexDocumentAsync(product);
            return response.IsValid;
        }
        public async Task<List<AttachFileESModel>> GetByDataidAndType(long dataid, int? type)
        {
            var response = await _client.SearchAsync<AttachFileESModel>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t.Field(f => f.DataId).Value(dataid)),
                            m => m.Term(t => t.Field(f => f.Type).Value(type))
                        )
                    )
                )
            );
            return response.Documents.ToList();
        }
        public async Task<DeleteByQueryResponse> DeleteByDataidAndType(long dataid, int? type)
        {
            var response = await _client.DeleteByQueryAsync<AttachFileESModel>(q => q
                .Query(qu => qu
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t.Field(f => f.DataId).Value(dataid)),
                            m => m.Term(t => t.Field(f => f.Type).Value(type))
                        )
                    )
                )
            );
            return response;
        }
    }




}
