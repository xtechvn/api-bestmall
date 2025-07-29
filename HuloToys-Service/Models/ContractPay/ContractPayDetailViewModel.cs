using Entities.Models;
using HuloToys_Service.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Funding
{
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
        public ContractPayDetail ContractPayDetail { get; set; }
        public ContractPay ContractPay { get; set; }
        //public OrderViewModel Order { get; set; }
        //public DepositFunding DepositHistory { get; set; }
        public long ContractPayId { get; set; }
        public long ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public int? ServiceType { get; set; }
    }
     
}
