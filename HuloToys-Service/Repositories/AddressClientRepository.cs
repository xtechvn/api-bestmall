using HuloToys_Service.Controllers.IRepositories;
using HuloToys_Service.Controllers.SQL;
using Entities.ConfigModels;
using Microsoft.Extensions.Options;
using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Controllers.Repositories
{
    public class AddressClientRepository : IAddressClientRepository
    {
        private readonly AddressClientDAL addressClientDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AddressClientRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            addressClientDAL = new AddressClientDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }

        public int InsertAddressClient(AddressClient addressClient)
        {
            return  addressClientDAL.Insert(addressClient);
        }
    }
}
