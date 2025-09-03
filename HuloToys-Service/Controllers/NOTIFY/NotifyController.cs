
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
        public async Task<ActionResult> GetListNotify([FromForm] string token)
        {
            try
            {
                // --- Validate token ---
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

                string cacheListKey = $"NOTIFY_{user_id}";          // Redis List chứa notify mới nhất
                string cacheUnseenKey = $"NOTIFY_UNSEEN_{user_id}"; // Redis Set chứa notify chưa đọc
                int dbIndex = 14;

                List<ReceiverMessageViewModel> notifyList = new List<ReceiverMessageViewModel>();

                if (pageindex == 1)
                {

                    // Nếu muốn lấy 10 notify mới nhất theo new → old
                    var redisData = _redisService.LRange(cacheListKey, -pagesize, -1, dbIndex);

                    if (redisData != null && redisData.Any())
                    {
                        notifyList = redisData
                            .Select(x => JsonConvert.DeserializeObject<ReceiverMessageViewModel>(x))
                            .Reverse()
                            .ToList(); // giữ nguyên thứ tự cũ → mới
                    }

                    else
                    {
                        // 2️⃣ Nếu Redis trống thì fallback Mongo
                        notifyList = await _notifyMongoDAL.getListNotifyPage(user_id, pageindex, pagesize);

                        // 3️⃣ Nếu Mongo có data thì cache lại Redis
                        if (notifyList != null && notifyList.Any())
                        {
                            foreach (var item in notifyList)
                            {
                                _redisService.LPush(cacheListKey, JsonConvert.SerializeObject(item), dbIndex);
                            }
                            _redisService.LTrim(cacheListKey, 0, 49, dbIndex); // giữ 50 record mới nhất
                        }
                    }
                }
                else
                {
                    // Các page > 1 thì query thẳng Mongo
                    notifyList = await _notifyMongoDAL.getListNotifyPage(user_id, pageindex, pagesize);
                }

                // --- Đếm unseen trực tiếp từ Mongo qua DAL ---
                var unseenInfo = await _notifyMongoDAL.GetUnseenNotifyInfo(user_id);
                var unseenIds = _redisService.SMembers(cacheUnseenKey, dbIndex);

                // --- Wrap kết quả ---
                var objNotify = new NotifySummeryViewModel
                {
                    total_not_seen = unseenInfo.total,
                    lst_id_not_seen = string.Join(",", unseenIds),
                    lst_not_seen_detail = notifyList
                };

                if (objNotify.lst_not_seen_detail.Any())
                {
                    // publish realtime
                    _subscriber.Publish(cacheListKey, JsonConvert.SerializeObject(objNotify));

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
        //
        [HttpPost("notify/get-count.json")]
        public async Task<ActionResult> GetCountNotify([FromForm] string token)
        {
            try
            {
                if (!CommonHelper.GetParamWithKey(token, out JArray objParr, _configuration["DataBaseConfig:key_api:b2c"]))
                {
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Token không hợp lệ" });
                }

                var user_id = Convert.ToInt32(objParr[0]["user_id"]);
                int dbIndex = 14;
                string cacheName = $"NOTIFY_{user_id}";

                // Gọi DAL lấy unseen info
                var (total, ids) = await _notifyMongoDAL.GetUnseenNotifyInfo(user_id);

                var obj = new
                {
                    total_not_seen = total,
                    lst_id_not_seen = string.Join(",", ids)
                };

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = $"Notify count user_id {user_id} thành công",
                    total = total,
                    data = obj
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"notify/get-count.json error: {ex} | token = {token}");
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Transaction Error !!!" });
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
                string unseenKey = $"NOTIFY_UNSEEN_{userReceiverId}";
                int dbIndex = 14;

                //// clear cache cũ
                //_redisService.clear(cacheName, dbIndex);

                //// update lại cache
                //var objNotify = await _notifyMongoDAL.getListNotify(userReceiverId);
                //if (objNotify != null)
                //{
                //    _redisService.Set(cacheName, JsonConvert.SerializeObject(objNotify), dbIndex);
                //    _subscriber.Publish(cacheName, JsonConvert.SerializeObject(objNotify));
                //}
                _redisService.RPush(cacheName, JsonConvert.SerializeObject(receiverModel), dbIndex);
                _redisService.LTrim(cacheName, -50, -1, dbIndex); // giữ 50 bản ghi cuối cùng

                _redisService.SAdd(unseenKey, notifyId, dbIndex);

                _subscriber.Publish(cacheName, JsonConvert.SerializeObject(receiverModel));

                return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Notify thành công", preview = content });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendOrderNotify error: " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Exception: " + ex.Message });
            }
        }

        [HttpPost("notify/message/update-status-view.json")]
        public async Task<ActionResult> UpdateStatusReadNotify([FromForm] string token)
        {
            try
            {
                // --- Validate token ---
                if (!CommonHelper.GetParamWithKey(token, out JArray objParr, _configuration["DataBaseConfig:key_api:b2c"]))
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Token không hợp lệ"
                    });
                }

                var notifyIds = objParr[0]["notify_id"].ToString().Split(",").ToList();
                var seen_status = Convert.ToInt16(objParr[0]["seen_status"]);
                var user_seen_id = Convert.ToInt32(objParr[0]["user_seen_id"]);

                // --- Update Mongo ---
                var rs = await _notifyMongoDAL.updateSeenNotify(notifyIds, seen_status, user_seen_id);

                if (!rs)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Không update được notify"
                    });
                }

                int dbIndex = 14;
                string cacheListKey = $"NOTIFY_{user_seen_id}";

                // --- PATCH update trực tiếp vào Redis list ---
                var redisData = _redisService.LRange(cacheListKey, 0, -1, dbIndex);
                if (redisData != null && redisData.Any())
                {
                    for (int i = 0; i < redisData.Count; i++)
                    {
                        var item = JsonConvert.DeserializeObject<ReceiverMessageViewModel>(redisData[i]);
                        if (notifyIds.Contains(item.notify_id.ToString()))
                        {
                            item.seen_status = seen_status;
                            item.seen_date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            _redisService.LSet(cacheListKey, i, JsonConvert.SerializeObject(item), dbIndex);
                        }
                    }
                }

                // --- Fallback: nếu notify không có trong Redis thì rebuild lại cache page 1 từ Mongo ---
                if (redisData == null || !redisData.Any())
                {
                    var freshList = await _notifyMongoDAL.getListNotifyPage(user_seen_id, 1, 50);
                    if (freshList != null && freshList.Any())
                    {
                        _redisService.Del(cacheListKey, dbIndex);
                        foreach (var item in freshList)
                        {
                            _redisService.LPush(cacheListKey, JsonConvert.SerializeObject(item), dbIndex);
                        }
                        _redisService.LTrim(cacheListKey, 0, 49, dbIndex);
                    }
                }

                // --- Lấy lại unseen count từ Mongo (chuẩn dữ liệu) ---
                var unseenInfo = await _notifyMongoDAL.GetUnseenNotifyInfo(user_seen_id);

                // --- Build view model ---
                var notifyList = await _notifyMongoDAL.getListNotifyPage(user_seen_id, 1, 20);
                var objNotify = new NotifySummeryViewModel
                {
                    total_not_seen = unseenInfo.total,
                    lst_id_not_seen = string.Join(",", unseenInfo.ids),
                    lst_not_seen_detail = notifyList
                };

                // --- Publish realtime event ---
                foreach (var id in notifyIds)
                {
                    _subscriber.Publish(cacheListKey, JsonConvert.SerializeObject(new
                    {
                        notify_id = id,
                        seen_status = seen_status
                    }));
                }
                _subscriber.Publish(cacheListKey, JsonConvert.SerializeObject(objNotify));

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = objNotify
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateStatusReadNotify error: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Exception: " + ex.Message
                });
            }
        }






    }
}
