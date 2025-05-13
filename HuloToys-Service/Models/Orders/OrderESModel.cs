using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuloToys_Service.Models.Orders
{
    public class OrderESModel
    {
        [PropertyName("Id")]

        public long Id { get; set; }
        [PropertyName("OrderId")]

        public long OrderId { get; set; }
        [PropertyName("ClientId")]

        public long ClientId { get; set; }
        [PropertyName("OrderNo")]

        public string OrderNo { get; set; }
        [PropertyName("CreatedDate")]

        public DateTime CreatedDate { get; set; }
        [PropertyName("CreatedBy")]

        public int? CreatedBy { get; set; }
        [PropertyName("UpdateLast")]

        public DateTime? UpdateLast { get; set; }
        [PropertyName("UserUpdateId")]

        public int? UserUpdateId { get; set; }
        [PropertyName("Price")]

        public double? Price { get; set; }
        [PropertyName("Profit")]

        public double? Profit { get; set; }
        [PropertyName("Discount")]

        public double? Discount { get; set; }
        [PropertyName("Amount")]

        public double? Amount { get; set; }
        [PropertyName("OrderStatus")]

        public int OrderStatus { get; set; }
        [PropertyName("PaymentType")]

        public short PaymentType { get; set; }
        [PropertyName("PaymentStatus")]

        public int PaymentStatus { get; set; }
        [PropertyName("UtmSource")]

        public string UtmSource { get; set; }
        [PropertyName("UtmMedium")]

        public string UtmMedium { get; set; }

        [PropertyName("Note")]


        public string Note { get; set; }
        [PropertyName("VoucherId")]

        public int? VoucherId { get; set; }
        [PropertyName("IsDelete")]

        public int? IsDelete { get; set; }
        [PropertyName("UserId")]

        public int? UserId { get; set; }
        [PropertyName("UserGroupIds")]

        public string UserGroupIds { get; set; }
        [PropertyName("ReceiverName")]

        public string ReceiverName { get; set; }
        [PropertyName("Phone")]

        public string Phone { get; set; }
        [PropertyName("ProvinceId")]

        public int? ProvinceId { get; set; }
        [PropertyName("DistrictId")]

        public int? DistrictId { get; set; }
        [PropertyName("WardId")]

        public int? WardId { get; set; }
        [PropertyName("Address")]

        public string Address { get; set; }
        [PropertyName("ShippingFee")]

        public double? ShippingFee { get; set; }
        [PropertyName("CarrierId")]

        public int? CarrierId { get; set; }
        [PropertyName("ShippingType")]

        public int? ShippingType { get; set; }
        [PropertyName("ShippingCode")]

        public string ShippingCode { get; set; }
        [PropertyName("ShippingStatus")]

        public int? ShippingStatus { get; set; }
        [PropertyName("PackageWeight")]

        public double? PackageWeight { get; set; }
    }
}
