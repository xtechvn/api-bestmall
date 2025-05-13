using Entities.Models;
using ENTITIES.ViewModels.ArticleViewModels;
using HuloToys_Service.Controllers.News.Business;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using System.Diagnostics;
using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Controllers
{
    [Route("api/news")]
    [ApiController]
    
    public class NewsController : ControllerBase
    {

        public IConfiguration configuration;
        private readonly RedisConn _redisService;
        private readonly NewsBusiness _newsBusiness;
        private readonly WorkQueueClient work_queue;
        private readonly DataMSContext _dbContext;

        public NewsController(IConfiguration config, RedisConn redisService , DataMSContext dbContext)
        {
            configuration = config;

            _redisService = redisService;
            _redisService = new RedisConn(config);
            _redisService.Connect();
            work_queue = new WorkQueueClient(configuration);
            _newsBusiness = new NewsBusiness(configuration, dbContext);
            _dbContext = dbContext;
        }
        [HttpPost("remote-upsert.json")]
        public async Task<IActionResult> RemoteUpsert([FromBody] ArticleModel model)
        {
            try
            {
                int node_redis = Convert.ToInt32(configuration["Redis:Database:db_common"]);
                if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Body))
                    return BadRequest("Tiêu đề hoặc nội dung bài viết không được để trống");

                // Lưu DB
                var articleId = await _newsBusiness.SaveArticle(model);

                string cache_name = CacheType.ARTICLE_CATEGORY_ID + 22;
                var j_data = await _redisService.GetAsync(cache_name, node_redis);

                // Push queue cập nhật ES
                var j_param = new Dictionary<string, object>
                {
                    { "store_name", "SP_GetAllArticle" },
                    { "index_es", "hulotoys_sp_getarticle" },
                    { "project_type", 1 },
                    { "id", -1 }
                };
                var dataPush = JsonConvert.SerializeObject(j_param);
                var queueResult = work_queue.InsertQueueSimple(dataPush, QueueName.queue_app_push);
                if (!articleId.IsSuccess)
                    return StatusCode(500, articleId.Message);

                return Ok(new
                {
                    isSuccess = true,
                    message = "Lưu bài viết thành công",
                    dataId = articleId
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RemoteUpsert Exception: " + ex);
                return StatusCode(500, $"Lỗi xử lý: {ex.Message}");
            }
        }

        [HttpPost("get-list-news.json")]
        public async Task<ActionResult> getListNews([FromBody] APIRequestGenericModel input)
        {
          
            try
            {
                Stopwatch sw = new Stopwatch(); // tạo stopwatch
                sw.Start(); // bắt đầu đo thời gian
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    int node_redis = Convert.ToInt32(configuration["Redis:Database:db_common"]);
                    var _category_detail = new GroupProductESModel();
                    var list_article = new List<CategoryArticleModel>();
                    int total_max_cache = 100; // số bản ghi tối đa để cache    
                    int category_id = Convert.ToInt32(objParr[0]["category_id"]);

                    int skip = Convert.ToInt32(objParr[0]["skip"]);
                    int top = Convert.ToInt32(objParr[0]["top"]);

                    string cache_name = CacheType.ARTICLE_CATEGORY_ID + category_id;
                    var j_data = await _redisService.GetAsync(cache_name, node_redis);

                    // Kiểm tra có trong cache không ?
                    if (!string.IsNullOrEmpty(j_data))
                    {
                        list_article = JsonConvert.DeserializeObject<List<CategoryArticleModel>>(j_data);
                        // Nếu tổng số bản ghi muốn lấy vượt quá số bản ghi trong Redis thì vào ES lấy                        
                        if (top > list_article.Count())
                        {
                            // Lấy ra trong es
                            list_article = await _newsBusiness.getListNews(category_id, top);
                        }
                    }
                    else // Không có trong cache
                    {
                        // Lấy ra số bản ghi tối đa để cache
                        list_article = await _newsBusiness.getListNews(category_id, Math.Max(total_max_cache, top));

                        if (list_article.Count() > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(list_article), node_redis);
                        }
                    }

                    if (list_article != null && list_article.Count() > 0)
                    {
                        sw.Stop(); // dừng đo
                        Console.WriteLine($"Thời gian chạy: {sw.ElapsedMilliseconds} ms");
                        return Ok(new
                        {
                             speed =  sw.ElapsedMilliseconds,
                            status = (int)ResponseType.SUCCESS,
                            data = list_article.ToList().Skip(skip).Take(top)
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "data empty !!!"
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Error: " + ex.ToString(),
                });
            }
        }
        /// <summary>
        /// Lấy ra chi tiết bài viết
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-article-detail.json")]
        public async Task<ActionResult> getArticleDetail([FromBody] APIRequestGenericModel input)
        {
            try
            {
                int node_redis = Convert.ToInt32(configuration["Redis:Database:db_common"]);
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var article_detail = new ArticleModel2();

                    long article_id = Convert.ToInt64(objParr[0]["article_id"]);

                    string cache_name = CacheType.ARTICLE_ID + article_id;
                    var j_data = await _redisService.GetAsync(cache_name, node_redis);

                    // Kiểm tra có trong cache không ?
                    if (!string.IsNullOrEmpty(j_data))
                    {
                        article_detail = JsonConvert.DeserializeObject<ArticleModel2>(j_data);
                    }
                    else // Không có trong cache
                    {
                        article_detail = await _newsBusiness.getArticleDetail(article_id);

                        if (article_detail != null)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(article_detail), node_redis);
                        }
                    }

                    if (article_detail != null)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = article_detail
                        }); ;
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "data empty !!!"
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Error: " + ex.ToString(),
                });
            }
        }

        [HttpPost("get-total-news.json")]
        public async Task<ActionResult> getTotalItemNewsByCategoryId([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    int category_id = Convert.ToInt32(objParr[0]["category_id"]);
                    // Lấy ra trong es
                    var total = await _newsBusiness.getTotalItemNewsByCategoryId(category_id);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = total
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Error: " + ex.ToString(),
                });
            }
        }

        /// <summary>
        /// Lấy ra bài viết theo 1 chuyên mục
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-list-by-categoryid.json")]
        public async Task<ActionResult> getListArticleByCategoryId([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //string j_param = "{'category_id':1}";
                //token = CommonHelper.Encode(j_param, configuration["KEY:private_key"]);
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    string db_type = string.Empty;
                    int _category_id = Convert.ToInt32(objParr[0]["category_id"]);
                    string cache_name = CacheType.ARTICLE_CATEGORY_ID + _category_id;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var list_article = new List<ArticleFeModel>();

                    if (j_data != null)
                    {
                        list_article = JsonConvert.DeserializeObject<List<ArticleFeModel>>(j_data);
                        db_type = "cache";
                    }
                    else
                    {
                        list_article = await _newsBusiness.getArticleListByCategoryId(_category_id);
                        list_article = list_article.GroupBy(s => s.id).Select(s => s.First()).ToList();
                        if (list_article.Count() > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(list_article), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                        }
                        db_type = "database";
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data_list = list_article,
                        category_id = _category_id,
                        msg = "Get " + db_type + " Successfully !!!"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Error: " + ex.ToString(),
                });
            }
        }
        [HttpPost("get-most-viewed-article.json")]
        public async Task<ActionResult> GetMostViewedArticle([FromBody] APIRequestGenericModel input)
        {
            try
            {
                int status = (int)ResponseType.FAILED;
                string msg = "No Item Found";
                var data_list = new List<ArticleFeModel>();
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    string cache_name = CacheType.ARTICLE_MOST_VIEWED;
                    string j_data = null;
                    try
                    {
                        j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - GetMostViewedArticle: " + ex + "\n Token: " + input.token);

                    }
                    var detail = new ArticleFeModel();

                    if (j_data != null && j_data != "[]")
                    {
                        data_list = JsonConvert.DeserializeObject<List<ArticleFeModel>>(j_data);
                        msg = "Get From Cache Success";

                    }
                    else
                    {
                        NewsMongoService services = new NewsMongoService(configuration);
                        var list = await services.GetMostViewedArticle();
                        if (list != null && list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                var article = await _newsBusiness.GetMostViewedArticle(item.articleID);
                                if (article != null) data_list.Add(article);
                            }
                            try
                            {
                                _redisService.Set(cache_name, JsonConvert.SerializeObject(data_list), DateTime.Now.AddMinutes(5), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                            }
                            catch (Exception ex)
                            {
                                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - GetMostViewedArticle: " + ex + "\n Token: " + input.token);

                            }
                            status = (int)ResponseType.SUCCESS;
                            msg = "Get from DB Success";
                        }
                    }
                    return Ok(new { status = (int)ResponseType.SUCCESS, msg = msg, data = data_list });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - GetMostViewedArticle: " + ex + " token = " + input.token);

                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution"
                });
            }
        }
        [HttpPost("get-detail.json")]
        public async Task<ActionResult> GetArticleDetailLite([FromBody] APIRequestGenericModel input)
        {
            try
            {
                // string j_param = "{'article_id':1}";


                // token = CommonHelper.Encode(j_param, configuration["KEY:private_key"]);

                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    string db_type = string.Empty;
                    long article_id = Convert.ToInt64(objParr[0]["article_id"]);
                    string cache_name = CacheType.ARTICLE_ID + article_id;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var detail = new ArticleFeDetailModel();

                    if (j_data != null)
                    {
                        detail = JsonConvert.DeserializeObject<ArticleFeDetailModel>(j_data);
                        db_type = "cache";
                    }
                    else
                    {
                        detail = await _newsBusiness.GetArticleDetailLite(article_id);
                        detail.Tags = await _newsBusiness.GetAllTagByArticleID(article_id);
                        if (detail != null)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(detail), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                            db_type = "database";
                        }

                    }
                    var view_count = new NewsViewCount()
                    {
                        articleID = article_id,
                        pageview = 1
                    };
                    NewsMongoService services = new NewsMongoService(configuration);
                    services.AddNewOrReplace(view_count);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = detail,
                        msg = "Get " + db_type + " Successfully !!!",
                        _token = input.token
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[api/article/detail] = " + ex.ToString(),
                    _token = input.token
                });
            }
        }

        [HttpPost("find-article.json")]
        public async Task<ActionResult> FindArticleByTitle([FromBody] APIRequestGenericModel input)
        {
            try
            {
                // string j_param = "{'title':'54544544444','parent_cate_faq_id':279}";
                // token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    string db_type = "database";
                    string title = (objParr[0]["title"]).ToString().Trim();
                    int parent_cate_faq_id = Convert.ToInt32(objParr[0]["parent_cate_faq_id"]);

                    var detail = new List<ArticleRelationModel>();

                    detail = await _newsBusiness.FindArticleByTitle(title, parent_cate_faq_id);

                    return Ok(new
                    {
                        status = detail.Count() > 0 ? (int)ResponseType.SUCCESS : (int)ResponseType.EMPTY,
                        data_list = detail.Count() > 0 ? detail : null,
                        msg = "Get " + db_type + " Successfully !!!",
                        _token = input.token
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "find-article.json = " + ex.ToString(),
                    _token = input.token
                });
            }
        }

        /// <summary>
        /// Lấy ra bài viết theo 1 chuyên mục, skip+take, sắp xếp theo ngày gần nhất
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-list-by-categoryid-order.json")]
        public async Task<ActionResult> getListArticleByCategoryIdOrderByDate([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                string msg = "";
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    string db_type = string.Empty;
                    int category_id = Convert.ToInt32(objParr[0]["category_id"]);
                    int skip = Convert.ToInt32(objParr[0]["skip"]);
                    int take = Convert.ToInt32(objParr[0]["take"]);
                    string cache_key = CacheType.CATEGORY_NEWS + category_id;
                    var j_data = await _redisService.GetAsync(cache_key, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    List<ArticleFeModel> data_list;
                    List<ArticleFeModel> pinned_article;
                    List<ArticleFeModel> video_article;
                    int total_count = -1;
                    int total_page = 1;
                    if (j_data == null || j_data == "")
                    {
                        var group_product = await _newsBusiness.GetGroupProductNameAsync(category_id);
                        var data_100 = await _newsBusiness.getArticleListByCategoryIdOrderByDate(category_id, 0, 100, group_product);
                        if (skip + take > 100)
                        {
                            var data = await _newsBusiness.getArticleListByCategoryIdOrderByDate(category_id, skip, take, group_product);
                            data_list = data.list_article_fe;
                            total_count = data.total_item_count;
                            pinned_article = data.list_article_pinned;
                            total_page = Convert.ToInt32(total_count / take);
                            if (total_page < ((float)total_count / take))
                            {
                                total_page++;
                            }
                        }
                        else
                        {
                            data_list = data_100.list_article_fe.Skip(skip == 1 ? 0 : skip).Take(take).ToList();
                            total_count = data_100.total_item_count;
                            pinned_article = data_100.list_article_pinned;
                            total_page = Convert.ToInt32(total_count / take);
                            if (total_page < ((float)total_count / take))
                            {
                                total_page++;
                            }
                        }


                        try
                        {
                            _redisService.Set(cache_key, JsonConvert.SerializeObject(data_100), DateTime.Now.AddMinutes(15), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                        }
                        catch (Exception ex)
                        {
                            LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - getListArticleByCategoryIdOrderByDate: " + ex + "\n Token: " + input.token);

                        }
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data_list = data_list,
                            pinned = pinned_article,
                            total_item = total_count,
                            total_page = total_page

                        });

                        //return Content(JsonConvert.SerializeObject(data_list));
                    }
                    else
                    {
                        var group_product = await _newsBusiness.GetGroupProductNameAsync(category_id);

                        if (skip + take > 100)
                        {
                            var data = await _newsBusiness.getArticleListByCategoryIdOrderByDate(category_id, skip, take, group_product);
                            data_list = data.list_article_fe;
                            total_count = data.total_item_count;
                            pinned_article = data.list_article_pinned;
                            total_page = Convert.ToInt32(total_count / take);
                            if (total_page < ((float)total_count / take))
                            {
                                total_page++;
                            }
                        }
                        else
                        {
                            var data_pinned = new List<ArticleFeModel>();
                            var i = 0;
                            var data_100 = JsonConvert.DeserializeObject<ArticleFEModelPagnition>(j_data);
                            var data_pinned_1 = data_100.list_article_pinned.Where(s => s.position == 1).Skip(skip == 1 ? 0 : (skip - 1) * take).Take(take).ToList();
                            if (data_pinned_1 != null && data_pinned_1.Count > 0)
                            {
                                data_pinned.AddRange(data_pinned_1);
                            }
                            else
                            {
                                var data = data_100.list_article_fe.Skip(skip == 1 ? 0 : (skip - 1) * take).Take(take).ToList();
                                if (data != null && data.Count > 0)
                                {
                                    data[0].position = 1;
                                    data_pinned.Add(data[0]);
                                    i++;
                                }
                            }
                            var data_pinned_2 = data_100.list_article_pinned.Where(s => s.position == 2).Skip(skip == 1 ? 0 : (skip - 1) * take).Take(take).ToList();
                            if (data_pinned_2 != null && data_pinned_2.Count > 0)
                            {
                                data_pinned.AddRange(data_pinned_2);

                            }
                            else
                            {
                                var data = data_100.list_article_fe.Skip(skip == 1 ? 0 : (skip - 1) * (take + 1)).Take(take).ToList();
                                if (data != null && data.Count > 0)
                                {
                                    data[0].position = 2;
                                    data_pinned.Add(data[0]);
                                    i++;
                                }
                            }
                            var data_pinned_3 = data_100.list_article_pinned.Where(s => s.position == 3).Skip(skip == 1 ? 0 : (skip - 1) * take).Take(take).ToList();
                            if (data_pinned_3 != null && data_pinned_3.Count > 0)
                            {
                                data_pinned.AddRange(data_pinned_3);
                            }
                            else
                            {
                                var data = data_100.list_article_fe.Skip(skip == 1 ? 0 : (skip - 1) * (take + 2)).Take(take).ToList();
                                if (data != null && data.Count > 0)
                                {
                                    data[0].position = 3;
                                    data_pinned.Add(data[0]);
                                    i++;
                                }
                            }

                            data_list = data_100.list_article_fe.Skip(skip == 1 ? 0 : (skip - 1) * (take + i)).Take(take).ToList();
                            total_count = data_100.total_item_count;
                            pinned_article = data_pinned;
                            total_page = Convert.ToInt32(total_count / take);
                            if (total_page < ((float)total_count / take))
                            {
                                total_page++;
                            }
                        }

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data_list = data_list,
                            pinned = pinned_article,
                            total_item = total_count,
                            total_page = total_page
                        });
                        // return Content(JsonConvert.SerializeObject(data_list));
                    }

                }
                else
                {
                    msg = "Key ko hop le";
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = msg
                });
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution.",
                    _token = input.token
                });
            }
        }
        /// <summary>
        /// Lấy ra tất cả các chuyên mục thuộc B2C
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-category.json")]
        public async Task<ActionResult> GetAllCategory([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //string j_param = "{'confirm':1}";
                //token = CommonHelper.Encode(j_param, configuration["DataBaseConfig:key_api:b2c"]);
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    int _category_id = Convert.ToInt32(objParr[0]["category_id"]);
                    string cache_name = CacheType.ARTICLE_CATEGORY_MENU + "_" + _category_id;
                    string j_data = null;
                    try
                    {
                        j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    }
                    catch (Exception ex)
                    {

                        LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - GetMostViewedArticle: " + ex + "\n Token: " + input.token);
                    }
                    List<ArticleGroupViewModel> group_product = null;

                    if (j_data != null)
                    {
                        group_product = JsonConvert.DeserializeObject<List<ArticleGroupViewModel>>(j_data);
                    }
                    else
                    {
                        group_product = await _newsBusiness.GetArticleCategoryByParentID(_category_id);
                        if (group_product.Count > 0)
                        {
                            try
                            {
                                _redisService.Set(cache_name, JsonConvert.SerializeObject(group_product), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                            }
                            catch (Exception ex)
                            {
                                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - GetAllCategory: " + ex + "\n Token: " + input.token);

                            }
                        }
                    }

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        categories = group_product,
                        data = group_product
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - GetAllCategory: " + ex + "\n Token: " + input.token);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Error: " + ex.ToString(),
                });
            }
        }
        [HttpPost("get-list-by-tag-order.json")]
        public async Task<ActionResult> getListArticleByTagsOrder([FromBody] APIRequestGenericModel input)
        {

            try
            {
                //string j_param = "{'tag':'#adavigo','size':10, 'page': 1}";
                //token = CommonHelper.Encode(j_param, configuration["DataBaseConfig:key_api:b2c"]);

                JArray objParr = null;
                string msg = "";
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    string db_type = string.Empty;
                    string tag = objParr[0]["tag"].ToString();
                    int page = Convert.ToInt32(objParr[0]["page"]);
                    int size = Convert.ToInt32(objParr[0]["size"]);
                    int take = (size <= 0) ? 10 : size;
                    int skip = ((page - 1) <= 0) ? 0 : (page - 1) * take;
                    string cache_key = CacheType.CATEGORY_TAG + CommonHelper.MD5Hash(tag.Trim().ToLower());
                    string j_data = null;
                    try
                    {
                        j_data = await _redisService.GetAsync(cache_key, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - getListArticleByTagsOrder: " + ex + "\n Token: " + input.token);

                    }
                    List<ArticleFeModel> data_list;
                    List<ArticleFeModel> pinned_article;
                    int total_count = -1;
                    int total_page = 1;
                    if (j_data == null || j_data == "")
                    {
                        var data_100 = await _newsBusiness.getArticleListByTags(tag, 0, 100);
                        if (skip + take > 100)
                        {
                            var data = await _newsBusiness.getArticleListByTags(tag, skip, take);
                            data_list = data.list_article_fe;
                            total_count = data.total_item_count;
                            total_page = Convert.ToInt32(total_count / take);
                            if (total_page < ((float)total_count / take))
                            {
                                total_page++;
                            }
                        }
                        else
                        {
                            data_list = data_100.list_article_fe.Skip(skip).Take(take).ToList();
                            total_count = data_100.total_item_count;
                            total_page = Convert.ToInt32(total_count / take);
                            if (total_page < ((float)total_count / take))
                            {
                                total_page++;
                            }
                        }

                        try
                        {
                            _redisService.Set(cache_key, JsonConvert.SerializeObject(data_100), DateTime.Now.AddMinutes(15), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                        }
                        catch (Exception ex)
                        {
                            LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - getListArticleByTagsOrder: " + ex + "\n Token: " + input.token);

                        }
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = data_list,
                            total_item = total_count,
                            total_page = total_page

                        });

                    }
                    else
                    {
                        var data_100 = JsonConvert.DeserializeObject<ArticleFEModelPagnition>(j_data);
                        if (skip + take > 100)
                        {
                            var data = await _newsBusiness.getArticleListByTags(tag, skip, take);
                            data_list = data.list_article_fe;
                            total_count = data.total_item_count;
                            total_page = Convert.ToInt32(total_count / take);
                            if (total_page < ((float)total_count / take))
                            {
                                total_page++;
                            }
                        }
                        else
                        {
                            data_list = data_100.list_article_fe.Skip(skip).Take(take).ToList();
                            total_count = data_100.total_item_count;
                            total_page = Convert.ToInt32(total_count / take);
                            if (total_page < ((float)total_count / take))
                            {
                                total_page++;
                            }
                        }
                        _redisService.Set(cache_key, JsonConvert.SerializeObject(data_100), DateTime.Now.AddMinutes(15), Convert.ToInt32(configuration["Redis:Database:db_common"]));

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = data_list,
                            total_item = total_count,
                            total_page = total_page
                        });
                    }

                }
                else
                {
                    msg = "Key ko hop le";
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = msg
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "NewsController - get-list-by-categoryid-order.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution.",
                    _token = input.token
                });
            }
        }
        [HttpPost("find-all-article.json")]
        public async Task<ActionResult> FindArticleByBody([FromBody] APIRequestGenericModel input)
        {
            try
            {
                // string j_param = "{'title':'54544544444','parent_cate_faq_id':279}";
                // token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    string db_type = "database";
                    string title = (objParr[0]["title"]).ToString().Trim();
                    string parent_cate_faq_id = objParr[0]["parent_cate_faq_id"].ToString();

                    var detail = new List<ArticleRelationModel>();

                    detail = await _newsBusiness.FindArticleByBody(title, parent_cate_faq_id);

                    return Ok(new
                    {
                        status = detail.Count() > 0 ? (int)ResponseType.SUCCESS : (int)ResponseType.EMPTY,
                        data_list = detail.Count() > 0 ? detail : null,
                        msg = "Get " + db_type + " Successfully !!!",
                        _token = input.token
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "find-article.json = " + ex.ToString(),
                    _token = input.token
                });
            }
        }
    }
}
