using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.Orders;
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

        public async Task<OrderDetailViewModel> GetDetailViewOrderByOrderId(long OrderId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                DataTable dt = _DbWorker.GetDataTable(ProcedureConstants.SP_GetDetailOrderByOrderId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<OrderDetailViewModel>();
                    return data[0];
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderByOrderId - OrderDal: " + ex);
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
        public async Task<long> UpdateOrder(Order model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[] {
                   new SqlParameter("@OrderId", model.OrderId),
                    new SqlParameter("@ClientId", model.ClientId<=0?(object)DBNull.Value:model.ClientId),
                    new SqlParameter("@OrderNo", model.OrderNo??(object)DBNull.Value),
                    new SqlParameter("@Price", model.Price ??(object) DBNull.Value),
                    new SqlParameter("@Profit", model.Profit ??(object) DBNull.Value),
                    new SqlParameter("@Discount", model.Discount??(object)DBNull.Value),
                    new SqlParameter("@Amount", model.Amount??(object)DBNull.Value),
                    new SqlParameter("@Status", model.OrderStatus<=0?(object)DBNull.Value:model.OrderStatus),
                    new SqlParameter("@PaymentType", model.PaymentType <= 0 ?(object) DBNull.Value : model.PaymentType),
                    new SqlParameter("@PaymentStatus", model.PaymentStatus <= 0 ?(object) DBNull.Value : model.PaymentStatus),
                    new SqlParameter("@UtmSource", model.UtmSource ??(object) DBNull.Value),
                    new SqlParameter("@UtmMedium", model.UtmMedium??(object)DBNull.Value),
                    new SqlParameter("@Note", model.Note??(object)DBNull.Value),
                    new SqlParameter("@VoucherId", model.VoucherId??(object)DBNull.Value),
                    new SqlParameter("@IsDelete", model.IsDelete??(object)DBNull.Value),
                    new SqlParameter("@UserId", model.UserId <= 0 ?(object) DBNull.Value : model.UserId),
                    new SqlParameter("@UserGroupIds", model.UserGroupIds??(object)DBNull.Value),
                    new SqlParameter("@UserUpdateId", model.UserUpdateId??(object)DBNull.Value),
                    new SqlParameter("@ProvinceId", model.ProvinceId??(object)DBNull.Value),
                    new SqlParameter("@DistrictId", model.DistrictId??(object)DBNull.Value),
                    new SqlParameter("@WardId", model.WardId??(object)DBNull.Value),
                    new SqlParameter("@Address", model.Address??(object)DBNull.Value),
                    new SqlParameter("@ShippingFee", model.ShippingFee??(object)DBNull.Value),
                    new SqlParameter("@CarrierId", model.CarrierId??(object)DBNull.Value),
                    new SqlParameter("@ShippingType", model.ShippingType??(object)DBNull.Value),
                    new SqlParameter("@ShippingCode", model.ShippingCode ??(object) DBNull.Value),
                    new SqlParameter("@ShippingStatus", model.ShippingStatus ??(object) DBNull.Value),
                    new SqlParameter("@PackageWeight", model.PackageWeight??(object)DBNull.Value),
                    new SqlParameter("@Phone", model.Phone??(object)DBNull.Value),
                    new SqlParameter("@RefundStatus", model.RefundStatus??(object)DBNull.Value),
                    new SqlParameter("@RefundReason", model.RefundReason??(object)DBNull.Value),
                    new SqlParameter("@RefundDate", model.RefundDate??(object)DBNull.Value),
                    new SqlParameter("@ShippingToken", model.ShippingToken??(object)DBNull.Value),
                    new SqlParameter("@ShippingTypeCode", model.ShippingTypeCode??(object)DBNull.Value),
                    new SqlParameter("@SupplierId", model.SupplierId??(object)DBNull.Value),
                    new SqlParameter("@OrderMergeId", model.OrderMergeId??(object)DBNull.Value),
                    new SqlParameter("@ShippingFee", model.ShippingFee??(object)DBNull.Value),

                };

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.Sp_UpdateOrder, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderSaler - OrderDal: " + ex);
                return -2;
            }
        }
        public async Task<long> UpdateOrderStatus(Order model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[] {
                   new SqlParameter("@OrderId", model.OrderId),
                    new SqlParameter("@Status", model.OrderStatus),
                    new SqlParameter("@UpdatedBy", model.UserUpdateId??(object)DBNull.Value),
                    new SqlParameter("@UserVerify", model.UserUpdateId ??(object) DBNull.Value),
                    new SqlParameter("@RefundStatus", model.RefundStatus ??(object) DBNull.Value),
                    new SqlParameter("@RefundReason", model.RefundReason ??(object) DBNull.Value),

                    

                };

                return _DbWorker.ExecuteNonQuery("SP_UpdateOrderStatus", objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderStatus - OrderDal: " + ex);
                return -2;
            }
        }

        public async Task<OrderDetailViewModel> GetDetailOrderByOrderId(long OrderId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                DataTable dt = _DbWorker.GetDataTable(ProcedureConstants.SP_GetDetailOrderByOrderId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<OrderDetailViewModel>();
                    return data[0];
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderByOrderId - OrderDal: " + ex);
            }
            return null;
        }

    }
}
