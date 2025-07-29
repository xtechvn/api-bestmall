using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class ContractPayDAL : GenericService<ContractPay>
    {
        private static DbWorker _DbWorker;
        private static OrderDAL orderDAL;
        public ContractPayDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
            orderDAL = new OrderDAL(connection);
        }

        public ContractPay GetById(long contractPayId)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ContractPays.AsNoTracking().FirstOrDefault(x => x.PayId == contractPayId);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ContractPayDAL: " + ex);
                return null;
            }
        }

        public ContractPay GetByBillNo(string billNo)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ContractPays.AsNoTracking().FirstOrDefault(x => x.BillNo == billNo);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByBillNo - ContractPayDAL: " + ex);
                return null;
            }
        }


        public List<ContractPayDetail> GetByContractPayIds(List<int> contractPayIds)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var contractPays = _DbContext.ContractPayDetails.AsNoTracking().Where(x => contractPayIds.Contains(x.PayId)
                    && (x.ServiceId == 0 || x.ServiceId == null)).ToList();
                    if (contractPays != null)
                    {
                        return contractPays;
                    }
                }
                return new List<ContractPayDetail>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByContractPayIds - ContractPayDAL: " + ex);
                return new List<ContractPayDetail>();
            }
        }


        public List<ContractPayDetail> GetByContractPayId(int contractPayId)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var contractPays = _DbContext.ContractPayDetails.AsNoTracking().Where(x => x.PayId == contractPayId).ToList();
                    if (contractPays != null)
                    {
                        return contractPays;
                    }
                }
                return new List<ContractPayDetail>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByContractPayId - ContractPayDAL: " + ex);
                return new List<ContractPayDetail>();
            }
        }



        public int CreateContractPay(ContractPayViewModel model)
        {
            int id = 0;
            List<int> detailIds = new List<int>();
            try
            {
                SqlParameter[] objParam_contractPay = new SqlParameter[16];
                objParam_contractPay[0] = new SqlParameter("@BillNo", model.BillNo);
                if (model.ClientId == null || model.ClientId == 0)
                    objParam_contractPay[1] = new SqlParameter("@ClientId", DBNull.Value);
                else
                    objParam_contractPay[1] = new SqlParameter("@ClientId", model.ClientId);
                objParam_contractPay[2] = new SqlParameter("@Note", string.IsNullOrEmpty(model.Note) ? DBNull.Value.ToString() :
                    model.Note);
                objParam_contractPay[3] = new SqlParameter("@Amount", model.Amount);

                objParam_contractPay[4] = new SqlParameter("@Type", model.Type);
                objParam_contractPay[5] = new SqlParameter("@PayType", model.PayType);
                objParam_contractPay[6] = new SqlParameter("@BankingAccountId", model.BankingAccountId);

                objParam_contractPay[7] = new SqlParameter("@Description", string.IsNullOrEmpty(model.Description)
                    ? DBNull.Value.ToString() : model.Description);
                objParam_contractPay[8] = new SqlParameter("@AttatchmentFile", string.IsNullOrEmpty(model.AttatchmentFile)
                    ? DBNull.Value.ToString() : model.AttatchmentFile);
                objParam_contractPay[9] = new SqlParameter("@ExportDate", DateTime.Now);
                objParam_contractPay[10] = new SqlParameter("@PayStatus", (int)DepositHistoryConstant.CONTRACT_PAY_STATUS.KE_TOAN_DUYET);
                objParam_contractPay[11] = new SqlParameter("@CreatedBy", model.CreatedBy);
                objParam_contractPay[12] = new SqlParameter("@CreatedDate", DateTime.Now);
                objParam_contractPay[13] = new SqlParameter("@SupplierId", Convert.ToInt32(model.SupplierId));
                objParam_contractPay[14] = new SqlParameter("@ObjectType", Convert.ToInt32(model.ObjectType));
                objParam_contractPay[15] = new SqlParameter("@EmployeeId", Convert.ToInt32(model.EmployeeId));
                id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertContractPay, objParam_contractPay);
                if (id > 0)
                {
                    foreach (var item in model.ContractPayDetails)
                    {
                        var detailId = 0;
                        SqlParameter[] objParam_contractPayDetail = new SqlParameter[8];
                        objParam_contractPayDetail[0] = new SqlParameter("@PayId", id);
                        objParam_contractPayDetail[1] = new SqlParameter("@DataId", item.OrderId);

                        objParam_contractPayDetail[2] = new SqlParameter("@CreatedBy", model.CreatedBy);
                        objParam_contractPayDetail[3] = new SqlParameter("@Amount", item.Amount);
                        objParam_contractPayDetail[4] = new SqlParameter("@CreatedDate", DateTime.Now);
                        objParam_contractPayDetail[5] = new SqlParameter("@ServiceId", Convert.ToInt64(item.ServiceId));
                        objParam_contractPayDetail[6] = new SqlParameter("@ServiceType", Convert.ToInt32(item.ServiceType));
                        if (string.IsNullOrEmpty(item.ServiceCode))
                            objParam_contractPayDetail[7] = new SqlParameter("@ServiceCode", DBNull.Value);
                        else
                            objParam_contractPayDetail[7] = new SqlParameter("@ServiceCode", item.ServiceCode);

                        detailId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertContractPayDetail, objParam_contractPayDetail);
                        if (detailId > 0)
                            detailIds.Add(detailId);
                        if (detailId <= 0)
                        {
                            using (var _DbContext = new EntityDataContext(_connection))
                            {
                                var entity = _DbContext.ContractPays.Find(id);
                                _DbContext.ContractPays.Remove(entity);
                                foreach (var idDetail in detailIds)
                                {
                                    var detail = _DbContext.ContractPayDetails.Find(idDetail);
                                    _DbContext.ContractPayDetails.Remove(detail);
                                }
                                _DbContext.SaveChanges();
                            }
                            return -1;
                        }
                        //nếu thanh toán đủ thì cập nhật trạng thái của đơn hàng - Đơn hàng đã thanh toán
                        var status = (int)OrderStatus.PAID;
                        var orderInfo = orderDAL.GetDetailViewOrderByOrderId(item.OrderId).Result;
                        if (orderInfo != null && orderInfo.OrderStatus != (int)OrderStatus.CREATED_ORDER)
                        {
                            status = (int)orderInfo.OrderStatus;
                        }
                        switch (model.Type)
                        {
                            case (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG:
                                {
                                    if (item.Amount >= item.TotalNeedPayment)
                                    {
                                        SqlParameter[] objParam_updateFinishPayment = new SqlParameter[5];
                                        objParam_updateFinishPayment[0] = new SqlParameter("@OrderId", item.OrderId);
                                        objParam_updateFinishPayment[1] = new SqlParameter("@IsFinishPayment", true);
                                        objParam_updateFinishPayment[2] = new SqlParameter("@PaymentStatus", (int)PaymentStatus.PAID);
                                        objParam_updateFinishPayment[3] = new SqlParameter("@Status", status);
                                        objParam_updateFinishPayment[4] = new SqlParameter("@DebtStatus", (int)DepositHistoryConstant.ORDER_DEBT_STATUS.GACH_NO_DU);
                                        _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderFinishPayment, objParam_updateFinishPayment);
                                    }
                                    else 
                                    {

                                        SqlParameter[] objParam_updateFinishPayment = new SqlParameter[5];
                                        objParam_updateFinishPayment[0] = new SqlParameter("@OrderId", item.OrderId);
                                        objParam_updateFinishPayment[1] = new SqlParameter("@IsFinishPayment", false);
                                        objParam_updateFinishPayment[2] = new SqlParameter("@PaymentStatus", (int)PaymentStatus.PAID_NOT_ENOUGH);
                                        objParam_updateFinishPayment[3] = new SqlParameter("@Status", status);
                                        objParam_updateFinishPayment[4] = new SqlParameter("@DebtStatus", (int)DepositHistoryConstant.ORDER_DEBT_STATUS.GACH_NO_CHUA_DU);
                                        _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderFinishPayment, objParam_updateFinishPayment);
                                    }
                                    orderDAL.UpdateOrderStatus(item.OrderId, status, model.CreatedBy.Value, model.CreatedBy.Value).Wait();

                                }
                                break;
                            case (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY:
                                { 
                                }
                                break;
                        }


                    }

                    SqlParameter[] objParam_UpdateContractPayDebtStatus = new SqlParameter[3];
                    objParam_UpdateContractPayDebtStatus[0] = new SqlParameter("@PayId", id);
                    objParam_UpdateContractPayDebtStatus[1] = new SqlParameter("@DebtStatus", model.ContractPayDetails.Sum(n => n.Amount) >= model.Amount ?
                        (int)DepositHistoryConstant.CONTRACTPAY_DEBT_STATUS.DA_GACH_HET :
                         (int)DepositHistoryConstant.CONTRACTPAY_DEBT_STATUS.CHUA_GACH_HET);
                    objParam_UpdateContractPayDebtStatus[2] = new SqlParameter("@UpdatedBy", model.CreatedBy);
                    _DbWorker.ExecuteNonQuery(StoreProcedureConstant.Sp_UpdateDebtStatusByPayId, objParam_UpdateContractPayDebtStatus);
                }
                return id;
            }
            catch (Exception ex)
            {
                DeleteContractPayFail(id, detailIds);
                LogHelper.InsertLogTelegram("CreateContactPay - ContractPayDAL. " + ex);
                return -1;
            }
        }

        private void DeleteContractPayFail(int id, List<int> detailIds)
        {
            using (var _DbContext = new EntityDataContext(_connection))
            {
                var entity = _DbContext.ContractPays.Find(id);
                _DbContext.ContractPays.Remove(entity);
                foreach (var idDetail in detailIds)
                {
                    var detail = _DbContext.ContractPayDetails.Find(idDetail);
                    _DbContext.ContractPayDetails.Remove(detail);
                }
                _DbContext.SaveChanges();
            }
        }
        public long CountContractPayInYear()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.ContractPays.AsNoTracking().Where(x => ((DateTime)x.CreatedDate).Year == DateTime.Now.Year).Count();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountContractPayInYear - ContractPayDAL: " + ex.ToString());
                return -1;
            }
        }
        public async Task<string> getContractPayByBillNo(string bill_no)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var data = _DbContext.ContractPays.AsNoTracking().FirstOrDefault(s => s.BillNo == bill_no);
                    return data == null ? "" : data.BillNo;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getContractPayByBillNo - ContractPayDAL: " + ex);
                return "";
            }
        }



    }
}
