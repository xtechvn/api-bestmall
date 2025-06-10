using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Controllers.IRepositories
{
    public interface IAddressClientRepository
    {
        public int InsertAddressClient(AddressClient addressClient);
    }
}
