using API_CORE.Controllers.Models.Models;

namespace API_CORE.Controllers.IRepositories
{
    public interface IAddressClientRepository
    {
        public int InsertAddressClient(AddressClient addressClient);
    }
}
