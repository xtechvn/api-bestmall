using Caching.Elasticsearch;
using Entities.Models;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Models.Client;
using HuloToys_Service.Models.Models;
using HuloToys_Service.RedisWorker;
using Newtonsoft.Json;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Address.Business
{
    public class AddressClientService
    {
        private readonly IConfiguration configuration;
        private readonly AccountClientESService accountClientESService;
        private readonly AddressClientESService addressClientESService;
        private readonly RedisConn redisService;
        private readonly ClientServices clientServices;
        private readonly LocationESService locationESService;

        public AddressClientService(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
            accountClientESService = new AccountClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            addressClientESService = new AddressClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            clientServices = new ClientServices(configuration);
            locationESService = new LocationESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);

        }
        public  ClientAddressListResponseModel AddressByClient(ClientAddressGeneralRequestModel request)
        {
            ClientAddressListResponseModel model = new ClientAddressListResponseModel()
            {
                list = new List<AddressClientFEModel>()
            };
            try
            {
                long account_client_id =  clientServices.GetAccountClientIdFromToken(request.token).Result;
                if (account_client_id <= 0)
                {
                    return model;
                }
                var account_client = accountClientESService.GetById(account_client_id);
                var client_id = (long)account_client.ClientId;
               
                var list = addressClientESService.GetByClientID(client_id);
                if (list!=null && list.Count > 0)
                {
                    List<Province> provinces = new List<Province>();
                    List<District> districts = new List<District>();
                    List<Ward> wards = new List<Ward>();

                    var cache_name = CacheType.PROVINCE;
                    var j_data =  redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"])).Result;
                    if (j_data != null && j_data.Trim() != "")
                    {
                        provinces = JsonConvert.DeserializeObject<List<Province>>(j_data);
                    }
                    cache_name = CacheType.DISTRICT;
                    j_data = redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"])).Result;
                    if (j_data != null && j_data.Trim() != "")
                    {
                        districts = JsonConvert.DeserializeObject<List<District>>(j_data);
                    }
                    cache_name = CacheType.WARD;
                    j_data = redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"])).Result;
                    if (j_data != null && j_data.Trim() != "")
                    {
                        wards = JsonConvert.DeserializeObject<List<Ward>>(j_data);
                    }
                    foreach (var item in list) {
                        AddressClientFEModel submit_item = JsonConvert.DeserializeObject<AddressClientFEModel>(JsonConvert.SerializeObject(item));
                        if (submit_item != null) {
                            submit_item.province_detail = (provinces==null || provinces.Count<=0) ? locationESService.GetProvincesByProvinceId(submit_item.ProvinceId) : provinces.FirstOrDefault(x=>x.ProvinceId==submit_item.ProvinceId);
                            submit_item.district_detail = (districts == null || districts.Count <= 0) ? locationESService.GetDistrictByDistrictId(submit_item.DistrictId) : districts.FirstOrDefault(x => x.DistrictId == submit_item.DistrictId);
                            submit_item.ward_detail = (wards == null || wards.Count <= 0) ? locationESService.GetWardsByWardId(submit_item.WardId) : wards.FirstOrDefault(x => x.WardId == submit_item.WardId);
                            model.list.Add(submit_item);
                        }
                    }
                }
               
            }
            catch
            {

            }
            return model;
        }
        public AddressClientFEModel DefaultAddress(ClientAddressGeneralRequestModel request)
        {
            AddressClientFEModel model = new AddressClientFEModel();
            try
            {
                long account_client_id = clientServices.GetAccountClientIdFromToken(request.token).Result;
                if (account_client_id <= 0)
                {
                    return model;
                }
                var account_client = accountClientESService.GetById(account_client_id);
                var client_id = (long)account_client.ClientId;

                var list = addressClientESService.GetByClientID(client_id);
                var provinces = redisService.Get(CacheType.PROVINCE, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                var district = redisService.Get(CacheType.DISTRICT, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                var ward = redisService.Get(CacheType.WARD, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                var data_provinces = JsonConvert.DeserializeObject<List<Province>>(provinces);
                var data_district = JsonConvert.DeserializeObject<List<District>>(district);
                var data_ward = JsonConvert.DeserializeObject<List<Ward>>(ward);

                if (list != null && list.Count > 0)
                {
                    var selected = list.FirstOrDefault(x => x.IsActive == true);
                    if (selected == null) selected = list.FirstOrDefault();
                    model = JsonConvert.DeserializeObject<AddressClientFEModel>(JsonConvert.SerializeObject(selected));
                    model.province_detail = data_provinces.FirstOrDefault(x => x.ProvinceId == selected.ProvinceId);
                    model.district_detail = data_district.FirstOrDefault(x => x.DistrictId == selected.DistrictId);
                    model.ward_detail = data_ward.FirstOrDefault(x => x.WardId == selected.WardId);
                }

            }
            catch
            {

            }
            return model;
        }
    }
}
