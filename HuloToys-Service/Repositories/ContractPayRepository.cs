using DAL;
using Entities.ConfigModels;
using Entities.ViewModels;
using HuloToys_Service.Models.Models;
using HuloToys_Service.SQL;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;

namespace Repositories.Repositories
{
    public class ContractPayRepository : IContractPayRepository
    {
        private readonly ContractPayDAL _contractPayDAL;
        private readonly AllCodeDAL allCodeDAL;
        private readonly OrderDAL orderDAL;
        private readonly ClientDAL clientDAL;
        private readonly ContractPayDetailDAL contractPayDetailDAL;

        public ContractPayRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            allCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            orderDAL = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            contractPayDetailDAL = new ContractPayDetailDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public int CreateContractPay(ContractPayViewModel model)
        {
            var entity = _contractPayDAL.GetByBillNo(model.BillNo);
            if (entity != null)
                return -2;
            return _contractPayDAL.CreateContractPay(model);
        }
        public ContractPayDetail ContractPayDetailByServiceCode(string service_code)
        {
            return contractPayDetailDAL.GetByServiceCode(service_code);
        }
    }
}
