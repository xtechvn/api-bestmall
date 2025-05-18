using API_CORE.Controllers.Models;

namespace API_CORE.Controllers.Elasticsearch
{
    public interface IESRepository<TEntity> where TEntity : class
    {
        TEntity FindById(string indexName, object value, string field_name);

    }
}
