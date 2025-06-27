namespace HuloToys_Service.Models.Orders
{
    public class OrdersUpdateAddressRequestModel
    {
        public string province_id { get; set; }
        public string district_id { get; set; }
        public string ward_id { get; set; }
        public string token { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public long order_id { get; set; }


    }
}
