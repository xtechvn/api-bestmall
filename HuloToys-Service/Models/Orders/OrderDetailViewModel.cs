using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Models.Orders
{
    public class OrderDetailViewModel : Order
    {
        public string Label { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public string PermisionTypeName { get; set; }
        public string FullName { get; set; }
        public string UserUpdateFullName { get; set; }
        public double AmountPay { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string TaxNo { get; set; }
        public string BusinessAddress { get; set; }
        public double Id { get; set; }

        public int ContactClientId { get; set; }
        public int SalerId { get; set; }
        public long? AccountClientId { get; set; }
        public string BankCode { get; set; }
        public short? BranchCode { get; set; }
        public byte? ServiceType { get; set; }
        public short? SystemType { get; set; }
        public string OrderStatusName { get; set; }
        public string PaymentStatusName { get; set; }
        public string SystemTypeName { get; set; }
        public string PaymentTypeName { get; set; }
        public DateTime PaymentDate { get; set; }
        public string SalerGroupId { get; set; }
        public string BranchCodeName { get; set; }
        public string code { get; set; }

        public long Refund { get; set; }
        public string WardName { get; set; }
        public string DistrictName { get; set; }
        public string ProvinceName { get; set; }

        public string ShippingTypeName { get; set; }
        public string CarrierTypeName { get; set; }
        public string PhoneOrder { get; set; }

    }

}
