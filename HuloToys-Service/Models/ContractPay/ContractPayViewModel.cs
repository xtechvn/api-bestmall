using Entities.ViewModels.Funding;
using HuloToys_Service.Models.Models;

namespace Entities.ViewModels
{
    public class ContractPayViewModel : ContractPay
    {

        public List<ContractPayDetailViewModel> ContractPayDetails { get; set; } // Assuming contractPayDetails is an array/list of objects

    }
    public class ContractPayDetailViewModel
    {
        public int Id { get; set; }
        public int PayId { get; set; }
        public int OrderId { get; set; }
        public double AmountOrder { get; set; }
        public double Payment { get; set; }
        public int PayDetailId { get; set; }
        public double Amount { get; set; }
        public double TotalNeedPayment { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string BankName { get; set; }
        public string BankAccount { get; set; }
        public string PaymentTypeStr { get; set; }
        public string OrderCode { get; set; }
        public long ContractPayId { get; set; }
        public long ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public int? ServiceType { get; set; }
    }
    public class ContractPayViewModelBK : ContractPay
    {
        public string ClientName { get; set; }
        public string ContractPayType { get; set; }
        public string PayDetail { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string TypeStr { get; set; }
        public string PayTypeStr { get; set; }
        public string CreatedByName { get; set; }
        public string BankName { get; set; }
        public string BankAccount { get; set; }
        public double TotalDeposit { get; set; }
        public long TotalRow { get; set; }
        public string PayDetailId { get; set; }
        public IFormFile imagefile { get; set; }
    }
}
