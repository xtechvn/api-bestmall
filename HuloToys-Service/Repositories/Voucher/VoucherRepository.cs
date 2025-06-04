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
        public async Task<List<VoucherFEModel>> GetVoucherList(long account_client_id, string hotel_id)
        {
            try
            {

                DataTable data = await _VoucherDAL.GetVoucherList(account_client_id, hotel_id);
                return data.ToList<VoucherFEModel>();
              
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayByOrderId - ContractPayDAL. " + ex);
            }
            return null;
        }
    }
}
