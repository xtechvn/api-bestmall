using Entities.Models;
using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Models.Location
{
    public class ProvinceESModel:Province
    {
        public string id { get; set; }
    }
    public class DistrictESModel : District
    {
        public string id { get; set; }
    }
    public class WardESModel : Ward
    {
        public string id { get; set; }
    }
}
