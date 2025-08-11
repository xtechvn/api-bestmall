using System.ComponentModel;

namespace Utilities.Contants
{
    // Trạng thái đơn
    public enum OrderStatus
    {
        /// <summary>
        /// Mặc định trạng thái đơn khi được khởi tạo
        /// </summary>
        [Description("Chờ thanh toán")]
        CREATED_ORDER = 0,

        [Description("Đã thanh toán")]
        PAID = 6,

        [Description("Đang xử lý")]
        PROCESSING = 1,


        [Description("Đang giao hàng")]
        DELIVERY = 2,

        [Description("Giao thành công")]
        FINISHED_DELIVERY = 5,

        [Description("Hoàn thành")]
        FINISHED = 3,

        [Description("Đã hủy")]
        CANCEL = 4,

        [Description("Trả hàng/ Hoàn tiền")]
        REFUND = 7,

    }

    // Trạng thái đơn
    public enum OrderĐebtStatus
    {
        /// <summary>
        /// Đã gạch nợ đủ cho đơn hàng
        /// </summary>
        [Description("Gạch nợ đủ")]
        PAID_ENOUGH = 1,
        /// <summary>
        /// Chưa đã gạch nợ đủ cho đơn hàng
        /// </summary>
        [Description("Gạch nợ chưa đủ")]
        PAID_NOT_ENOUGH = 2,

    }
    public enum ProductFlashSaleBadgeStatus
    {
        NORMAL = -1,
        FEATURED_PRODUCT = 1,
        FEATURED_BRAND = 2,
        BESTCHOICE = 3,
        FAVOURITES = 4,
        PREMIUM = 5,
        SUPERSALE = 6,
        MOSTLY_SOLD = 7

    }
}
