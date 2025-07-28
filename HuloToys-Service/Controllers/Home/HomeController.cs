using Azure.Core;
using Caching.Elasticsearch;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Models;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Home
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly RedisConn _redisService;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IConfiguration _configuration;
        public HomeController(RedisConn redisService, IAllCodeRepository allCodeRepository, IConfiguration configuration)
        {
            _redisService = redisService;
            _redisService.Connect();
            _allCodeRepository = allCodeRepository;
            _configuration = configuration;
        }
        [HttpPost("banner")]

        public async Task<ActionResult> GetBanner([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var cache_name = CacheType.HOMEPAGE_SLIDE;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    HomepageBannerModel result = new HomepageBannerModel();
                    if (j_data != null && j_data.Trim() != "")
                    {
                        result = JsonConvert.DeserializeObject<HomepageBannerModel>(j_data);
                        if(result != null && result.main!=null && result.main.Count>0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Success",
                                data = "",
                                main_slide = result.main.Where(x=>x.Description!=null && x.Description.Trim()!="").Select(x => new { x.OrderNo, x.Description }),
                                sub_banner = result.sub.Where(x => x.Description != null && x.Description.Trim() != "").Select(x => new { x.OrderNo, x.Description }),
                            });
                        }
                    }
                    var slide = _allCodeRepository.GetListByType("HOMEPAGE_SLIDE");
                    var sub = _allCodeRepository.GetListByType("HOMEPAGE_SUBBANNER");
                    var trending_main = _allCodeRepository.GetListByType("HOMEPAGE_TRENDINGMAIN");
                    var trending_sub = _allCodeRepository.GetListByType("HOMEPAGE_TRENDINGSUB");
                    result = new HomepageBannerModel()
                    {
                        main = slide == null ? new List<Models.Models.AllCode>() : slide.Where(x => x.Description != null && x.Description.Trim() != "").ToList(),
                        sub = sub == null ? new List<Models.Models.AllCode>() : sub.Where(x => x.Description != null && x.Description.Trim() != "").ToList(),
                        trending_main= trending_main == null ? new List<Models.Models.AllCode>() : trending_main.Where(x => x.Description != null && x.Description.Trim() != "").ToList(),
                        trending_sub = trending_sub == null ? new List<Models.Models.AllCode>() : trending_sub.Where(x => x.Description != null && x.Description.Trim() != "").ToList(),
                    };
                    if (slide != null && slide.Count > 0) {

                        _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        string static_url = _configuration["config_value:ImageStatic"];

                        foreach (var item in result.main) {
                            if (item.Description == null) continue;
                            item.Description = (!item.Description.Contains(static_url) && !item.Description.Contains("data:image") && !item.Description.Contains("http")) ? (static_url + item.Description) : item.Description;
                        }
                        foreach (var item in result.sub)
                        {
                            if (item.Description == null) continue;
                            item.Description = (!item.Description.Contains(static_url) && !item.Description.Contains("data:image") && !item.Description.Contains("http")) ? (static_url + item.Description) : item.Description;
                        }
                        foreach (var item in result.trending_main)
                        {
                            if (item.Description == null) continue;
                            item.Description = (!item.Description.Contains(static_url) && !item.Description.Contains("data:image") && !item.Description.Contains("http")) ? (static_url + item.Description) : item.Description;
                        }
                        foreach (var item in result.trending_sub)
                        {
                            if (item.Description == null) continue;
                            item.Description = (!item.Description.Contains(static_url) && !item.Description.Contains("data:image") && !item.Description.Contains("http")) ? (static_url + item.Description) : item.Description;
                        }
                    }

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = "",
                        main_slide = result.main.Where(x => x.Description != null && x.Description.Trim() != "").Select(x => new { x.OrderNo, x.Description }),
                        sub_banner = result.sub.Where(x => x.Description != null && x.Description.Trim() != "").Select(x => new { x.OrderNo, x.Description }),
                        //trending_main = result.trending_main.Where(x => x.Description != null && x.Description.Trim() != "").Select(x => new { x.OrderNo, x.Description }),
                        //trending_sub = result.trending_sub.Where(x => x.Description != null && x.Description.Trim() != "").Select(x => new { x.OrderNo, x.Description }),
                    });

                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }

    }
}
