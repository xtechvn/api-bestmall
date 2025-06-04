using ENTITIES.ViewModels.Voucher;
using HuloToys_Service.Models.Models;

namespace REPOSITORIES.IRepositories
{
    public  interface IVoucherRepository
    {
        Task<Voucher> getDetailVoucher(string voucher_name);
        Task<Voucher> getDetailVoucherbyId(long Id);
        Task<List<VoucherFEModel>> GetVoucherList(long account_client_id, string hotel_id);
    }
}
