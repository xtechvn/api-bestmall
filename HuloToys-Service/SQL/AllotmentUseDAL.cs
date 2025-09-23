using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using Nest;
using System.Data;
using System.Drawing.Printing;
using Utilities;

namespace DAL
{
    public class AllotmentUseDAL : GenericService<AllotmentUse>
    {
        private static DbWorker _DbWorker;

        public AllotmentUseDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public int Insert(AllotmentUse model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@DataId", model.DataId),
                    new SqlParameter("@CreateDate", model.CreateDate==null||model.CreateDate<=DateTime.MinValue ? DBNull.Value:model.CreateDate),
                    new SqlParameter("@AmountUse", model.AmountUse),
                    new SqlParameter("@AllomentFundId", model.AllotmentFundId),
                    new SqlParameter("@AccountClientId", model.AccountClientId),
                    new SqlParameter("@ServiceType", model.ServiceType),
                    new SqlParameter("@ClientId", model.ClientId),
                    new SqlParameter("@PaymentStatus", model.PaymentStatus),
                     new SqlParameter("@Description", (object?)model.Description ?? DBNull.Value),
                    new SqlParameter("@TotalAmoutCalculate", (object?)model.TotalAmoutCalculate ?? DBNull.Value),
                    new SqlParameter("@PaymentFromDate", (object?)model.PaymentFromDate ?? DBNull.Value),
                    new SqlParameter("@PaymentToDate", (object?)model.PaymentToDate ?? DBNull.Value),
                    new SqlParameter("@BankId", model.BankId ?? (object?)DBNull.Value),
                    new SqlParameter("@AccountNumber", model.AccountNumber?? (object?)DBNull.Value),
                    new SqlParameter("@AccountName", (object?)model.AccountName ?? DBNull.Value),
                    new SqlParameter("@Branch", (object?)model.Branch ?? DBNull.Value),
                };

                return _DbWorker.ExecuteNonQuery("SP_InsertAllotmentUse", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - AllotmentUseDAL: " + ex);
                return -1;
            }
        }

        public int Update(AllotmentUse model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@DataId", model.DataId),
                    new SqlParameter("@AmountUse", model.AmountUse),
                    new SqlParameter("@AllomentFundId", model.AllotmentFundId),
                    new SqlParameter("@AccountClientId", model.AccountClientId),
                    new SqlParameter("@ServiceType", model.ServiceType),
                    new SqlParameter("@ClientId", model.ClientId),
                    new SqlParameter("@CreateDate", model.CreateDate==null||model.CreateDate<=DateTime.MinValue ? DBNull.Value:model.CreateDate),
                    new SqlParameter("@PaymentStatus", model.PaymentStatus),
                     new SqlParameter("@Description", (object?)model.Description ?? DBNull.Value),
                    new SqlParameter("@TotalAmoutCalculate", (object?)model.TotalAmoutCalculate ?? DBNull.Value),
                    new SqlParameter("@PaymentFromDate", (object?)model.PaymentFromDate ?? DBNull.Value),
                    new SqlParameter("@PaymentToDate", (object?)model.PaymentToDate ?? DBNull.Value),
                    new SqlParameter("@BankId", model.BankId ?? (object?)DBNull.Value),
                    new SqlParameter("@AccountNumber", model.AccountNumber?? (object?)DBNull.Value),
                    new SqlParameter("@AccountName", (object?)model.AccountName ?? DBNull.Value),
                    new SqlParameter("@Branch", (object?)model.Branch ?? DBNull.Value),
                };

                return _DbWorker.ExecuteNonQuery("SP_UpdateAllotmentUse", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - AllotmentUseDAL: " + ex);
                return -1;
            }
        }

        public GenericViewModel<AllotmentUse> GetByAccountClientId(long accountClientId, int pageIndex=1, int pageSize=10)
        {
            GenericViewModel<AllotmentUse> result = new GenericViewModel<AllotmentUse>();
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@AccountClientId", accountClientId),
                    new SqlParameter("@page_index", pageIndex),
                    new SqlParameter("@page_size", pageSize),
                    new SqlParameter("@service_type", 1),

                };
                LogHelper.InsertLogTelegram("AffiliatePaymentListing GetByAccountClientId [" + accountClientId + "][" + pageIndex + "][" + pageSize + "][" + 1 + "]");
                var dt= _DbWorker.GetDataTable("SP_GetAllotmentUseByAccountClientId", objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    result.ListData = dt.ToList<AllotmentUse>();
                    result.CurrentPage = pageIndex;
                    result.PageSize = pageSize;
                    result.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    result.TotalPage = (int)Math.Ceiling((double)result.TotalRecord / pageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByAccountClientId - AllotmentUseDAL: " + ex);
            }
            return result;

        }
    }
}
