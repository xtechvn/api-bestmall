using HuloToys_Service.Utilities.constants.NinjaVan;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Linq;
using static HuloToys_Service.Utilities.constants.NinjaVan.NinjaVanShippingFee;

namespace HuloToys_Service.Controllers.Shipping.Business
{
    public class NinjaVanService
    {
        private readonly IConfiguration _configuration;
        private RestClient ninja_van_client;

        public NinjaVanService(IConfiguration configuration)
        {
            _configuration = configuration;
            var options = new RestClientOptions(_configuration["NinjaVan:APIs:Domain"]);
            ninja_van_client = new RestClient(options);
        }
        public int CaclucateShippingFee(int province_id, int weight_in_grams)
        {

            try
            {
                if (province_id <= 0 || weight_in_grams<=0) return -1;
                var area = GetShippingArea(province_id);
                if (area < -1) return -1;
                int shipping_fee = 0;
                var base_fee = NinjaVanShippingFee.FEE.FirstOrDefault(x => x.area == area && weight_in_grams > x.min_weight && weight_in_grams <= x.max_weight);
                if (weight_in_grams >= NinjaVanShippingFee.MAX_STANDARD_WEIGHT)
                {
                    var additional_weight = weight_in_grams - NinjaVanShippingFee.MAX_STANDARD_WEIGHT;
                    var addtional_fee_base = NinjaVanShippingFee.ADDITIONAL_FEE.First(x => x.area == area);
                    shipping_fee += ((int)(additional_weight / (int)addtional_fee_base.unit) + 1) *addtional_fee_base.amount;
                    base_fee = NinjaVanShippingFee.FEE.Where(x => x.area == area).OrderByDescending(x=>x.min_weight).First();
                }

                shipping_fee += base_fee.amount;
                return shipping_fee * NinjaVanShippingFee.RATE;
            }
            catch
            {

            }
            return -1;
        }
        private int GetShippingArea(int province_id)
        {
            if (NinjaVanShippingFee.WareHouse_Province_id.Contains(province_id))
            {
                return (int)ShippingAreaType.IN_TOWN;
            }
            if (NinjaVanShippingFee.HCM_ProvinceId==province_id)
            {
                return (int)ShippingAreaType.BETWEEN_CITY;
            }
            if (NinjaVanShippingFee.AREA_NORTH_ProvinceId.Contains(province_id))
            {
                return (int)ShippingAreaType.IN_AREA;
            }
            if (NinjaVanShippingFee.AREA_CENTER_ProvinceId.Contains(province_id))
            {
                return (int)ShippingAreaType.BETWEEN_NEARBY_AREA;
            }
            if (NinjaVanShippingFee.AREA_SOUTH_ProvinceId.Contains(province_id))
            {
                return (int)ShippingAreaType.BETWEEN_AREA;
            }
            return -1;
        }
        public async Task<string> GetBearerToken()
        {
            try
            {
                var request = new RestRequest(_configuration["NinjaVan:APIs:OAuth"], Method.Post);
                request.AddHeader("Content-Type", "application/json");
                var model = new
                {
                    client_id = _configuration["NinjaVan:client_id"],
                    client_secret = _configuration["NinjaVan:client_secret"],
                    grant_type = _configuration["NinjaVan:grant_type"],
                };
                var body = JsonConvert.SerializeObject(model);
                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await ninja_van_client.ExecuteAsync(request);
                var jsonData = JObject.Parse(response.Content);
                var response_token = jsonData["access_token"].ToString();

                if (response_token != null && response_token.Trim()!="")
                {
                    return response_token.Trim();
                }
            }
            catch
            {

            }
            return null;
        }
    }
}
