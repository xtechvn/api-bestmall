using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Contants
{
    public static class ResponseMessages
    {
        public const string FunctionExcutionFailed = "Có lỗi xảy ra trong quá trình xử lý, vui lòng liên hệ bộ phận chăm sóc để được hỗ trợ";
        public const string DataInvalid = "Dữ liệu không chính xác, vui lòng kiểm tra lại / liên hệ bộ phận chăm sóc để được hỗ trợ";
        public const string DataOutputInvalid = "Không tìm thấy kết quả, vui lòng kiểm tra lại";
        public const string Success = "Success";
        public const string ClientEmailExists = "Địa chỉ Email này đã tồn tại trong hệ thống, vui lòng đăng ký sử dụng Email khác";

    }
}
