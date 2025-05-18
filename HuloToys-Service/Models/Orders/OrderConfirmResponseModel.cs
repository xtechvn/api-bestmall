namespace API_CORE.Controllers.Models.Orders
{
    public class OrderConfirmResponseModel
    {
        public string id { get; set; }
        public string order_no { get; set; }
        public bool pushed { get; set; }

    }
}
