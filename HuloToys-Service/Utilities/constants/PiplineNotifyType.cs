using System;
using System.ComponentModel;

namespace Utilities.Contants
{
    public enum SeenType
    {
        NOT_SEEN = 0,
        SEEN_ALL = 1, // click vao chuong
        SEEN_DETAIL = 2 // click vao item notify
    }
    public enum RoleType
    {
        SaleOnl = 1,//Sale online
        SaleKd = 2,//Sale kinh doanh phòng khách sạn
        SaleTour = 3,//Sale Tour
        TPVe = 4,//Trưởng phòng vé máy bay
        Admin = 6,//Admin
        TPKd = 7,//Trưởng phòng kinh doanh khách sạn
        TPTour = 8,//Trưởng phòng kinh doanh Tour
        GDHN = 9,//Giám đốc chi nhánh Hà Nội
        GDHPQ = 10,//Giám đốc chi nhánh Phú Quốc
        GD = 11,//Tổng giám đốc điều hành (CEO)
        KT = 12,//Kế toán
        DHP = 13,//Điều hành phòng
        DHTour = 14,//Điều hành tour
        DHVe = 15,//Điều hành vé máy bay
        DHPks = 16,//Điều hành phòng khách sạn
        TPDHKS = 18,//Trưởng phòng điều hành khách sạn
        TPDHVe = 19,//Trưởng phòng điều hành vé máy bay
        TPDHTour = 20,//Trưởng phòng điều hành Tour
        DUYETHD = 24,//Duyệt hợp đồng
        DUYET_YEU_CAU_CHI = 25,//Duyệt yêu cầu chi
    }

    public enum ModuleType
    {
        PHIEU_YEU_CAU_CHI = 0,
        DON_HANG = 1,
        HOP_DONG = 2,
        KHACH_HANG = 3,
        PHIEU_THU = 4,
        DICH_VU = 5,
        BANG_KE = 6,
        CONG_NO_DON_HANG = 7,
    }
    public enum ActionType
    {
        TAO_MOI = 0,
        VAN_CHUYEN = 1,
        THANH_CONG = 2,
        HUY = 3,
        TRA_HANG = 4,
        THANH_TOAN = 5,
        XU_LY = 6,
        //GUI_DIEU_HANH_DUYET = 7,
        //HOAN_THANH = 8,
        //TAO_YEU_CAU_CHI = 9,
        //TAO_MOI_PHIEU_THU = 10,
        //DUYET_DICH_VU = 11,
        //DA_DUYET = 12,
        //TU_CHOI_HOP_DONG = 13,
        //DUYET_YEU_CAU_CHI = 14,
        //BO_DUYET_YEU_CAU_CHI = 15,
        //TU_CHOI_DUYET_YEU_CAU_CHI = 16,
        //DA_DUYET_CONG_NO = 19,
        //TU_CHOI_DON_CONG_NO = 20,
        //DUYET_CONG_NO = 21,
    }
    public static class NotifyLabelName
    {
        public static string getModuleName(int module_type)
        {
            switch (module_type)
            {
                case (Int16)ModuleType.DON_HANG:
                    return "đơn hàng";
                case (Int16)ModuleType.PHIEU_YEU_CAU_CHI:
                    return "phiếu yêu cầu chi";
                case (Int16)ModuleType.HOP_DONG:
                    return "hợp đồng";
                case (Int16)ModuleType.PHIEU_THU:
                    return "phiếu thu";
                case (Int16)ModuleType.DICH_VU:
                    return "dịch vụ";
                case (Int16)ModuleType.CONG_NO_DON_HANG:
                    return "bảo lãnh công nợ";
                default:
                    return "n/a";
            }
        }
        public static string getActionName(int action_type)
        {
            switch (action_type)
            {
                case (Int16)ActionType.TAO_MOI:
                    return "tạo đơn";
                case (Int16)ActionType.VAN_CHUYEN:
                    return "vận chuyển";
                case (Int16)ActionType.THANH_CONG:
                    return "gửi đến";
                case (Int16)ActionType.HUY:
                    return "hủy";
                case (Int16)ActionType.TRA_HANG:
                    return "hoàn trả";
                case (Int16)ActionType.THANH_TOAN:
                    return "thanh toán";
                case (Int16)ActionType.XU_LY:
                    return "xử lý";
                //case (Int16)ActionType.NHAN_TRIEN_KHAI:
                //    return "nhận triển khai";
                //case (Int16)ActionType.HOAN_THANH:
                //    return "hoàn thành";
                //case (Int16)ActionType.TAO_YEU_CAU_CHI:
                //case (Int16)ActionType.TAO_MOI_PHIEU_THU:
                //    return "tạo mới phiếu thu";
                //case (Int16)ActionType.DUYET_DICH_VU:
                //    return "duyệt dịch vụ";
                //case (Int16)ActionType.DA_DUYET:
                //    return "đã duyệt";

                //case (Int16)ActionType.DUYET_YEU_CAU_CHI:
                //    return "duyệt yêu cầu chi";
                //case (Int16)ActionType.BO_DUYET_YEU_CAU_CHI:
                //    return "bỏ duyệt yêu cầu chi";
                //case (Int16)ActionType.TU_CHOI_DUYET_YEU_CAU_CHI:
                //    return "từ chối duyệt yêu cầu chi";
                //case (Int16)ActionType.TU_CHOI_DON_CONG_NO:
                //    return "từ chối duyệt công nợ ";
                //case (Int16)ActionType.DA_DUYET_CONG_NO:
                //    return "đã duyệt công nợ ";
                //case (Int16)ActionType.DUYET_CONG_NO:
                //    return "duyệt công nợ ";
                default:
                    return "n/a";
            }
        }
    }

}
