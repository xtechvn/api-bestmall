using DAL;
using Entities.Models;
using Entities.ViewModels;
using HuloToys_Service.Models.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAllotmentFundRepository
    {
        public int Insert(AllotmentFund model);
        public int Update(AllotmentFund model);
        public AllotmentFund GetByAccountClientId(long accountClientId);

    }
}
