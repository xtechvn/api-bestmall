using ENTITIES.ViewModels.Voucher;
using HuloToys_Service.Models.Models;

namespace REPOSITORIES.IRepositories
{
    public  interface IVoucherRepository
    {
        Task<Voucher> getDetailVoucher(string voucher_name);
        Task<Voucher> getDetailVoucherbyId(long Id);
        Task<List<Voucher>> GetListVoucher(List<string> vouchers);
        Task<List<VoucherFEModel>> GetVoucherList(string keyword, int status = 1, int page_index = 1, int page_size = 10, long? client_id = null, int limit_use = 0);


    }
}
