
using ENTITIES.ViewModels.Notify;
using HuloToys_Service.Controllers.NOTIFY.Business;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.NOTIFY
{
    [Route("api")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        //private readonly INotifyRepository notifyRepository;
        private readonly ISubscriber _subscriber;
        private readonly NotifyServices _notifyMongoDAL;
        private readonly RedisConn _redisService;

        public NotifyController(IConfiguration configuration)
        {
            _configuration = configuration;
            //notifyRepository = _notifyRepository;
            var host = configuration["Redis:Host"];
            var port = configuration["Redis:Port"];
            var connection = ConnectionMultiplexer.Connect($"{host}:{port}");

            _subscriber = connection.GetSubscriber();
            _redisService = new RedisConn(_configuration);
            _redisService.Connect();
            _notifyMongoDAL = new NotifyServices(configuration);
        }
        /// <summary>
        /// Lấy ra danh sách noti của user:
        /// 1. Tổng số msg chưa đọc
        /// 2. Danh sách chi tiết các message chưa đọc
        /// Khi load page sẽ call lại api này để pub lại
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("notify/get-list.json")]
        public async Task<ActionResult> getListNotify([FromForm] string token)
        {
            try
            {
                if (!CommonHelper.GetParamWithKey(token, out JArray objParr, _configuration["DataBaseConfig:key_api:b2c"]))
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Token không hợp lệ"
                    });
                }

                // --- Lấy param từ token ---
                var user_id = Convert.ToInt32(objParr[0]["user_id"]);
                var pageindex = Convert.ToInt32(objParr[0]["pageindex"]);
                var pagesize = Convert.ToInt32(objParr[0]["pagesize"]);

                string cacheName = $"NOTIFY_{user_id}";
                int dbIndex = 14;

                NotifySummeryViewModel objNotify = null;

                // --- Thử lấy từ Redis trước ---
                var cacheData = _redisService.Get(cacheName, dbIndex);
                if (!string.IsNullOrEmpty(cacheData))
                {
                    objNotify = JsonConvert.DeserializeObject<NotifySummeryViewModel>(cacheData);
                }

                // --- Nếu cache rỗng thì query DB ---
                if (objNotify == null)
                {
                    objNotify = await _notifyMongoDAL.getListNotifyPage(user_id, pageindex, pagesize);

                    if (objNotify != null)
                    {
                        // Set lại cache để lần sau dùng luôn
                        _redisService.Set(cacheName, JsonConvert.SerializeObject(objNotify), dbIndex);
                    }
                }

                // --- Check kết quả ---
                if (objNotify != null && !(objNotify.total_not_seen == 0 && objNotify.lst_not_seen_detail.Count == 0))
                {
                    // Có notify thì publish realtime
                    _subscriber.Publish(cacheName, JsonConvert.SerializeObject(objNotify));

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = $"Thông tin notify của user_id {user_id} đã public thành công",
                        total = objNotify.lst_not_seen_detail.Count,
                        data = objNotify
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.EMPTY,
                        msg = "Hiện tại không có notify nào của user này",
                        total = 0,
                        data = objNotify
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"notify/get-list.json error: {ex} | token = {token}");
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Transaction Error !!!"
                });
            }
        }


        [HttpPost("notify/message/send.json")]
        public async Task<ActionResult> sendNotify([FromForm] string token) // 👈 thêm [FromForm]
        {
            try
            {
                // Giải mã token
                if (!CommonHelper.GetParamWithKey(token, out JArray objParr, _configuration["DataBaseConfig:key_api:b2c"]))
                {
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Key invalid!" });
                }

                var p0 = objParr[0];

                string userName = p0["user_name_send"]?.ToString() ?? "";
                int userIdSend = Convert.ToInt32(p0["user_id_send"] ?? "0");
                string code = p0["code"]?.ToString() ?? "";
                string linkRedirect = p0["link_redirect"]?.ToString() ?? "";
                short moduleType = Convert.ToInt16(p0["module_type"] ?? "0");
                short actionType = Convert.ToInt16(p0["action_type"] ?? "0");
                string serviceCode = p0["service_code"]?.ToString() ?? "";

                // 👇 thêm trực tiếp user nhận từ CMS
                int userReceiverId = Convert.ToInt32(p0["user_receiver_id"] ?? "0");

                // Lấy nhãn mô-đun và action
                //string moduleLabel = NotifyLabelName.getModuleName(moduleType);
                string actionLabel = NotifyLabelName.getActionName(actionType);

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(actionLabel) || userReceiverId == 0)
                {
                    return Ok(new { status = (int)ResponseType.EMPTY, msg = "Thiếu tham số notify" });
                }

                // Nội dung notify custom theo actionType
                string content;
                if (actionType == 6) // NHAN_TRIEN_KHAI
                {
                    content = $" Đơn hàng {code} đang được {actionLabel}";
                }
                else
                {
                    content = $" Đơn hàng {code} đã được {actionLabel} thành công";
                }

                // --- Lưu notify chính ---
                var model = new MessageViewModel
                {
                    content = content,
                    code = code,
                    module_type = moduleType,
                    send_date = DateTimeOffset.Now.ToUnixTimeSeconds(),

                    user_id_send = userIdSend,
                    user_name_send = userName
                };

                var notifyId = await _notifyMongoDAL.pushMessage(model);

                if (string.IsNullOrEmpty(notifyId))
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Không lưu được notify" });
                }

                // --- Lưu người nhận + Clear Cache + Update Cache ---
                var receiverModel = new ReceiverMessageViewModel
                {
                    seen_status = (short)SeenType.NOT_SEEN,
                    notify_id = notifyId,
                    seen_date = DateTimeOffset.Now.ToUnixTimeSeconds(), // chưa xem
                    user_receiver_id = userReceiverId,
                    link_redirect = linkRedirect,
                    content = content,
                    company_type = 0
                };

                await _notifyMongoDAL.pushReceiverReadMessage(receiverModel);

                string cacheName = $"NOTIFY_{userReceiverId}";
                int dbIndex = 14;

                // clear cache cũ
                _redisService.clear(cacheName, dbIndex);

                // update lại cache
                var objNotify = await _notifyMongoDAL.getListNotify(userReceiverId);
                if (objNotify != null)
                {
                    _redisService.Set(cacheName, JsonConvert.SerializeObject(objNotify), dbIndex);
                    _subscriber.Publish(cacheName, JsonConvert.SerializeObject(objNotify));
                }

                return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Notify thành công", preview = content });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendOrderNotify error: " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Exception: " + ex.Message });
            }
        }

        [HttpPost("notify/message/update-status-view.json")]
        public async Task<ActionResult> updateStatusReadNotify([FromForm] string token)
        {
            try
            {
                JArray objParr = null;
                var j_param = new Dictionary<string, object>
                {
                    {"notify_id", "64c4fab98a016634cdddced5,64c4fac68a016634cdddced7"},
                    {"user_seen_id", "34"},
                    {"seen_status", "1"}
                };
                var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
                int user_seen_id = -1;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["DataBaseConfig:key_api:b2c"]))
                {
                    var notify_id = objParr[0]["notify_id"].ToString();
                    var seen_status = Convert.ToInt16(objParr[0]["seen_status"]);
                    user_seen_id = Convert.ToInt32(objParr[0]["user_seen_id"]);

                    var arr_notify_id = notify_id.Split(",");

                    var rs = await _notifyMongoDAL.updateSeenNotify(arr_notify_id.ToList(), seen_status, user_seen_id);
                    if (rs)
                    {
                        // clear cache
                        string cache_name = "NOTIFY_" + user_seen_id;

                        int dbIndex = 14;

                        // clear cache cũ
                        _redisService.clear(cache_name, dbIndex);

                        // Update lại cho cache này
                        var obj_notify = await _notifyMongoDAL.getListNotify(user_seen_id);
                        if (obj_notify != null)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(obj_notify), dbIndex);
                            _subscriber.Publish(cache_name, JsonConvert.SerializeObject(obj_notify));
                        }
                    }
                    return Ok(new
                    {
                        status = rs ? (int)ResponseType.SUCCESS : (int)ResponseType.ERROR,
                    });

                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "error with preview notify: " + token
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "error with preview notify:  ex " + ex.ToString()
                });
            }
        }



    }
}
