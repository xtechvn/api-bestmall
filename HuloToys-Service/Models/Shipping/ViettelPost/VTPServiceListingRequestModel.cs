using HuloToys_Service.Controllers.Shipping.Business;

namespace HuloToys_Service.Models.Shipping.ViettelPost
{
    public class VTPServiceListingRequestModel
    {
        public List<VTPServiceListingRequestCart> carts { get; set; }
        public int receiver_provinces_id { get; set; }
        public int receiver_district_id { get; set; }
    }
    public class VTPServiceListingRequestCart
    {
        public string _id { get; set; }
        public int quanity { get; set; }
    }

    public class VTPServiceListingResponseModel
    {
        public int supplier_id { get; set; }
        public string supplier_name { get; set; }
        public List<string> cart_ids { get; set; }
        public  List<VTPServiceListingResponseMethod> services { get; set; }
    }
    public class VTPServiceListingResponseMethod { 
        public string service_code { get; set; }
        public string name { get; set; }
        public string time { get; set; }
        public long total_amount { get; set; }
    
    
    }
}
