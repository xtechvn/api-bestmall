namespace HuloToys_Service.Models.Client
{
    public class ClientLoginResponseModel
    {
        public string  token { get; set; }
        public string user_name { get; set; }
        public string name { get; set; }
        public string ip { get; set; }
        public DateTime time_expire { get; set; }

    }
}
