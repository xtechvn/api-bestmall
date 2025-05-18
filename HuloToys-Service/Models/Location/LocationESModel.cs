using Entities.Models;
using API_CORE.Controllers.Models.Models;

namespace API_CORE.Controllers.Models.Location
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
