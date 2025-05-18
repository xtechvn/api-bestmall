using DAL;
using Entities.ConfigModels;
using Entities.Models;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace Repositories.IRepositories
{
    public class AccountClientRepository : IAccountClientRepository
    {
        private readonly AccountClientDAL accountClientDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AccountClientRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            accountClientDAL = new AccountClientDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }

        public long GetMainAccountClientByClientId(long client_id)
        {
            return  accountClientDAL.GetMainAccountClientByClientId(client_id);
        }
        public AccountClient AccountClientByClientId(long client_id)
        {
            return accountClientDAL.AccountClientByClientId(client_id);
        }
        public async Task<List<AccountClient>> GetByCondition(Expression<Func<AccountClient, bool>> expression)
        {
            try
            {
                var data = accountClientDAL.GetByCondition(expression);
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByCondition - ClientRepository: " + ex);
                return null;
            }
        }
    }
}
