using DAL;
using Entities.ConfigModels;
using HuloToys_Service.Models.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;

namespace HuloToys_Service.Repositories
{
    public class AllotmentFundRepository : IAllotmentFundRepository
    {
        private readonly AllotmentFundDAL allotmentFundDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AllotmentFundRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            allotmentFundDAL = new AllotmentFundDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }
        public int Insert(AllotmentFund model)
        {
            return allotmentFundDAL.Insert(model);
        }
        public int Update(AllotmentFund model)
        {
            return allotmentFundDAL.Update(model);

        }
        public AllotmentFund GetByAccountClientId(long accountClientId)
        {
            return allotmentFundDAL.GetByAccountClientId(accountClientId);
        }
    }
}
