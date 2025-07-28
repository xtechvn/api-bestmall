namespace HuloToys_Service.Models.Orders
{
    public class OrdersGeneralRequestModel
    {
        public string id { get; set; }

    }
    public class OrdersVNPAYRequestModel: OrdersGeneralRequestModel
    {
        public string client_ip { get; set; }
        public string country { get; set; }

    }
    public class OrdersVNPAYValidateRequestModel 
    {
        public string response_from_vnpay { get; set; }

    }
}
