using Entities.Models;
using Entities.ViewModels;
using HuloToys_Service.Models.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAccountClientRepository
    {
        long GetMainAccountClientByClientId(long client_id);
        AccountClient AccountClientByClientId(long client_id);
        Task<List<AccountClient>> GetByCondition(Expression<Func<AccountClient, bool>> expression);

    }
}
