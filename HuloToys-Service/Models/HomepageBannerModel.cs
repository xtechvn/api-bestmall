using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Models
{
    public class HomepageBannerModel
    {
        public List<AllCode> main { get; set; }
        public List<AllCode> sub { get; set; }
        public List<AllCode> trending_main { get; set; }
        public List<AllCode> trending_sub { get; set; }
    }
}
