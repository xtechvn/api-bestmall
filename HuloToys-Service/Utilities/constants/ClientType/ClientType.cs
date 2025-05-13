namespace HuloToys_Service.Utilities.constants.ClientType
{
    public enum ClientType
    {
        KL = 0,
        DOI_TAC_CL = 1,
        CTV = 2,
        DAILYC1 = 3,
        DOANH_NGHIEP = 4,

    }
    public enum ClientTypes
    {
        ALL = -1,
        AGENT = 1, // Đại lý
        TIER_1_AGENT = 2, // Đối tác chiến lược
        TIER_2_AGENT = 3, // Đại lý cấp 2
        TIER_3_AGENT = 4,// Đại lý cấp 3
        CUSTOMER = 5, //Khách lẻ
        SALE = 6, // nv kinh doanh
        ENTERPRISE = 7, // Doanh nghiệp
        COLLABORATORS = 8// Cộng tác viên

    }
    public enum ClientProfileType
    {
        BOOKER = 0, //Sale booking, chỉ có trong trường hợp book hộ
        CONTACT_CLIENT = 1, // Thông tin thành viên chính , sử dụng để liên hệ
        GUEST_ADULT = 2, // Thông tin thành viên trong đoàn là người lớn
        GUEST_CHILD = 2, // Thông tin thành viên trong đoàn là người lớn
        GUEST_INFANT = 2, // Thông tin thành viên trong đoàn là trẻ sơ sinh
    }

}
