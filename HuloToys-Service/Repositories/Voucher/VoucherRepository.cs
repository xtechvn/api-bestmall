using DAL;
using Entities.ConfigModels;
using ENTITIES.ViewModels.Voucher;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories;
using System.Data;
using Utilities;

namespace REPOSITORIES.Repositories
{
    public class VoucherRepository: IVoucherRepository
    {
        private readonly VoucherDAL _VoucherDAL;

        public VoucherRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var _StrConnection = dataBaseConfig.Value.SqlServer.ConnectionString;
            _VoucherDAL = new VoucherDAL(_StrConnection);
        }


        public async  Task<Voucher> getDetailVoucher(string voucher_name)
        {
            try
            {
                return await _VoucherDAL.FindByVoucherCode(voucher_name);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[Repository] getDetailVoucher in VoucherRepository" + ex);
                return null;
            }
        }
        public async Task<Voucher> getDetailVoucherbyId(long Id )
        {
            try
            {
                return await _VoucherDAL.FindByVoucheId(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getDetailVoucherbyId - VoucherRepository" + ex);
                return null;
            }
        }
        public async Task<List<VoucherFEModel>> GetVoucherList(string keyword, int status = 1, int page_index = 1, int page_size = 10, long? client_id = null)
        {
            try
            {
                List<VoucherFEModel> list=new List<VoucherFEModel>();
                DataTable data = await _VoucherDAL.GetVoucherList(keyword, status,page_index,page_size, client_id);
                if(data!=null && data.Rows.Count > 0)
                {
                    //    list = (from row in data.AsEnumerable()
                    //            select new VoucherFEModel
                    //            {
                    //                Id = Convert.ToInt32(row["Id"]),
                    //                code = row["code"].ToString(),
                    //                cdate = row["cdate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["cdate"]) : null,
                    //                udate = row["udate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["udate"]) : null,
                    //                eDate = row["eDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["eDate"]) : null,
                    //                limitUse = Convert.ToInt32(row["limitUse"]),
                    //                price_sales = row["price_sales"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["price_sales"]) : null,
                    //                unit = row["unit"].ToString(),
                    //                rule_type = row["rule_type"] != DBNull.Value ? (int?)Convert.ToInt32(row["rule_type"]) : null,
                    //                group_user_priority = row["group_user_priority"] != DBNull.Value ? row["group_user_priority"].ToString() : null,
                    //                IsPublic = row["is_public"] != DBNull.Value ? (bool?)Convert.ToBoolean(row["is_public"]) : null,
                    //                description = row["description"] != DBNull.Value ? row["description"].ToString() : null,
                    //                is_limit_voucher = row["is_limit_voucher"] != DBNull.Value ? (bool?)Convert.ToBoolean(row["is_limit_voucher"]) : null,
                    //                limit_total_discount = row["limit_total_discount"] != DBNull.Value ? (double?)Convert.ToDouble(row["limit_total_discount"]) : null,
                    //                store_apply = row["store_apply"] != DBNull.Value ? row["store_apply"].ToString() : null,
                    //                is_max_price_product = row["is_max_price_product"] != DBNull.Value ? (bool?)Convert.ToBoolean(row["is_max_price_product"]) : null,
                    //                MinTotalAmount = row["min_total_amount"] != DBNull.Value ? (double?)Convert.ToDouble(row["min_total_amount"]) : null,
                    //                campaign_id = row["campaign_id"] != DBNull.Value ? (int?)Convert.ToInt32(row["campaign_id"]) : null,
                    //                TotalRow = Convert.ToInt32(row["TotalRow"])
                    //            }).ToList();
                    list = data.ToList<VoucherFEModel>();
                }
                
                return list;
              
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayByOrderId - ContractPayDAL. " + ex);
            }
            return null;
        }
        public async Task<List<Voucher>> GetListVoucher(List<string> vouchers)
        {
            try
            {
                return await _VoucherDAL.GetListVoucher(vouchers);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVoucher - ContractPayDAL. " + ex);
            }
            return null;
        }
    }
}
