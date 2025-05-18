using API_CORE.Controllers.IRepositories;
using API_CORE.Controllers.Models.Models;
using API_CORE.Controllers.SQL;
using DAL;
using Entities.ConfigModels;
using Microsoft.Extensions.Options;
using System;

namespace API_CORE.Controllers.Repositories
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
