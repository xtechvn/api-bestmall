using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using HuloToys_Service.Models.Models;
using Newtonsoft.Json;

namespace HuloToys_Service.Controllers.SQL
{
    public class ArticleTagDAL : GenericService<ArticleTag>
    {
        private static DbWorker _DbWorker;
        public ArticleTagDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public List<long> GetTagIDByArticleID(long articleID)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var a= _DbContext.ArticleTags.Where(s => s.ArticleId == articleID).Select(s => s.TagId);
                    if(a!=null && a.Count() > 0)
                    {
                       var json = JsonConvert.SerializeObject(a.Distinct().ToList());
                       return JsonConvert.DeserializeObject<List<long>>(json);
                    }
                }
            }
            catch
            {
            }
            return null;
        }
    }
}
