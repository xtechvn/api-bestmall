using Azure.Core;
using Caching.Elasticsearch.FlashSale;
using HuloToys_Service.Models.Shipping.ViettelPost;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Models.MongoDb;
using Newtonsoft.Json;
using System.Text;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Shipping.Business
{
    public class ViettelPostService
    {
        public readonly string DOMAIN = "https://partner.viettelpost.vn/v2/";

        private string token_temporary = "";
        private string token_ownerconnect = "";
        private readonly string USERNAME = "0962753455";
        private readonly string PASSWORD = "Lamtutam888";

        private readonly string API_LOGIN = "user/Login";
        private readonly string API_OWNERCONNECT = "user/ownerconnect";
        private readonly string API_GETPRICEALL = "order/getPriceAll";
        private readonly string API_GETPRICE = "order/getPrice";
        private readonly string API_CREATEORDER = "order/createOrder";
        private readonly HttpClient _httpClient;
        private readonly RedisConn _redisService;
        private int exprire_time = 86400;
        private IConfiguration _configuration;
        private readonly SupplierESRepository _supplierESRepository;

        public ViettelPostService(RedisConn redisService, IConfiguration configuration, SupplierESRepository supplierESRepository)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();

            _redisService = redisService;
            try
            {
                _redisService.Connect();
            }
            catch
            {

            }
            _supplierESRepository = supplierESRepository;

        }
        public async Task<bool> GetTemporaryToken()
        {
            try
            {
                try
                {
                    var token = _redisService.Get("ViettelPostToken", Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    if (token != null && token.Trim() != "")
                    {
                        token_temporary = token.Trim();
                        return true;
                    }
                }
                catch { }

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_LOGIN);
                string json_content = "{\"USERNAME\":\"" + USERNAME + "\",\"PASSWORD\":\"" + PASSWORD + "\"}";
                StringContent content = new StringContent(json_content, null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                if (responseBody == null || responseBody.Trim() == "")
                {
                    return false;

                }
                dynamic responseObject = JsonConvert.DeserializeObject(responseBody);

                // Kiểm tra xem token có tồn tại không
                if (responseObject != null && responseObject.data != null && responseObject.data.token != null)
                {
                    token_temporary = responseObject.data.token;
                    LogHelper.InsertLogTelegram("GetTemporaryToken - ViettelPostService: Token= [" + token_temporary + "] [" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "]");
                    try
                    {
                        _redisService.Set("ViettelPostToken", responseObject.data.token, DateTime.Now.AddSeconds(exprire_time), Convert.ToInt32(_configuration["Redis:Database:db_common"]));

                    }
                    catch { }
                    return true;
                }
                else
                {
                    LogHelper.InsertLogTelegram("GetTemporaryToken - ViettelPostService: Token not found on API [" + (DOMAIN + API_LOGIN) + "] [" + json_content + "]");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTemporaryToken - ViettelPostService: error [" + (DOMAIN + API_LOGIN) + "] :" + ex.ToString());
            }
            return false;
        }
        private async Task<bool> GetOwnerConnectToken()
        {
            try
            {
                if (token_temporary == null || token_temporary.Trim() == "")
                {
                    await GetTemporaryToken();
                }
                var request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_OWNERCONNECT);
                request.Headers.Add("Token", token_temporary);
                request.Headers.Add("Cookie", "SERVERID=A");
                string json_content = "{\"USERNAME\":\"" + USERNAME + "\",\"PASSWORD\":\"" + PASSWORD + "\"}";
                StringContent content = new StringContent(json_content, null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                if (responseBody == null || responseBody.Trim() == "")
                {
                    return false;

                }
                dynamic responseObject = JsonConvert.DeserializeObject(responseBody);

                if (responseObject != null && responseObject.data != null && responseObject.data.token != null)
                {
                    token_ownerconnect = responseObject.data.token;
                    return true;
                }
                else
                {
                    LogHelper.InsertLogTelegram("GetOwnerConnectToken - ViettelPostService: Token not found on API [" + (DOMAIN + API_OWNERCONNECT) + "] [" + json_content + "]");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOwnerConnectToken - ViettelPostService: error [" + (DOMAIN + API_OWNERCONNECT) + "] :" + ex.ToString());
            }
            return false;
        }
        public async Task<List<VTPGetPriceAllResponse>> GetShippingMethods(VTPGetPriceAllRequest requestData)
        {
            try
            {
                if (token_temporary == null || token_temporary.Trim() == "")
                {
                    LogHelper.InsertLogTelegram(
                       "Viettelpost GetTemporaryToken: "
                       );
                    await GetTemporaryToken();
                }
                var request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_GETPRICEALL);
                request.Headers.Add("Token", token_temporary);
                request.Headers.Add("Cookie", "SERVERID=A");
                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                LogHelper.InsertLogTelegram(
                        "Viettelpost GetShippingMethods: "
                        + " list_supplier count=" + (responseBody == null ? "NULL" : responseBody)

                        );
                List<VTPGetPriceAllResponse> shippingServices = JsonConvert.DeserializeObject<List<VTPGetPriceAllResponse>>(responseBody);

                return shippingServices;
            }
            catch (HttpRequestException e)
            {
                LogHelper.InsertLogTelegram("GetOwnerConnectToken - GetShippingMethods: HttpCall error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.StatusCode + "][" + e.Message + "]");

                return null;
            }
            catch (JsonException e)
            {
                LogHelper.InsertLogTelegram("GetOwnerConnectToken - GetShippingMethods: JSON parse error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.Message + "]");
                return null;
            }
            catch (Exception e)
            {
                LogHelper.InsertLogTelegram("GetOwnerConnectToken - GetShippingMethods: error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.Message + "]");
                return null;
            }
        }
        public async Task<VTPGetPriceResponse> CalculateShippingPrice(VTPGetPriceRequest requestData)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_GETPRICE);
                request.Headers.Add("Token", token_temporary);
                request.Headers.Add("Cookie", "SERVERID=A");
                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                VTPGetPriceResponse shippingServices = JsonConvert.DeserializeObject<VTPGetPriceResponse>(responseBody);

                return shippingServices;
            }
            catch (HttpRequestException e)
            {
                LogHelper.InsertLogTelegram("GetOwnerConnectToken - CalculateShippingPrice: HttpCall error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.StatusCode + "][" + e.Message + "]");

                return null;
            }
            catch (JsonException e)
            {
                LogHelper.InsertLogTelegram("GetOwnerConnectToken - CalculateShippingPrice: JSON parse error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.Message + "]");
                return null;
            }
            catch (Exception e)
            {
                LogHelper.InsertLogTelegram("GetOwnerConnectToken - CalculateShippingPrice: error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.Message + "]");
                return null;
            }
        }
        public async Task<VTPOrderResponseModel> CreateVTPOrder(VTPOrderRequestModel requestData)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_CREATEORDER);
                request.Headers.Add("Token", token_temporary);
                request.Headers.Add("Cookie", "SERVERID=A");
                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                VTPOrderResponseModel shippingServices = JsonConvert.DeserializeObject<VTPOrderResponseModel>(responseBody);

                return shippingServices;
            }
            catch (HttpRequestException e)
            {
                LogHelper.InsertLogTelegram("CreateVTPOrder - CalculateShippingPrice: HttpCall error [" + (DOMAIN + API_CREATEORDER) + "] [" + e.StatusCode + "][" + e.Message + "]");

                return null;
            }
            catch (JsonException e)
            {
                LogHelper.InsertLogTelegram("CreateVTPOrder - CalculateShippingPrice: JSON parse error [" + (DOMAIN + API_CREATEORDER) + "] [" + e.Message + "]");
                return null;
            }
            catch (Exception e)
            {
                LogHelper.InsertLogTelegram("CreateVTPOrder - CalculateShippingPrice: error [" + (DOMAIN + API_CREATEORDER) + "] [" + e.Message + "]");
                return null;
            }
        }
        public async Task<List<VTPServiceListingResponseModel>> GetShippingFeeByListCart(List<CartItemMongoDbModel> carts, VTPServiceListingRequestModel request)
        {
            List<VTPServiceListingResponseModel> response = new List<VTPServiceListingResponseModel>();

            try
            {
                if (carts != null && carts.Count > 0)
                {
                    var list_supplier = carts.Select(x => x.product.supplier_id).Distinct();
                    bool fill_first_supplier = false;
                    foreach (var supplier in list_supplier)
                    {
                        var cart_belong_to_supplier = carts.Where(x => x.product.supplier_id == supplier);
                        var detail_supplier = await _supplierESRepository.GetById(supplier);
                        //LogHelper.InsertLogTelegram(
                        //      "GetVTPServiceListing "
                        //      + " detail_supplier " + (detail_supplier == null ? "NULL" : detail_supplier.supplierid)

                        //      );
                        int package_weight = 0;
                        //int package_width = 0;
                        //int package_height = 0;
                        //int package_depth = 0;
                        double amount = 0;
                        foreach (var c in cart_belong_to_supplier)
                        {
                            var selected = carts.First(x => x._id == c._id);
                            package_weight += Convert.ToInt32(((c.product.weight <= 0 ? 0 : c.product.weight) * selected.quanity));
                            //package_width += Convert.ToInt32(((c.product.package_width <= 0 ? 0 : c.product.package_width) * selected.quanity));
                            //package_height += Convert.ToInt32(((c.product.package_height <= 0 ? 0 : c.product.package_height) * selected.quanity));
                            //package_depth += Convert.ToInt32(((c.product.package_depth <= 0 ? 0 : c.product.package_depth) * selected.quanity));
                            amount += Convert.ToInt32(((c.product.amount_after_flashsale == null ? c.product.amount : c.product.amount_after_flashsale) * selected.quanity));
                        }
                        //LogHelper.InsertLogTelegram(
                        //        "GetVTPServiceListing "
                        //        + " _viettelPostService " + (_viettelPostService == null ? "NULL" : "_viettelPostService")

                        //        );
                        var response_item = await GetShippingMethods(new VTPGetPriceAllRequest()
                        {
                            MoneyCollection = 0,
                            ProductHeight = 0,
                            ProductLength = 0,
                            ProductPrice = Convert.ToInt64(amount),
                            ProductType = "HH",
                            ProductWeight = package_weight,
                            ProductWidth = 0,
                            SenderDistrict = detail_supplier.districtid == null ? 4 : (int)detail_supplier.districtid,
                            SenderProvince = detail_supplier.provinceid == null ? 1 : (int)detail_supplier.provinceid,
                            ReceiverDistrict = request.receiver_district_id,
                            ReceiverProvince = request.receiver_provinces_id,
                            Type = 1
                        });
                        if (response_item != null && response_item.Count > 0)
                        {
                            if (fill_first_supplier == false)
                            {
                                response.Add(new VTPServiceListingResponseModel()
                                {
                                    supplier_id = 0,
                                    supplier_name = "Giao hàng khả dụng cho tất cả sản phẩm",
                                    cart_ids = carts.Select(x => x._id).ToList(),
                                    services = response_item.Select(x => new VTPServiceListingResponseMethod()
                                    {
                                        name = x.TenDichVu,
                                        service_code = x.MaDvChinh,
                                        total_amount = x.GiaCuoc,
                                        time = x.ThoiGian
                                    }).ToList()
                                });
                                fill_first_supplier = true;
                            }
                            else if (response.Count > 0)
                            {
                                foreach (var delivery in response_item)
                                {
                                    if (response[0].services.Any(x => x.service_code.Trim() == delivery.MaDvChinh.Trim()))
                                    {
                                        response[0].services.First(x => x.service_code.Trim() == delivery.MaDvChinh.Trim()).total_amount += delivery.GiaCuoc;
                                    }
                                    else
                                    {
                                        response[0].services.RemoveAll(x => x.service_code.Trim() == delivery.MaDvChinh.Trim());
                                    }
                                    LogHelper.InsertLogTelegram(" response_item " +
                                             "[" + (detail_supplier.provinceid == null ? 1 : (int)detail_supplier.provinceid) + "]"
                                            + "[" + (detail_supplier.districtid == null ? 4 : (int)detail_supplier.districtid) + "]"
                                            + "[" + request.receiver_provinces_id + "]"
                                            + "[" + request.receiver_district_id + "]"
                                            + "[" + delivery.MaDvChinh.Trim() + "]"
                                            + "[" + delivery.GiaCuoc + "]"
                                            + "[" + package_weight + "]"
                                            + "[" + supplier + "]"
                                             );
                                }

                            }

                        }
                    }
                   
                }
            }
            catch(Exception e)
            {
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], "GetShippingFeeByListCart - ViettelPostService: "+e.ToString());

            }
            return response;
        }
        
    }

    // --- INPUT MODEL ---
    public class VTPGetPriceAllRequest
    {
        [JsonProperty("SENDER_DISTRICT")]
        public int SenderDistrict { get; set; }

        [JsonProperty("SENDER_PROVINCE")]
        public int SenderProvince { get; set; }

        [JsonProperty("RECEIVER_DISTRICT")]
        public int ReceiverDistrict { get; set; }

        [JsonProperty("RECEIVER_PROVINCE")]
        public int ReceiverProvince { get; set; }

        [JsonProperty("PRODUCT_TYPE")]
        public string ProductType { get; set; }

        [JsonProperty("PRODUCT_WEIGHT")]
        public int ProductWeight { get; set; }

        [JsonProperty("PRODUCT_PRICE")]
        public long ProductPrice { get; set; }

        [JsonProperty("MONEY_COLLECTION")]
        public long MoneyCollection { get; set; }

        [JsonProperty("PRODUCT_LENGTH")]
        public int ProductLength { get; set; }

        [JsonProperty("PRODUCT_WIDTH")]
        public int ProductWidth { get; set; }

        [JsonProperty("PRODUCT_HEIGHT")]
        public int ProductHeight { get; set; }

        [JsonProperty("TYPE")]
        public int Type { get; set; }
    }

    // --- OUTPUT MODELS ---
    public class VTPExtraService
    {
        [JsonProperty("SERVICE_CODE")]
        public string ServiceCode { get; set; }

        [JsonProperty("SERVICE_NAME")]
        public string ServiceName { get; set; }

        [JsonProperty("DESCRIPTION")]
        public object Description { get; set; } // Có thể là null, nên dùng object
    }

    public class VTPGetPriceAllResponse
    {
        [JsonProperty("MA_DV_CHINH")]
        public string MaDvChinh { get; set; }

        [JsonProperty("TEN_DICHVU")]
        public string TenDichVu { get; set; }

        [JsonProperty("GIA_CUOC")]
        public int GiaCuoc { get; set; }

        [JsonProperty("THOI_GIAN")]
        public string ThoiGian { get; set; }

        [JsonProperty("EXCHANGE_WEIGHT")]
        public int ExchangeWeight { get; set; }

        [JsonProperty("EXTRA_SERVICE")]
        public List<VTPExtraService> ExtraService { get; set; }
    }
    public class VTPGetPriceRequest
    {
        [JsonProperty("PRODUCT_WEIGHT")]
        public int ProductWeight { get; set; }

        [JsonProperty("PRODUCT_PRICE")]
        public long ProductPrice { get; set; }

        [JsonProperty("MONEY_COLLECTION")]
        public long MoneyCollection { get; set; }

        [JsonProperty("ORDER_SERVICE_ADD")]
        public string OrderServiceAdd { get; set; } // Có thể là chuỗi rỗng

        [JsonProperty("ORDER_SERVICE")]
        public string OrderService { get; set; } // Ví dụ: "VCBO"

        [JsonProperty("SENDER_DISTRICT")]
        public int SenderDistrict { get; set; }

        [JsonProperty("SENDER_PROVINCE")]
        public int SenderProvince { get; set; }

        [JsonProperty("RECEIVER_DISTRICT")]
        public int ReceiverDistrict { get; set; }

        [JsonProperty("RECEIVER_PROVINCE")]
        public int ReceiverProvince { get; set; }

        [JsonProperty("PRODUCT_LENGTH")]
        public int ProductLength { get; set; }

        [JsonProperty("PRODUCT_WIDTH")]
        public int ProductWidth { get; set; }

        [JsonProperty("PRODUCT_HEIGHT")]
        public int ProductHeight { get; set; }

        [JsonProperty("PRODUCT_TYPE")]
        public string ProductType { get; set; }

        [JsonProperty("NATIONAL_TYPE")]
        public int NationalType { get; set; }
    }

    // --- OUTPUT MODELS for getPrice API ---
    public class VTPGetPriceData
    {
        [JsonProperty("MONEY_TOTAL_OLD")]
        public int MoneyTotalOld { get; set; }

        [JsonProperty("MONEY_TOTAL")]
        public int MoneyTotal { get; set; }

        [JsonProperty("MONEY_TOTAL_FEE")]
        public int MoneyTotalFee { get; set; }

        [JsonProperty("MONEY_FEE")]
        public int MoneyFee { get; set; }

        [JsonProperty("MONEY_COLLECTION_FEE")]
        public int MoneyCollectionFee { get; set; }

        [JsonProperty("MONEY_OTHER_FEE")]
        public int MoneyOtherFee { get; set; }

        [JsonProperty("MONEY_VAS")]
        public int MoneyVas { get; set; }

        [JsonProperty("MONEY_VAT")]
        public int MoneyVat { get; set; }

        [JsonProperty("KPI_HT")]
        public double KpiHt { get; set; }
    }

    public class VTPGetPriceResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public VTPGetPriceData Data { get; set; }
    }
    public class VTPOrderRequestListItem
    {
        public string PRODUCT_NAME { get; set; }
        public int PRODUCT_PRICE { get; set; }
        public int PRODUCT_WEIGHT { get; set; }
        public int PRODUCT_QUANTITY { get; set; }
    }

    public class VTPOrderRequestModel
    {
        public string ORDER_NUMBER { get; set; }
        public int GROUPADDRESS_ID { get; set; }
        public int CUS_ID { get; set; }
        public string DELIVERY_DATE { get; set; }
        public string SENDER_FULLNAME { get; set; }
        public string SENDER_ADDRESS { get; set; }
        public string SENDER_PHONE { get; set; }
        public string SENDER_EMAIL { get; set; }
        public int SENDER_WARD { get; set; }
        public int SENDER_DISTRICT { get; set; }
        public int SENDER_PROVINCE { get; set; }
        public int SENDER_LATITUDE { get; set; }
        public int SENDER_LONGITUDE { get; set; }
        public string RECEIVER_FULLNAME { get; set; }
        public string RECEIVER_ADDRESS { get; set; }
        public string RECEIVER_PHONE { get; set; }
        public string RECEIVER_EMAIL { get; set; }
        public int RECEIVER_WARD { get; set; }
        public int RECEIVER_DISTRICT { get; set; }
        public int RECEIVER_PROVINCE { get; set; }
        public int RECEIVER_LATITUDE { get; set; }
        public int RECEIVER_LONGITUDE { get; set; }
        public string PRODUCT_NAME { get; set; }
        public string PRODUCT_DESCRIPTION { get; set; }
        public int PRODUCT_QUANTITY { get; set; }
        public int PRODUCT_PRICE { get; set; }
        public int PRODUCT_WEIGHT { get; set; }
        public int PRODUCT_LENGTH { get; set; }
        public int PRODUCT_WIDTH { get; set; }
        public int PRODUCT_HEIGHT { get; set; }
        public string PRODUCT_TYPE { get; set; }
        public int ORDER_PAYMENT { get; set; }
        public string ORDER_SERVICE { get; set; }
        public string ORDER_SERVICE_ADD { get; set; }
        public string ORDER_VOUCHER { get; set; }
        public string ORDER_NOTE { get; set; }
        public int MONEY_COLLECTION { get; set; }
        public int EXTRA_MONEY { get; set; }
        public bool CHECK_UNIQUE { get; set; }
        public List<VTPOrderRequestListItem> LIST_ITEM { get; set; }
    }
    public class VTPOrderResponseData
    {
        public string ORDER_NUMBER { get; set; }
        public int MONEY_COLLECTION { get; set; }
        public int EXCHANGE_WEIGHT { get; set; }
        public int MONEY_TOTAL { get; set; }
        public int MONEY_TOTAL_FEE { get; set; }
        public int MONEY_FEE { get; set; }
        public int MONEY_COLLECTION_FEE { get; set; }
        public int MONEY_OTHER_FEE { get; set; }
        public int MONEY_VAS { get; set; }
        public int MONEY_VAT { get; set; }
        public double KPI_HT { get; set; }
        public int RECEIVER_PROVINCE { get; set; }
        public int RECEIVER_DISTRICT { get; set; }
        public int RECEIVER_WARDS { get; set; }
    }
    public class VTPOrderResponseModel
    {
        public int status { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        public VTPOrderResponseData data { get; set; }
    }
}
