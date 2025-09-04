using DAL;
using Entities.ConfigModels;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;

namespace HuloToys_Service.Repositories
{
    public class AllotmentUseRepository: IAllotmentUseRepository
    {
        private readonly AllotmentUseDAL allotmentUseDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AllotmentUseRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            allotmentUseDAL = new AllotmentUseDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }
        public int Insert(AllotmentUse model)
        {
            return allotmentUseDAL.Insert(model);
        }
        public int Update(AllotmentUse model)
        {
            return allotmentUseDAL.Update(model);

        }
        public GenericViewModel<AllotmentUse> GetByAccountClientId(long accountClientId, int pageIndex = 1, int pageSize = 10)
        {
            return allotmentUseDAL.GetByAccountClientId(accountClientId,  pageIndex ,  pageSize);
        }
    }
}
