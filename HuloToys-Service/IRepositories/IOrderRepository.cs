using Entities.Models;
using Entities.ViewModels;
using API_CORE.Controllers.Models.Article;
using API_CORE.Controllers.Models.Models;
using System.Linq.Expressions;

namespace Repositories.IRepositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrder(Order order);
        public long CountOrderInYear();
        Task<List<Order>> GetByCondition(Expression<Func<Order, bool>> expression);
    }
}
