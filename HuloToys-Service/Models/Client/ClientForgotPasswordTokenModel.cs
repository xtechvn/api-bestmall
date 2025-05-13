namespace HuloToys_Service.Models.Client
{
    public class ClientForgotPasswordTokenModel
    {
        public long account_client_id {  get; set; }
        public long client_id {  get; set; }
        public string email {  get; set; }
        public string user_name {  get; set; }
        public DateTime created_time {  get; set; }
        public DateTime exprire_time {  get; set; }
    }
}
