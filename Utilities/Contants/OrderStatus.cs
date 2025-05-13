using System.ComponentModel;

namespace Utilities.Contants
{
    // Trạng thái đơn
    public enum OrderStatus
    {
        /// <summary>
        /// Mặc định trạng thái đơn khi được khởi tạo
        /// </summary>
        [Description("Tạo mới")]
        CREATED_ORDER = 0,
        /// <summary>
        /// Sau khi tạo đơn thành công, Sale sẽ vào nhận chăm sóc đơn tại nút "Nhận xử lý" trong chi tiết đơn hàng  Sẽ chuyển đơn hàng status này
        /// </summary>
        [Description("Nhận triển khai")]
        CONFIRMED_SALE = 1,

        /// <summary>
        /// Sau khi tạo phiếu thu đủ số tiền cho đơn hàng có Status =1, Sẽ chuyển đơn hàng Status này
        /// </summary>
        [Description("Chờ điều hành duyệt")]
        WAITING_FOR_OPERATOR = 2,

        /// <summary>
        /// Điều hành từ chối vì bất kỳ lý do gì Sẽ chuyển đơn hàng Stauts này
        /// </summary>
        [Description("Điều hành từ chối")]
        OPERATOR_DECLINE = 3,

        /// <summary>
        /// Sau khi điều hành nhận xử lý nghiệp vụ,trả code, tạo yêu cầu chi ... Khi nhấn vào nút quyết toán, Sẽ chuyển đơn hàng Stauts này
        /// </summary>
        [Description("Chờ kế toán duyệt")]
        WAITING_FOR_ACCOUNTANT = 4,
        /// <summary>
        /// Kế toán từ chối vì bất kỳ lý do gì, thì đổi Status này
        /// </summary>
        [Description("Kế toán từ chối")]
        ACCOUNTANT_DECLINE = 5,
        /// <summary>
        /// Kế toán tiến hành nghiệp vụ, quyết toán thành công , thu hồi công nợ đủ số tiền ... Sẽ chuyển đơn hàng về status này
        /// </summary>
        [Description("Hoàn thành")]
        FINISHED = 6,
        /// <summary>
        /// Với trường hợp đơn bị hủy / Nhấn vào nút "Hủy đơn hàng" tại phần chi tiết đơn , Sẽ chuyển đơn hàng về status này
        /// </summary>
        [Description("Hủy")]
        CANCEL = 7,
       
        [Description("Đơn rác")]
        DonRac = 8,
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
}
