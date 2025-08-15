using DAL;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using HuloToys_Service.IRepositories;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Data;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq.Expressions;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class OrderMergeRepository : IOrderMergeRepository
    {
        private readonly OrderMergeDAL _orderMergeDAL;


        public OrderMergeRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _orderMergeDAL = new OrderMergeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);

        }

        public async Task<long> InsertOrderMerge(OrderMerge model)
        {
           return await _orderMergeDAL.InsertOrderMerge(model);
        }


        public async Task<long> UpdateOrderMerge(OrderMerge model)
        {
            return await _orderMergeDAL.UpdateOrderMerge(model);

        }


        public async Task<DataTable> GetOrderMergePaging(int pageIndex, int pageSize, string keyword)
        {
            return await _orderMergeDAL.GetOrderMergePaging(pageIndex,pageSize,keyword);

        }
        public OrderMerge GetById(long OrderId)
        {
            return  _orderMergeDAL.GetById(OrderId);

        }
    }
}
