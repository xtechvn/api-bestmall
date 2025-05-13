using HuloToys_Service.Models;

namespace HuloToys_Service.Elasticsearch
{
    public interface IESRepository<TEntity> where TEntity : class
    {
        TEntity FindById(string indexName, object value, string field_name);

    }
}
