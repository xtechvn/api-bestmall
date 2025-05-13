using Entities.Models;
using HuloToys_Service.Models.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HuloToys_Service.IRepositories
{
    public interface IClientRepository
    {
        Task<Client> GetClientDetailAsync(long clientId);
        Task<List<Client>> GetClientType(int Type);
        Task<List<AccountClient>> GetUserCreateSuggest(string name);
        Task<Client> GetClientDetailByClientId(long clientId);
        List<Client> GetAllClient();
        Client GetClientByEmail(string email);
        Client GetClientByTaxNo(string Maso_id);
        int SetUpClient(Client model);
        Task<Client> GetClientByClientCode(string client_code);
        Task<List<Client>> GetByCondition(Expression<Func<Client, bool>> expression);
    }
}
