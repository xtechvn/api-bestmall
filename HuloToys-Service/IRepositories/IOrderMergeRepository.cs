using DAL;
using Entities.ConfigModels;
using HuloToys_Service.Models.Models;
using Microsoft.Extensions.Options;
using System.Data;

namespace HuloToys_Service.IRepositories
{
    public interface IOrderMergeRepository
    {
        public  Task<long> InsertOrderMerge(OrderMerge model);


        public  Task<long> UpdateOrderMerge(OrderMerge model);


        public  Task<DataTable> GetOrderMergePaging(int pageIndex, int pageSize, string keyword);
        public OrderMerge GetById(long OrderId);

    }
}
