namespace HuloToys_Service.Models.Orders
{
    public class OrderDetailViewModel
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public string Label { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public double Amount { get; set; }
        public int Status { get; set; }
        public string PermisionTypeName { get; set; }
        public string FullName { get; set; }
        public string UserUpdateFullName { get; set; }
        public double AmountPay { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string TaxNo { get; set; }
        public string BusinessAddress { get; set; }
        public double Id { get; set; }
        public long ClientId { get; set; }
        public long CreatedBy { get; set; }
        public int ContactClientId { get; set; }
        public int SalerId { get; set; }
        public DateTime? UpdateLast { get; set; }
        public long? UserUpdateId { get; set; }
        public long? AccountClientId { get; set; }
        public int? OrderStatus { get; set; }
        public string BankCode { get; set; }
        public string Note { get; set; }
        public short? BranchCode { get; set; }
        public int? PaymentStatus { get; set; }
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
        public long Discount { get; set; }
        public long Profit { get; set; }
        public long Refund { get; set; }
        public string Address { get; set; }
        public string WardName { get; set; }
        public string DistrictName { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string ShippingCode { get; set; }
        public double ShippingFee { get; set; }
        public string ShippingTypeName { get; set; }
        public string CarrierTypeName { get; set; }
        public string ReceiverName { get; set; }
        public string PhoneOrder { get; set; }
        public int? RefundStatus { get; set; }
        public string RefundReason { get; set; }
        public DateTime? RefundDate { get; set; }
    }

}
