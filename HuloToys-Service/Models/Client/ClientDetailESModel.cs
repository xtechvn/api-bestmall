namespace HuloToys_Service.Models.Client
{
    public class ClientDetailESModel: ClientAddressGeneralRequestModel
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string ClientName { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string Phone { get; set; }
    }
}
