using DAL;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAL _OrderDal;
        private readonly ClientDAL _clientDAL;


        public OrderRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _OrderDal = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);

        }
        


        public async Task<Order> CreateOrder(Order order)
        {
            try
            {

                var id = await _OrderDal.CreateOrder(order);
                if (order.OrderId > 0)
                {
                    return order;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateOrder in OrderRepository: " + ex);
            }
            return null;
        }
        public long CountOrderInYear()
        {
            return _OrderDal.CountOrderInYear();
        }
        public async Task<List<Order>> GetByCondition(Expression<Func<Order, bool>> expression)
        {
            try
            {
                var data = _OrderDal.GetByCondition(expression);
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByCondition - OrderRepository: " + ex);
                return null;
            }
        }
        public Order GetByOrderNo(string orderNo)
        {
            return _OrderDal.GetByOrderNo(orderNo);
        }
        public Task<long> UpdateOrderStatus(Order model)
        {
            return _OrderDal.UpdateOrderStatus(model);
        }
        public Task<OrderDetailViewModel> GetDetailOrderByOrderId(long OrderId)
        {
            return _OrderDal.GetDetailOrderByOrderId(OrderId);
        }
        
        public async Task<List<Order>> GetByOrderMergeId(long order_merge_id)
        {
            return await _OrderDal.GetByOrderMergeId(order_merge_id);
        }
        

    }
}
