using Caching.Elasticsearch;
using ENTITIES.ViewModels.Voucher;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Voucher;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories;
using Utilities;
using Utilities.Contants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API_CORE.Controllers.VOUCHER
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly OrderESService orderESService;
        private readonly IVoucherRepository voucherRepository;
        private readonly RedisConn redisService;
        private readonly ClientServices clientServices;
        private readonly ClientESService clientESService;
        private readonly AccountClientESService accountClientESService;

        public VoucherController(IConfiguration _Configuration, IVoucherRepository _VoucherRepository, RedisConn _redisService)
        {
            configuration = _Configuration;
            voucherRepository = _VoucherRepository;
            orderESService = new OrderESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            redisService = _redisService;
            redisService = new RedisConn(_Configuration);
            redisService.Connect();
            clientServices = new ClientServices(configuration);
            clientESService = new ClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            accountClientESService = new AccountClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);

        }

        /// <summary>
        /// Hàm này sẽ lấy ra số tiền được giảm của Voucher
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>


        [HttpPost("apply.json")]
        public async Task<IActionResult> ApplyVoucher([FromBody] APIRequestGenericModel input)
        {
            JArray objParr = null;
            bool is_voucher_valid = false;
            int voucher_id = -1;
            double percent_decrease = 0;
            try
            {
                #region Giả lập test
                var j_param = new Dictionary<string, string>
                {
                        {"voucher_name", "KSWDPQ"}, // mã voucher: truyền động từ fe
                        {"token","172" }, // token đăng nhập
                        {"product_id","172" }, // hotel id được áp mã. Truyền động lấy từ thông tin khách sạn muốn áp mã
                        {"total_order_amount_before","1000000" }, // Tổn giá trị đơn hàng
                        {"total_shipping_fee_before","130000" }, // shipping_fee
                       
                };
                var data_product = JsonConvert.SerializeObject(j_param);
                // token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion
                if(input==null || input.token==null || input.token.Trim() == "")
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Token invalid !!!" });

                }

                if (!CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucherB2B Token invalid!!! => token= " + input.token.ToString() + " voucher name = " + objParr.ToString());
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Token invalid !!!" });
                }
                else
                {

                    string voucher_name = objParr[0]["voucher_name"].ToString(); // tên voucher
                    if (string.IsNullOrEmpty(voucher_name))
                    {
                        return Ok(new { status = (int)ResponseType.EMPTY, msg = "Mã voucher không được để trống" });
                    }
                    //-- Get Token để lấy AccountClientId
                    string token_user = objParr[0]["token"].ToString(); //token đăng nhập
                    if (string.IsNullOrEmpty(token_user))
                    {
                        return Ok(new { status = (int)ResponseType.EMPTY, msg = ResponseMessages.DataInvalid });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(token_user);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }

                   // string product_id = objParr[0]["product_id"].ToString(); //hotel id được áp mã

                    double total_order_amount_before = Convert.ToDouble(objParr[0]["total_order_amount_before"].ToString()); // tổng giá trị đơn hàng trước giảm
                    double total_shipping_fee_before = Convert.ToDouble(objParr[0]["total_shipping_fee_before"].ToString()); // tổng giá trị đơn hàng trước giảm
                   
                    double total_order_amount_after = 0; // tổng giá trị đơn hàng sau giảm
                    double total_discount = 0; // Số tiền được giảm
                    List<VoucherApplyRequestAmountSupplier> amount_by_supplier = new List<VoucherApplyRequestAmountSupplier>();
                    if (objParr[0]["amount_by_supplier"] != null && objParr[0]["amount_by_supplier"].ToString() != null)
                    {
                        amount_by_supplier = JsonConvert.DeserializeObject<List<VoucherApplyRequestAmountSupplier>>(JsonConvert.SerializeObject(objParr[0]["amount_by_supplier"]));

                    }
                    double total_amount_by_supplier_before = 0;
                    double total_amount_by_supplier_after = 0;
                    double total_shipping_fee_after = 0;
                    var client = clientESService.GetById(account_client_id);
                    string email_user_current = client != null ? client.Email : ""; 

                    #region VALIDATION



                    ////1. Check hợp lệ
                    //if (voucher_name.Length < 3 /*&& email_user_current.IndexOf("@") == -1*/)
                    //{
                    //    return Ok(new { status = (int)ResponseType.EXISTS, msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    //}

                    //2. Check null
                    var voucher = await voucherRepository.getDetailVoucher(voucher_name);
                    if (voucher == null || voucher.Id <= 0)
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }
                    //3. Check hệ thống
                    //if (Convert.ToInt16(SourcePaymentType.b2b) != voucher.ProjectType)
                    //{
                    //    return Ok(new { status = (int)ResponseType.EXISTS, msg = "Mã " + voucher_name + " không nằm trong chương trình của hệ thống b2b. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    //}

                    if (voucher == null)
                    {
                        LogHelper.InsertLogTelegram("Code " + voucher_name + " không hợp lệ. voucher == null");

                        return Ok(new { status = (int)ResponseType.EXISTS, msg = "Mã " + voucher_name + " không tồn tại trong hệ thống. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    // 3. Hiệu lực voucher               
                    DateTime current_date = DateTime.Now;
                    if (!(current_date >= voucher.Cdate && current_date <= voucher.EDate))
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " đã hết hiệu lực. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    // 4. Kiểm tra nhóm khách hàng thỏa mãn voucher
                    List<string> group_list_user = null;
                    if (voucher != null && voucher.GroupUserPriority != null && voucher.GroupUserPriority.Trim() != "")
                    {
                        try
                        {
                            group_list_user = JsonConvert.DeserializeObject<List<string>>(voucher.GroupUserPriority);
                        }
                        catch { }
                    }
                    //1 Kiểm tra user đăng nhập có nằm trong nhóm user này không                       
                    if (group_list_user != null && group_list_user.Count()>0)
                    {
                        var find_email = group_list_user.FirstOrDefault(x => x.ToLower().Trim()== email_user_current.ToLower().Trim());

                        if (find_email==null|| find_email.Trim()=="")
                        {
                            LogHelper.InsertLogTelegram("Mã " + voucher_name + " không hợp lệ. Do email " + email_user_current + " không nằm trong danh sách được hưởng khuyến mãi");
                            return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }

                    //5. Kiểm tra voucher này có được giới hạn nhãn hàng không
                    //if (voucher.StoreApply != null)
                    //{
                    //    if (voucher.StoreApply != "-1")
                    //    {
                    //        // Kiểm tra store mã voucher này có nằm trong store cart thanh toán không ?
                    //      //  string store_current_cart = "," + product_id + ",";
                    //        string store_apply_voucher = "," + voucher.StoreApply + ",";
                    //        if (store_apply_voucher.IndexOf(store_current_cart) == -1)
                    //        {

                    //            return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " không áp dụng cho sản phẩm này. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    //        }
                    //    }
                    //}
                    #endregion

                    //Thưc hiện Apply rule theo type               

                    //1. Phí được giảm giá bao gồm:                                           

                    double limit_total_discount = voucher.LimitTotalDiscount ?? 10000; // Số tiền tối đa được giảm lay tu db

                    #region VALIDATION VOUCHER

                    // Nếu voucher được set is_limit_voucher = 1 (true) nghĩa là voucher sẽ được giới hạn số lần sử dụng ở trường limituser
                    // Nếu is_limit_voucher  = 0 (false) thì sẽ hiểu là: mỗi 1 tài khoản sẽ được giới hạn số lần sử dụng ở trường limituser
                    //if (voucher.RuleType != VoucherRuleType.AMZ_DISCOUNT_FPF)
                    //{
                    //if (voucher.IsLimitVoucher == true)
                    //{
                    //    var total_used =  orderESService.CountOrdersByVoucherIdAndClientId(voucher.Id,-1); // Lay  ra so lan voucher da duoc su dung
                    //    if (total_used == -1)
                    //    {
                    //        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    //    }
                    //    else if (total_used >= voucher.LimitUse)
                    //    {
                    //        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    //    }
                    //}
                    //else
                    //{
                    //    var total_client_use =  orderESService.CountOrdersByVoucherIdAndClientId(voucher.Id, account_client_id); // lay ra so lan voucher da duoc su dung cua 1 user
                    //    if (total_client_use >= voucher.LimitUse)
                    //    {
                    //        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng với tài khoản của bạn. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    //    }
                    //}
                    if(voucher.LimitUse <= 0)
                    {
                        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }
                    double total_amount_calculate = 0;
                    switch (voucher.RuleType)
                    {
                        case 0: // Giảm giá trên tiền hàng
                            {
                                total_amount_calculate = total_order_amount_before;
                            }
                            break;
                        case 1: // Giảm giá trên phí ship
                            {
                                total_amount_calculate = total_shipping_fee_before;
                            }
                            break;
                        case 2: // Giảm giá trên NCC
                            {
                                if (voucher.CampaignId != null && voucher.CampaignId > 0 && amount_by_supplier != null && amount_by_supplier.Count > 0)
                                {
                                    var selected = amount_by_supplier.FirstOrDefault(x => x.supplier_id == voucher.CampaignId);
                                    if (selected != null)
                                    {
                                        total_amount_calculate = selected.total_amount;
                                        total_amount_by_supplier_before = selected.total_amount;
                                    }
                                }
                            }
                            break;
                    }
                    // Kiểm tra giới hạn số tiền của đơn hàng
                    if (voucher.MinTotalAmount > 0)
                    {
                        if (total_amount_calculate < voucher.MinTotalAmount)
                        {
                            string _msg = "Để sử dụng mã này.Tổng giá trị đơn hàng/ Số tiền vận chuyển của bạn phải trên " + (voucher.MinTotalAmount ?? 1000000).ToString("N0") + " đ";
                            return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = _msg + ". Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }
                    #endregion

                    //1. Chiết khấu trên phí mua hộ đã trừ. Tính ra số tiền sau khi được trừ    
                    //double percent = Convert.ToDouble(voucher.PriceSales); // Giá trị giảm của voucher. Có thể là % hoặc vnđ                  
                    //switch (voucher.Unit)
                    //{
                    //    case UnitVoucherType.PHAN_TRAM:
                    //        //Tinh số tiền giảm theo %
                    //        total_discount = total_order_amount_before * Convert.ToDouble(voucher.PriceSales / 100);  //Convert.ToDouble(total_fee_not_luxury * (percent / 100)) * rate_current; // so tien duoc giam tu  theo don vi %
                    //        percent_decrease = Convert.ToDouble(voucher.PriceSales);
                    //        break;
                    //    case UnitVoucherType.VIET_NAM_DONG:
                    //        total_discount = Convert.ToDouble(voucher.PriceSales); //Math.Min(Convert.ToDouble(voucher.LimitTotalDiscount), total_fee_not_luxury) ;
                    //        break;

                    //    default:
                    //        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    //}
                    double voucher_discount = 0;
                    double percent = Convert.ToDouble(voucher.PriceSales);
                    
                    if (total_amount_calculate > 0)
                    {
                        switch (voucher.Unit)
                        {
                            case "percent":
                                total_discount += (total_amount_calculate * Convert.ToDouble(percent / 100));
                                break;
                            case "vnd":
                                total_discount += percent;
                                break;

                            default: break;
                        }
                        switch (voucher.RuleType)
                        {
                            case 0: // Giảm giá trên tiền hàng
                                {
                                    total_order_amount_after = total_order_amount_before - total_discount;
                                }
                                break;
                            case 1: // Giảm giá trên phí ship
                                {
                                    total_shipping_fee_after = total_shipping_fee_before - total_discount;
                                }
                                break;
                            case 2: // Giảm giá trên NCC
                                {
                                    total_amount_by_supplier_after= total_amount_by_supplier_before - total_discount;
                                }
                                break;
                        }
                    }

                    //-- limit voucher
                    if (voucher.IsLimitVoucher==true&& voucher.LimitTotalDiscount != null && (double)voucher.LimitTotalDiscount < total_discount) {
                        total_discount = (double)voucher.LimitTotalDiscount;

                    }

                    total_order_amount_after = total_order_amount_before - total_discount;
                    if (total_order_amount_after > 0)
                    {
                        is_voucher_valid = true; // ghi nhan trang thai hop le cho voucher
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher  b2c: Số tiền giảm k hợp lệ, token = " + input.token + "--discount = " + total_discount);
                        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " không hợp lệ, tổng tiền phải > 200.000 đồng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    return Ok(new
                    {
                        status = is_voucher_valid ? ((int)ResponseType.SUCCESS).ToString() : ((int)ResponseType.FAILED).ToString(),
                        msg = "success",
                        voucher_id = voucher.Id,
                        percent_decrease = percent_decrease,
                        expire_date = (voucher.EDate ?? DateTime.Now).ToString("dd-MM-yyyy"),
                        voucher_name = voucher.Code,
                        total_order_amount_before,
                        discount = Math.Round(total_discount),
                        total_order_amount_after = total_order_amount_after,
                        value = Convert.ToDouble(voucher.PriceSales),
                        type = voucher.Unit,
                        rule_type=voucher.RuleType,
                        total_shipping_fee_before,
                        total_shipping_fee_after,
                        total_amount_by_supplier_before,
                        total_amount_by_supplier_after

                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher ex =  " + ex.ToString() + " token=" + input.token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Token invalid !!!" });
            }
        }
        [HttpPost("get-list")]
        public async Task<IActionResult> GetListVoucher([FromBody] APIRequestGenericModel input)
        {
            JArray objParr = null;
            
            try
            {
                #region Giả lập test
                //var j_param = new Dictionary<string, string>
                //{
                //        {"token", "F08nOlAVBi8vLwxaDGMgagRjYX97aVlkfFt7AmJnTlpFXyNQYmNiUgBpXnt3Q1BJUlZ0WE5BcCxNFysoPCdLQhRzZWoEfmR5Y2pZBHhfcAFnbFhPSQNlQ21wYFppZRI="},
                //};
                //input=new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), configuration["KEY:private_key"])
                //};
                #endregion
                if (input == null || input.token == null || input.token.Trim() == "")
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Token invalid !!!" });

                }

                if (!CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    LogHelper.InsertLogTelegram("[API] VoucherController - GetListVoucher Token invalid!!! => token= " + input.token.ToString() + " voucher name = " + objParr.ToString());
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Token invalid !!!" });
                }

                //-- Get Token để lấy AccountClientId
                string token_user = objParr[0]["token"].ToString(); //token đăng nhập
                if (string.IsNullOrEmpty(token_user))
                {
                    return Ok(new { status = (int)ResponseType.EMPTY, msg = ResponseMessages.DataInvalid });
                }
                long account_client_id = await clientServices.GetAccountClientIdFromToken(token_user);
                if (account_client_id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = ResponseMessages.DataInvalid
                    });
                }
                var account_client = accountClientESService.GetById(account_client_id);
                var client = clientESService.GetById((long)account_client.ClientId);
                List<VoucherFEModel> list = new List<VoucherFEModel>();
                List<VoucherFEModel> list_global = new List<VoucherFEModel>();

                // -- Read from Cache - global:
                string cache_name = CacheType.VOUCHER;
                var str = redisService.Get(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"]));
                if (str != null && str.Trim() != "")
                {
                    try { list_global = JsonConvert.DeserializeObject<List<VoucherFEModel>>(str); } catch { }
                }

                // -- Read from Cache - by client_id:
                cache_name = CacheType.VOUCHER + (long)account_client.ClientId;
                str = redisService.Get(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"]));
                if (str != null && str.Trim() != "")
                {
                    try
                    {
                        list = JsonConvert.DeserializeObject<List<VoucherFEModel>>(str);
                    }
                    catch { }
                }

                //-- db - global:
                if (list_global == null || list_global.Count <= 0)
                {
                    list_global = await voucherRepository.GetVoucherList(null, 1, 1, 50, null);
                   // list_global = list_global.Where(x => (x.group_user_priority == null || x.group_user_priority.Trim() == ""|| x.group_user_priority.Trim() == "[]") && x.limitUse > 0).ToList();
                    if (list_global != null && list_global.Count > 0)
                    {
                        int db_index = Convert.ToInt32(configuration["Redis:Database:db_search_result"].ToString());
                        cache_name = CacheType.VOUCHER;
                        redisService.Set(cache_name, JsonConvert.SerializeObject(list_global), db_index);

                    }
                }
                //-- db - by client_id:
                if (list == null || list.Count <= 0)
                {
                    list = await voucherRepository.GetVoucherList(null, 1, 1, 100, (long)account_client.ClientId);
                   // list = list.Where(x => x.group_user_priority != null && x.group_user_priority.Trim() != "" && x.limitUse > 0).ToList();
                    if (list != null && list.Count > 0)
                    {
                        cache_name = CacheType.VOUCHER + (long)account_client.ClientId;
                        int db_index = Convert.ToInt32(configuration["Redis:Database:db_search_result"].ToString());
                        redisService.Set(cache_name, JsonConvert.SerializeObject(list_global), db_index);

                    }
                }

                var mergedList = new List<VoucherFEModel>();
                if (list != null && list.Count > 0)
                {
                    mergedList.AddRange(list);
                }
                if (list_global != null && list_global.Count > 0)
                {
                    mergedList.AddRange(list_global);
                }
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "success",
                    data = mergedList
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher ex =  " + ex.ToString() + " token=" + input.token.ToString());
            }
            return Ok(new { status = (int)ResponseType.FAILED, msg = "Không tìm thấy dữ liệu" });

        }
    }
}
