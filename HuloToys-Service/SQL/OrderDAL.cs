using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class OrderDAL : GenericService<Order>
    {
        private static DbWorker _DbWorker;
        public OrderDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);

        }

        private DateTime CheckDate(string dateTime)
        {
            DateTime _date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateTime))
            {
                _date = DateTime.ParseExact(dateTime, "d/M/yyyy", CultureInfo.InvariantCulture);
            }

            return _date != DateTime.MinValue ? _date : DateTime.MinValue;
        }
        public Order GetByOrderId(long OrderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Orders.AsNoTracking().FirstOrDefault(s => s.OrderId == OrderId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderId - OrderDal: " + ex);
                return null;
            }
        }

        public List<Order> GetByOrderIds(List<long> orderIds)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Orders.AsNoTracking().Where(s => orderIds.Contains(s.OrderId)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderIds - OrderDal: " + ex);
                return new List<Order>();
            }
        }
        public List<Order> GetByOrderNos(List<string> orderNos)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Orders.AsNoTracking().Where(s => orderNos.Contains(s.OrderNo)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderNos - OrderDal: " + ex);
                return new List<Order>();
            }
        }
        public List<Order> GetByClientId(long Client_Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Orders.AsNoTracking().Where(s => s.ClientId == Client_Id).OrderByDescending(s => s.CreatedDate).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - OrderDal: " + ex);
                return null;
            }
        }
        public async Task<long> CreateOrder(Order order)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (order.OrderNo == null || order.OrderNo.Trim() == "")
                    {
                        return -1;

                    }
                    else
                    {
                        _DbContext.Orders.Add(order);
                        await _DbContext.SaveChangesAsync();
                        return order.OrderId;
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateOrder - OrderDal: " + ex);
                return -2;
            }
        }
        public long CountOrderInYear()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.Orders.AsNoTracking().Where(x => x.CreatedDate.Year == DateTime.Now.Year).Count();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountOrderInYear - OrderDAL: " + ex.ToString());
                return -1;
            }
        }
        public static async Task<string> getOrderNoByOrderNo(string order_no)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var data = _DbContext.Orders.AsNoTracking().FirstOrDefault(s => s.OrderNo == order_no);
                    return data == null ? "" : data.OrderNo;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderNoByOrderNo - OrderDAL: " + ex);
                return "";
            }
        }
        public Order GetByOrderNo(string orderNo)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Orders.AsNoTracking().FirstOrDefault(s => s.OrderNo == orderNo);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderNo - OrderDal: " + ex);
                return null;
            }
        }
        public async Task<long> UpdataOrder(Order order)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Orders.Update(order);
                    await _DbContext.SaveChangesAsync();
                    var OrderId = order.OrderId;
                    return OrderId;

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdataOrder - OrderDal: " + ex.ToString());
                return 0;
            }
        }

        public DataTable GetListOrderByClientId(long clienId, string proc, int status = 0)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@ClientId", clienId);
                objParam[1] = new SqlParameter("@IsFinishPayment", DBNull.Value);
                if (status == 0)
                    objParam[2] = new SqlParameter("@OrderStatus", DBNull.Value);
                else
                    objParam[2] = new SqlParameter("@OrderStatus", status);

                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListOrderByClientId - OrderDal: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetDetailOrderServiceByOrderId(int OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailOrderServiceByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderServiceByOrderId - OrderDal: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetDetailOrderByOrderId(int OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailOrderByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SP_GetDetailOrderByOrderId - OrderDal: " + ex);
            }
            return null;
        }


        public async Task<int> UpdateOrderStatus(long OrderId, long Status, long UpdatedBy, long UserVerify)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                objParam[1] = new SqlParameter("@Status", Status);
                objParam[2] = new SqlParameter("@UpdatedBy", UpdatedBy);
                objParam[3] = UserVerify == 0 ? new SqlParameter("@UserVerify", DBNull.Value) : new SqlParameter("@UserVerify", UserVerify);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderStatus, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderServiceByOrderId - OrderDal: " + ex);
            }
            return 0;
        }

        public async Task<DataTable> GetAllServiceByOrderId(long OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAllServiceByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllServiceByOrderId - OrderDal: " + ex);
            }
            return null;
        }

    }
}
