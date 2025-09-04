using Entities.Models;
using Entities.ViewModels;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAllotmentUseRepository
    {
        public int Insert(AllotmentUse model);
        public int Update(AllotmentUse model);
        public GenericViewModel<AllotmentUse> GetByAccountClientId(long accountClientId, int pageIndex = 1, int pageSize = 10);

    }
}
