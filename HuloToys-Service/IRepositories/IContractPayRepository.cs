using Entities.ViewModels;
using HuloToys_Service.Models.Models;

namespace Repositories.IRepositories
{
    public interface IContractPayRepository
    {

        int CreateContractPay(ContractPayViewModel model);
        ContractPayDetail ContractPayDetailByServiceCode(string service_code);
    }
}
