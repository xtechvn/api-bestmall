namespace HuloToys_Service.Models.Orders
{
    public class OrdersRefundRequestModel
    {
        public long id { get; set; }
        public string reason { get; set; }
        public string token { get; set; }

    }
    public class OrdersReceivedPackageRequestModel
    {
        public long id { get; set; }
        public string token { get; set; }

    }
}
