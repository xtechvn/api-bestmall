using HuloToys_Service.Models.Client;

namespace HuloToys_Service.Models.Address
{
    public class AddressViewModel : ClientAddressGeneralRequestModel
    {
        public long Id { get; set; }
        public long AccountClientId { get; set; }
        public long ClientId { get; set; }
        public string ReceiverName { get; set; }
        public string Phone { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public int? Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
