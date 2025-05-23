using Caching.Elasticsearch;
using Entities.ViewModels.Products;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.Models.ProductsFavourites;
using HuloToys_Service.Models.Raiting;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtpNet;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace WEB.CMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ProductController : ControllerBase
    {
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly ProductSpecificationMongoAccess _productSpecificationMongoAccess;
        private readonly CartMongodbService _cartMongodbService;
        private readonly RaitingESService _raitingESService;
        private readonly OrderDetailESService orderDetailESService;
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisService;
        private readonly GroupProductESService groupProductESService;
        private readonly ProductRaitingService productRaitingService;
        private readonly ProductDetailService productDetailService;
        private readonly ProductESRepository _productESRepository;
        private readonly AttachFileESModelESRepository attachFileESModelESRepository;
        private readonly ProductFavouritesMongoAccess _productFavouritesMongoAccess;
        private readonly ClientServices clientServices;

        public ProductController(IConfiguration configuration, RedisConn redisService)
        {
            _productDetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _productSpecificationMongoAccess = new ProductSpecificationMongoAccess(configuration);
            _productFavouritesMongoAccess = new ProductFavouritesMongoAccess(configuration);
            _cartMongodbService = new CartMongodbService(configuration);
            productRaitingService = new ProductRaitingService(configuration);
            productDetailService = new ProductDetailService(configuration);
            orderDetailESService = new OrderDetailESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            groupProductESService = new GroupProductESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _productESRepository = new ProductESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            attachFileESModelESRepository = new AttachFileESModelESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            clientServices = new ClientServices(configuration);
            _configuration = configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
        }

        [HttpPost("get-list")]
        public async Task<IActionResult> ProductListing([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //input.token = "F081O1oSKR4nJktCB3d5ekEyMysRMQY0LBBoCGN6TgYGUTYtKygpBxF9Xn85";
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductListRequestModel>(objParr[0].ToString());
                    if (request == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    // Kiểm tra các tham số giá
                    if (request.price_from == 0 || request.price_from == null) request.price_from = 0; // Mặc định là 0 nếu không có giá trị
                    if (request.price_to == 0 || request.price_to == null) request.price_to = double.MaxValue; // Mặc định là giá trị tối đa
                    if (request.keyword == null) request.keyword = "";
                    if (request.rating == null) request.rating = 0;


                    if (request.page_size <= 0) request.page_size = 10;
                    if (request.page_index < 1) request.page_index = 1;
                    // Nếu không lọc theo giá, sử dụng cache Redis
                    if (request.price_from == 0 && request.price_to == double.MaxValue && request.rating == 0)
                    {
                        var cache_name = CacheType.PRODUCT_LISTING + (request.keyword ?? "") + request.group_id + request.page_index + request.page_size;
                        var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        ProductListFEResponseModel result = null;
                        if (j_data != null && j_data.Trim() != "")
                        {
                            result = JsonConvert.DeserializeObject<ProductListFEResponseModel>(j_data);

                        }
                        if (result == null || result.items == null || result.items.Count <= 0)
                        {
                            //request.keyword = StringHelpers.NormalizeString(request.keyword);
                            //var data = await productDetailService.ProductListing(request);
                            result = await productDetailService.ProductListing(request);

                        }
                        if (result != null && result.items.Count > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                            var list = result.items.Select(x => new
                            {
                                x._id,
                                x.code,
                                x.name,
                                x.avatar,
                                x.price,
                                x.amount,
                                x.amount_min,
                                x.amount_max,
                                x.rating,
                                x.star,
                                x.total_sold,
                                x.review_count,
                                x.old_price,
                                x.discount
                            });
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = ResponseMessages.Success,
                                data = new
                                {
                                    items = list,
                                    count = result.count
                                }
                            });
                        }
                    }
                    else
                    {
                        // Trường hợp có lọc theo giá, bỏ qua cache Redis và truy vấn trực tiếp cơ sở dữ liệu
                        var result = await productDetailService.ProductListing(request);

                        if (result != null && result.items.Count > 0)
                        {
                            //var filteredItems = result.items.Where(x =>
                            //         // Sản phẩm có khoảng giá rõ ràng
                            //         (x.amount_min.HasValue && x.amount_max.HasValue && x.amount_max >= request.price_from && x.amount_min <= request.price_to)

                            //         // Hoặc sản phẩm chỉ có 1 giá cụ thể
                            //         || (x.amount_min == null && x.amount_max == null && x.amount >= request.price_from && x.amount <= request.price_to)
                            //     ).ToList();
                          var filteredItems = result.items
                              .Where(x =>
                              {
                                  var amountToCheck = (x.amount > 0) ? x.amount : x.amount_max;
                                  return amountToCheck >= request.price_from && amountToCheck <= request.price_to;
                              })
                              .Where(x => x.rating >= request.rating)
                              .ToList();


                            //Lọc theo khoảng giá và rating nếu có


                            //// Phân trang kết quả lọc
                            //var pagedItems = filteredItems.Skip((request.page_index - 1) * request.page_size).Take(request.page_size).ToList();

                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = ResponseMessages.Success,
                                data = new
                                {
                                    items = filteredItems,
                                    count = filteredItems.Count
                                }
                            });
                        }

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "No Items"
                        });
                    }
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
                msg = ResponseMessages.DataInvalid,
            });
        }


        [HttpPost("detail")]
        public async Task<IActionResult> ProductDetail([FromBody] APIRequestGenericModel input)
        {
            //var model_con = new
            //{
            //    id = "682ad9336b5155c27a8bd9d7"
            //};
            //input.token = CommonHelper.Encode(JsonConvert.SerializeObject(model_con), _configuration["KEY:private_key"]);
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductDetailRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    //var cache_name = CacheType.PRODUCT_DETAIL + request.id;
                    //var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    //if (j_data != null && j_data.Trim() != "")
                    //{
                    //    ProductDetailResponseModel result = JsonConvert.DeserializeObject<ProductDetailResponseModel>(j_data);
                    //    if (result != null)
                    //    {
                    //        return Ok(new
                    //        {
                    //            status = (int)ResponseType.SUCCESS,
                    //            msg = ResponseMessages.Success,
                    //            data = result
                    //        });
                    //    }
                    //}
                    var data = await _productDetailMongoAccess.GetFullProductById(request.id);
                    List<string> cert_root = new List<string>();
                    List<string> cert_product = new List<string>();
                    List<string> cert_supply = new List<string>();
                    List<string> cert_confirm = new List<string>();
                    bool favourite = false;
                    if (data != null)
                    {
                        // _redisService.Set(cache_name, JsonConvert.SerializeObject(data), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        var attach_root = await attachFileESModelESRepository.GetByDataidAndType(data.product_main.supplier_id, (int)AttachmentType.Supplier_Cert_RootProduct);
                        var attach_product = await attachFileESModelESRepository.GetByDataidAndType(data.product_main.supplier_id, (int)AttachmentType.Supplier_Cert_Product);
                        var attach_supply = await attachFileESModelESRepository.GetByDataidAndType(data.product_main.supplier_id, (int)AttachmentType.Supplier_Cert_Supply);
                        var attach_confirm = await attachFileESModelESRepository.GetByDataidAndType(data.product_main.supplier_id, (int)AttachmentType.Supplier_Cert_Confirm);
                        if(attach_root != null && attach_root.Count > 0)
                        {
                            cert_root = attach_root.Select(x => x.Path).ToList();
                        }
                        if (attach_product != null && attach_product.Count > 0)
                        {
                            cert_product = attach_product.Select(x => x.Path).ToList();
                        }
                        if (attach_supply != null && attach_supply.Count > 0)
                        {
                            cert_supply = attach_supply.Select(x => x.Path).ToList();
                        }
                        if (attach_confirm != null && attach_confirm.Count > 0)
                        {
                            cert_confirm = attach_confirm.Select(x => x.Path).ToList();
                        }
                    }
                    //favourites:
                    if(request.token!=null && request.token.Trim() != "")
                    {
                        long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                        if (account_client_id >0)
                        {
                            var exists=await _productFavouritesMongoAccess.GetByAccountAndProduct(request.id,account_client_id);
                            if(exists!=null && exists._id!=null && exists._id.Trim() != "")
                            {
                                favourite = true;
                            }

                        }
                    }
                    var count = await _productFavouritesMongoAccess.CountByProductId(request.id);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = data,
                        cert=new
                        {
                            root_product=cert_root,
                            product=cert_product,
                            supply=cert_supply,
                            confirm=cert_confirm
                        },
                        favourite= new
                        {
                            is_favourite=favourite,
                            count= count
                        }
                    });

                }

            }
            catch
            {

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Failed",
            });
        }
        [HttpPost("group-product")]
        public async Task<IActionResult> GroupProduct([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductListRequestModel>(objParr[0].ToString());
                    if (request == null || request.group_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var data = groupProductESService.GetListGroupProductByParentId(request.group_id);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = data
                    });
                }


            }
            catch
            {

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("search")]
        public async Task<IActionResult> ProductSearch([FromBody] APIRequestGenericModel input)
       {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductGlobalSearchRequestModel>(objParr[0].ToString());
                    if (request == null|| request.keyword == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }

                    // ✅ Chuẩn hóa keyword: bỏ ký tự đặc biệt + giữ dấu + normalize cho search
                    string rawKeyword = StringHelper.RemoveSpecialCharacterExceptVietnameseCharacter(request.keyword);
                    string normalizedKeyword = StringHelper.NormalizeKeyword(rawKeyword); // Dùng cho no_space_name
                    ProductListResponseModel data = new ProductListResponseModel();
                    var list = await _productESRepository.SearchByKeywordAsync(rawKeyword, normalizedKeyword);
                    if(list!=null && list.Count > 0)
                    {
                        data.count=list.Count;
                        data.items = list.Select(x => new ProductMongoDbModel()
                        {
                            amount = x.amount,
                            _id = x.product_id,
                            code = x.product_code,
                            description = x.description,
                            name = x.name,
                            avatar=x.avatar,
                        }).ToList();
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = data
                    });
                }
            }
            catch
            {

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        
        [HttpPost("raiting-count")]
        public async Task<IActionResult> ProductRaitingCount([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductRaitingRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    ProductRaitingResponseModel result = _raitingESService.CountCommentByProductId(request.id);
                    List<string> product_ids = new List<string>()
                    {
                        request.id
                    };
                    var product=await _productDetailMongoAccess.SubListing(request.id);
                    if(product!=null && product.Count > 0)
                    {
                        product_ids.AddRange(product.Select(x => x._id));
                    }
                    result.total_sold = orderDetailESService.CountByProductId(product_ids);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = result
                    });
                }
            }
            catch
            {

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("raiting")]
        public async Task<IActionResult> ProductRaiting([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductRaitingRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (request.page_index < 1) request.page_index = 1;
                    if (request.page_size < 1) request.page_size = 5;
                    var data= await productRaitingService.GetListByFilter(request);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = data
                    });
                }
            }
            catch
            {

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("global-search-filter")]
        public async Task<IActionResult> ProductGlobalSearchFilter([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductGlobalSearchRequestModel>(objParr[0].ToString());
                    if (request == null || request.keyword == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var data = await _productDetailMongoAccess.GlobalSearch(request.keyword, 0, "", "", 1, 500);
                    List<ProductSpecificationDetailMongoDbModel> brands = new List<ProductSpecificationDetailMongoDbModel>();
                    List<GroupProductESModel> groups = new List<GroupProductESModel>();
                    ProductListResponseModel items = new ProductListResponseModel();
                    if (data!=null && data.items!=null && data.items.Count > 0)
                    {
                        var value = string.Join(",", data.items.Select(x => x.group_product_id));
                        var ids = value.Split(",").Where(x=>x!=null && x.Trim()!="").Select(x => Convert.ToInt64(x)).ToList();
                        groups =  groupProductESService.GetGroupProductByIDs(ids);
                        brands = data.items.Where(x=>x.specification!=null && x.specification.Count>0).SelectMany(x => x.specification).Where(x=>x.attribute_id==1).Distinct().ToList();
                        brands = brands.Where(x => x.value != null &&x.value != "null" && x.value.Trim() != "").DistinctBy(x => x.value).ToList();
                        string brand_split = string.Join(",", brands.Select(x => x.value));
                        brands = brand_split.Split(",").Distinct().Select(x => new ProductSpecificationDetailMongoDbModel()
                        {
                            attribute_id = 1,
                            value = x,
                            value_type = 1,
                            type_ids = "1",
                            _id = ""
                        }).ToList();
                        items = new ProductListResponseModel()
                        {
                            items = data.items.Take(12).ToList(),
                            count = data.count
                        };
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data= items,
                        brands = brands,
                        groups= groups
                    });
                }
            }
            catch
            {

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("global-search")]
        public async Task<IActionResult> ProductGlobalSearch([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductGlobalSearchRequestModel>(objParr[0].ToString());
                    if (request == null || request.keyword == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (request.page_index == null || request.page_index <= 0) request.page_index = 1;
                    if (request.page_size == null || request.page_size <= 0) request.page_index = 12;
                    var data = await _productDetailMongoAccess.GlobalSearch(request.keyword, request.stars, request.group_product_id, request.brands, (int)request.page_index, (int)request.page_size);
                  
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = data
                    });
                }
            }
            catch
            {

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("list-by-supplier")]
        public async Task<IActionResult> ProductListingBySupplier([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //input.token = "F081O1oSKR4nJktCB3d5ekEyMysRMQY0LBBoCGN6TgYGUTYtKygpBxF9Xn85";
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductListByIdRequestModel>(objParr[0].ToString());
                    if (request == null || request.supplier_id ==null || request.supplier_id<=0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    // Kiểm tra các tham số giá
                    if (request.price_from == 0 || request.price_from == null) request.price_from = 0; // Mặc định là 0 nếu không có giá trị
                    if (request.price_to == 0 || request.price_to == null) request.price_to = double.MaxValue; // Mặc định là giá trị tối đa
                    if (request.keyword == null) request.keyword = "";
                    if (request.rating == null) request.rating = 0;


                    if (request.page_size <= 0) request.page_size = 10;
                    if (request.page_index < 1) request.page_index = 1;
                    // Nếu không lọc theo giá, sử dụng cache Redis
                    if (request.price_from == 0 && request.price_to == double.MaxValue && request.rating == 0)
                    {
                        var cache_name = CacheType.PRODUCT_LISTING + (request.keyword ?? "") + request.group_id + request.supplier_id + request.page_index + request.page_size;
                        var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        ProductListFEResponseModel result = null;
                        if (j_data != null && j_data.Trim() != "")
                        {
                            result = JsonConvert.DeserializeObject<ProductListFEResponseModel>(j_data);

                        }
                        if (result == null || result.items == null || result.items.Count <= 0)
                        {
                            result = await productDetailService.ProductListingByLabelAndSupplier(request);
                        }
                        if (result != null && result.items.Count > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                            var list = result.items.Select(x => new
                            {
                                x._id,
                                x.code,
                                x.name,
                                x.avatar,
                                x.price,
                                x.amount,
                                x.amount_min,
                                x.amount_max,
                                x.rating,
                                x.star,
                                x.total_sold,
                                x.review_count,
                                x.old_price,
                                x.discount
                            });
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = ResponseMessages.Success,
                                data = new
                                {
                                    items = list,
                                    count = result.count
                                }
                            });
                        }
                    }
                    else
                    {
                        // Trường hợp có lọc theo giá, bỏ qua cache Redis và truy vấn trực tiếp cơ sở dữ liệu
                        var result = await productDetailService.ProductListing(request);

                        if (result != null && result.items.Count > 0)
                        {
                            //var filteredItems = result.items.Where(x =>
                            //         // Sản phẩm có khoảng giá rõ ràng
                            //         (x.amount_min.HasValue && x.amount_max.HasValue && x.amount_max >= request.price_from && x.amount_min <= request.price_to)

                            //         // Hoặc sản phẩm chỉ có 1 giá cụ thể
                            //         || (x.amount_min == null && x.amount_max == null && x.amount >= request.price_from && x.amount <= request.price_to)
                            //     ).ToList();
                            var filteredItems = result.items
                            .Where(x =>
                                (x.amount_min.HasValue && x.amount_max.HasValue && x.amount_max >= request.price_from && x.amount_min <= request.price_to)
                                || (x.amount >= request.price_from && x.amount <= request.price_to))
                            .Where(x => x.rating >= request.rating) // ✅ Lọc theo rating
                            .ToList();


                            //Lọc theo khoảng giá và rating nếu có


                            //// Phân trang kết quả lọc
                            //var pagedItems = filteredItems.Skip((request.page_index - 1) * request.page_size).Take(request.page_size).ToList();

                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = ResponseMessages.Success,
                                data = new
                                {
                                    items = filteredItems,
                                    count = filteredItems.Count
                                }
                            });
                        }

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "No Items"
                        });
                    }
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
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("list-by-label")]
        public async Task<IActionResult> ProductListingByLabel([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //input.token = "F081O1oSKR4nJktCB3d5ekEyMysRMQY0LBBoCGN6TgYGUTYtKygpBxF9Xn85";
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductListByIdRequestModel>(objParr[0].ToString());
                    if (request == null || request.label_id == null || request.label_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    // Kiểm tra các tham số giá
                    if (request.price_from == 0 || request.price_from == null) request.price_from = 0; // Mặc định là 0 nếu không có giá trị
                    if (request.price_to == 0 || request.price_to == null) request.price_to = double.MaxValue; // Mặc định là giá trị tối đa
                    if (request.keyword == null) request.keyword = "";
                    if (request.rating == null) request.rating = 0;


                    if (request.page_size <= 0) request.page_size = 10;
                    if (request.page_index < 1) request.page_index = 1;
                    // Nếu không lọc theo giá, sử dụng cache Redis
                    if (request.price_from == 0 && request.price_to == double.MaxValue && request.rating == 0)
                    {
                        var cache_name = CacheType.PRODUCT_LISTING + (request.keyword ?? "") + request.group_id + request.label_id + request.page_index + request.page_size;
                        var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        ProductListFEResponseModel result = null;
                        if (j_data != null && j_data.Trim() != "")
                        {
                            result = JsonConvert.DeserializeObject<ProductListFEResponseModel>(j_data);

                        }
                        if (result == null || result.items == null || result.items.Count <= 0)
                        {
                            result = await productDetailService.ProductListingByLabelAndSupplier(request);

                        }
                        if (result != null && result.items.Count > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                            var list = result.items.Select(x => new
                            {
                                x._id,
                                x.code,
                                x.name,
                                x.avatar,
                                x.price,
                                x.amount,
                                x.amount_min,
                                x.amount_max,
                                x.rating,
                                x.star,
                                x.total_sold,
                                x.review_count,
                                x.old_price,
                                x.discount
                            });
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = ResponseMessages.Success,
                                data = new
                                {
                                    items = list,
                                    count = result.count
                                }
                            });
                        }
                    }
                    else
                    {
                        // Trường hợp có lọc theo giá, bỏ qua cache Redis và truy vấn trực tiếp cơ sở dữ liệu
                        var result = await productDetailService.ProductListing(request);

                        if (result != null && result.items.Count > 0)
                        {
                            //var filteredItems = result.items.Where(x =>
                            //         // Sản phẩm có khoảng giá rõ ràng
                            //         (x.amount_min.HasValue && x.amount_max.HasValue && x.amount_max >= request.price_from && x.amount_min <= request.price_to)

                            //         // Hoặc sản phẩm chỉ có 1 giá cụ thể
                            //         || (x.amount_min == null && x.amount_max == null && x.amount >= request.price_from && x.amount <= request.price_to)
                            //     ).ToList();
                            var filteredItems = result.items
                            .Where(x =>
                                (x.amount_min.HasValue && x.amount_max.HasValue && x.amount_max >= request.price_from && x.amount_min <= request.price_to)
                                || (x.amount >= request.price_from && x.amount <= request.price_to))
                            .Where(x => x.rating >= request.rating) // ✅ Lọc theo rating
                            .ToList();


                            //Lọc theo khoảng giá và rating nếu có


                            //// Phân trang kết quả lọc
                            //var pagedItems = filteredItems.Skip((request.page_index - 1) * request.page_size).Take(request.page_size).ToList();

                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = ResponseMessages.Success,
                                data = new
                                {
                                    items = filteredItems,
                                    count = filteredItems.Count
                                }
                            });
                        }

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "No Items"
                        });
                    }
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
                msg = ResponseMessages.DataInvalid,
            });
        }

        [HttpPost("favourites/listing")]
        public async Task<IActionResult> ProductFavouritesListing([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //input.token = "F081O1oSKR4nJktCB3d5ekEyMysRMQY0LBBoCGN6TgYGUTYtKygpBxF9Xn85";
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductFavouritesListRequestModel>(objParr[0].ToString());
                    if (request == null || request.token==null || request.token.Trim()=="")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (request.page_size <= 0) request.page_size = 50;
                    if (request.page_index < 1) request.page_index = 1;
                    int skip = (request.page_index - 1) * request.page_size;
                    ProductsFavouritesListingResponseModel result = null;
                    //var cache_name = CacheType.PRODUCT_FAVOURITES +  request.user_id;
                    //var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    //if (j_data != null && j_data.Trim() != "")
                    //{
                    //    result = JsonConvert.DeserializeObject<ProductsFavouritesListingResponseModel>(j_data);

                    //}
                   // if (result == null ||result.items == null || result.items.Count <= 0)
                   // {
                        result = await _productFavouritesMongoAccess.Listing(account_client_id);
                   // }
                    if (result != null && result.items !=null && result.items.Count > 0)
                    {
                        //_redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        result.items = (request.page_size * request.page_index) >= result.count ? result.items.Skip(skip).Take(request.page_size).ToList() : new List<ProductsFavouritesMongoDbModel>();
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = ResponseMessages.Success,
                            data = result.items,
                            total=result.count
                        });
                    }
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
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("favourites/insert")]
        public async Task<IActionResult> ProductFavouritesInsert([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //input.token = "F081O1oSKR4nJktCB3d5ekEyMysRMQY0LBBoCGN6TgYGUTYtKygpBxF9Xn85";
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductFavouritesInsertRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == "" || request.product_id==null || request.product_id.Trim() =="")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var detail = await _productDetailMongoAccess.GetByID(request.product_id);
                    if (detail == null|| detail._id == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var id = await _productFavouritesMongoAccess.AddNewAsync(new ProductsFavouritesMongoDbModel()
                    {
                        product_id=request.product_id,
                        updated_last=DateTime.Now,
                        account_client_id = account_client_id,
                         detail= detail,
                    });
                   // var cache_name = CacheType.PRODUCT_FAVOURITES + request.user_id;
                  //  _redisService.clear(cache_name,  Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = id
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
                msg = ResponseMessages.DataInvalid,
            });
        }
        [HttpPost("favourites/delete")]
        public async Task<IActionResult> ProductFavouritesDelete([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //input.token = "F081O1oSKR4nJktCB3d5ekEyMysRMQY0LBBoCGN6TgYGUTYtKygpBxF9Xn85";
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<ProductFavouritesInsertRequestModel>(objParr[0].ToString());
                    if (request == null || request.token == null || request.token.Trim() == "" || request.product_id == null || request.product_id.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                    if (account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var detail = await _productDetailMongoAccess.GetByID(request.product_id);
                    if (detail == null || detail._id == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var id = await _productFavouritesMongoAccess.DeleteAsync(account_client_id, request.product_id);
                   // var cache_name = CacheType.PRODUCT_FAVOURITES + request.user_id;
                    //_redisService.clear(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = id
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
                msg = ResponseMessages.DataInvalid,
            });
        }
    }

}