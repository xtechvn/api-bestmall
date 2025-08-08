using DAL.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data;
using DAL.StoreProcedure;
using HuloToys_Service.Models.Models;
using Entities.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using Utilities.Contants;

namespace DAL
{
    public class VoucherDAL : GenericService<Voucher>
    {
        private static DbWorker _DbWorker;

        public VoucherDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);

        }


        public async Task<Voucher> FindByVoucherCode(string voucherCode)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Code.ToUpper() == voucherCode.ToUpper());
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex.ToString());
                return null;
            }
        }
        public async Task<Voucher> FindByVoucheId(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Id == id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex.ToString());
                return null;
            }
        }

        public async Task<Voucher> FindByVoucherCode(string voucherCode, bool is_public = false)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Code.ToUpper() == voucherCode.ToUpper() && s.IsPublic == is_public);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex);
                return null;
            }
        }
        public async Task<string> FindByVoucherid(int voucherId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var Voucher = await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Id == voucherId);
                    return Voucher.Code;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex.ToString());
                return null;
            }
        }
        public async Task<DataTable> GetVoucherList(long account_client_id, string hotel_id)
        {
            try
            {

                SqlParameter[] input = new SqlParameter[]
                {
                    new SqlParameter("@HotelId",hotel_id==null?(object)DBNull.Value: hotel_id.ToString()),
                    new SqlParameter("@AccountClientId", account_client_id<=0?(object)DBNull.Value: account_client_id.ToString()),

                };
                return _DbWorker.GetDataTable(StoreProcedureConstant.GetListVoucher, input);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayByOrderId - ContractPayDAL. " + ex);
                return null;
            }
        }
        public async Task<List<Voucher>> GetListVoucher(List<string> vouchers)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.Where(s => vouchers.Contains(s.Code.ToUpper())).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVoucher - VoucherDAL: " + ex.ToString());
                return null;
            }
        }
    }
}
