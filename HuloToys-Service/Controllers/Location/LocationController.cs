using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using HuloToys_Service.Utilities.Lib;
using Utilities;
using Utilities.Contants;
using Caching.Elasticsearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Models.Location;
using Repositories.IRepositories;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IProvinceRepository _provinceRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly IWardRepository _wardRepository;
        private readonly LocationESService locationESService;
        private readonly RedisConn _redisService;

        public LocationController(IConfiguration configuration, RedisConn redisService, IProvinceRepository provinceRepository
            , IDistrictRepository districtRepository, IWardRepository wardRepository) {
            _configuration = configuration;
            locationESService = new LocationESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            _provinceRepository= provinceRepository;
            _districtRepository = districtRepository;
            _wardRepository = wardRepository;
            _redisService = new RedisConn(_configuration);
            _redisService.Connect();
        }
        [HttpPost("province")]
        public async Task<ActionResult> Province([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<LocationRequestModel>(objParr[0].ToString());
                    if (request == null)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var cache_name = CacheType.PROVINCE;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    List<Province> result = null;
                    if (j_data != null && j_data.Trim() != "")
                    {
                        result = JsonConvert.DeserializeObject<List<Province>>(j_data);
                    }
                    else
                    {
                        result = await _provinceRepository.GetProvincesList();
                        if(result!=null && result.Count > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        }
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = result != null? result : new List<Province>()
                    });
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("district")]
        public async Task<ActionResult> District([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<LocationRequestModel>(objParr[0].ToString());
                    if (request == null || request.id==null || request.id.Trim()=="")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var cache_name = CacheType.DISTRICT;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    List<District> result = null;
                    if (j_data != null && j_data.Trim() != "")
                    {
                        result = JsonConvert.DeserializeObject<List<District>>(j_data);
                    }
                    else
                    {
                        result = await _districtRepository.GetDistrictList();
                        if (result != null && result.Count > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        }
                    }
                    var district_es = (result == null|| result.Count<=0) ? new List<District>(): result.Where(x => x.ProvinceId == request.id).ToList();
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = district_es != null ? district_es : new List<District>()
                    });

                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }
        [HttpPost("ward")]
        public async Task<ActionResult> Ward([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<LocationRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var cache_name = CacheType.WARD;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    List<Ward> result = null;
                    if (j_data != null && j_data.Trim() != "")
                    {
                        result = JsonConvert.DeserializeObject<List<Ward>>(j_data);
                    }
                    else
                    {
                        result = await _wardRepository.GetWardList();
                        if (result != null && result.Count > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        }
                    }
                    var ward_es = (result == null || result.Count <= 0) ? new List<Ward>() : result.Where(x => x.DistrictId == request.id).ToList();
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = ward_es != null ? ward_es : new List<Ward>()
                    });

                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }
    }
}
