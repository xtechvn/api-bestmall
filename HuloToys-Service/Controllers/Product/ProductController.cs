using Azure.Core;
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Entities.ViewModels.Products;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Client.Business;
using HuloToys_Service.Controllers.Flashsale.Bussiness;
using HuloToys_Service.Controllers.News.Business;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.ProductsFavourites;
using HuloToys_Service.Models.Raiting;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.constants.Product;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtpNet;
using Repositories.IRepositories;
using System.Drawing.Printing;
using System.Reflection;
using System.Xml.Linq;
using Utilities;
using Utilities.Contants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WEB.CMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProductController : ControllerBase
    {
        //private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly ProductSpecificationMongoAccess _productSpecificationMongoAccess;
        private readonly CartMongodbService _cartMongodbService;
        private readonly RaitingESService _raitingESService;
        private readonly OrderDetailESService orderDetailESService;
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisService;
        private readonly GroupProductESService groupProductESService;
        private readonly ProductRaitingService productRaitingService;
        private readonly ProductDetailService _productDetailService;
        private readonly ProductESRepository _productESRepository;
        private readonly AttachFileESModelESRepository attachFileESModelESRepository;
        private readonly ProductFavouritesMongoAccess _productFavouritesMongoAccess;
        private readonly ClientServices clientServices;
		private readonly NewsBusiness _newsBusiness;
        private readonly DataMSContext _dbContext;
		private readonly ILabelRepository _labelRepository;
        private readonly FlashSaleESRepository flashSaleESRepository;
        private readonly FlashSaleProductESRepository flashSaleProductESRepository;
        private readonly FlashsaleService flashsaleService;
        public ProductController(IConfiguration configuration, RedisConn redisService, ILabelRepository labelRepository, DataMSContext dbContext, ProductRaitingService _productRaitingService
            , ProductDetailService productDetailService/*, ProductDetailMongoAccess productDetailMongoAccess*/, ProductFavouritesMongoAccess productFavouritesMongoAccess)
        {
            //_productDetailMongoAccess = productDetailMongoAccess;
            _productSpecificationMongoAccess = new ProductSpecificationMongoAccess(configuration);
            _productFavouritesMongoAccess = productFavouritesMongoAccess;
            _cartMongodbService = new CartMongodbService(configuration);
            _productDetailService = productDetailService;
            orderDetailESService = new OrderDetailESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            groupProductESService = new GroupProductESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _productESRepository = new ProductESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            attachFileESModelESRepository = new AttachFileESModelESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            clientServices = new ClientServices(configuration);
            _configuration = configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            _newsBusiness = new NewsBusiness(configuration, dbContext);
            _labelRepository = labelRepository;
            productRaitingService=_productRaitingService;

        }

        [HttpPost("get-list")]
        public async Task<IActionResult> ProductListing([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //input.token = "F081O1oSKR4nJktCB3d5ekEyMysRMQY0LBBoCGN6TgYGUTYtKygpBxF9Xn85";
                //var model_input = new
                //{
                //    group_id = -1,
                //    page_index = 1,
                //    page_size = 10
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(model_input), _configuration["KEY:private_key"])
                //};
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
                    ProductListResponseFEModel result = null;
                    var cache_name = CacheType.PRODUCT_LISTING + (request.keyword ?? "") + request.group_id + request.page_index + request.page_size;
                    // Kiểm tra các tham số giá
                    if (request.group_id <= 0) request.group_id = -1; // Mặc định là 0 nếu không có giá trị
                    if (request.price_from <= 0 || request.price_from == null) request.price_from = 0; // Mặc định là 0 nếu không có giá trị
                    if (request.price_to <= 0 || request.price_to == null) request.price_to = 0; // Mặc định là giá trị tối đa
                    if (request.keyword == null) request.keyword = "";
                    if (request.page_size <= 0) request.page_size = 10;
                    if (request.page_index < 1) request.page_index = 1;
                    var page_index=request.page_index;
                    var page_size=request.page_size;
                    int skip = (request.page_index - 1) * request.page_size;
                    int size = request.page_size;
                    // Nếu không lọc theo giá, sử dụng cache Redis
                    if (request.price_from <= 0 && request.price_to <= 0)
                    {
                        var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        if (j_data != null && j_data.Trim() != "")
                        {
                            result = JsonConvert.DeserializeObject<ProductListResponseFEModel>(j_data);
                           
                        }
                        
                    }
                    if ((result == null || result.items == null || result.items.Count <= 0) )
                    {
                        result = await _productDetailService.ProductListing(request);
                        //var expire_time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 4, 0, 0);
                        _redisService.Set(cache_name, JsonConvert.SerializeObject(result)/*, expire_time.AddDays(1)*/, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        //var product_filtered = _productDetailService.FilterProducts(result.items,request.price_from,request.price_to, page_index, page_size);
                        //if(product_filtered == null || product_filtered.Count <= 0)
                        //{
                        //    result.items=result.items.Skip(skip).Take(size).ToList();
                        //}
                        //else
                        //{
                        //    result.items = product_filtered;
                        //}
                    }
                    if (result != null && result.items !=null&& result.items.Count > 0)
                    {
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
                            x.discount,
                            x.exists_flashsale_id,
                            x.exists_flashsale_name,
                            x.amount_after_flashsale,
                            x.flash_sale_fromdate,
                            x.flash_sale_todate,
                            x.flashsale_badge_type
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
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Không có dữ liệu"
                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.DataInvalid
                });
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
            //    id = "682551b6711071e30c18bae5"
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
                    ProductDetailResponseModel result = new ProductDetailResponseModel();
                    Label label = new Label();
                    var cache_name = CacheType.PRODUCT_DETAIL + request.id;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    if (j_data != null && j_data.Trim() != "")
                    {
                        result = JsonConvert.DeserializeObject<ProductDetailResponseModel>(j_data);
                        //--Get Label:
                        if (result!=null && result.product_main != null && result.product_main.label_id > 0)
                        {
                            var cache_name_label = CacheType.LABEL + result.product_main.label_id;

                            var j_data_label = await _redisService.GetAsync(cache_name_label, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                            if (j_data_label != null && j_data_label.Trim() != "")
                            {
                                label = JsonConvert.DeserializeObject<Label>(j_data_label);
                            }
                            else
                            {
                                label = await _labelRepository.GetById(result.product_main.label_id);
                                if (label != null && label.Id > 0)
                                {
                                    _redisService.Set(cache_name_label, JsonConvert.SerializeObject(label), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));

                                }
                            }
                        }
                        if (result != null)
                        {
                            result = await _productDetailService.UpdateFullProductById(result);

                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Success",
                                data = new
                                {
                                    product_main = result.product_main,
                                    product_sub = result.product_sub
                                },
                                cert = result.cert,
                                favourite = result.favourite,
                                buywith = result.product_buy_with_output,
                                label_detail = label==null? null: new
                                {
                                    label.Id,
                                    label.LabelName,
                                    label.LabelCode,
                                    label.Icon,
                                    label.Banner,
                                    label.Description,
                                },
                                groups = (result.groups == null || result.groups.Count<=0) ? null : result.groups.Select(x=> new {
                                    x.Id,
                                    x.ParentId,
                                    x.ImagePath,
                                    x.Name
                                })
                            });
                        }
                    }
                    result  = await _productDetailService.GetFullProductById(request.id);
                    if (result == null || result.product_main == null || (result.product_main != null && result.product_main.status != (int)ProductStatus.ACTIVE))
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Sản phẩm không tồn tại, vui lòng thử lại",
                        });
                    }
                    result.cert = new ProductDetailResponseModelCertificate()
                    {
                        root_product = [],
                        product = [],
                        supply = [],
                        confirm = []
                    };
                    result.favourite = new ProductDetailResponseModelFavourite()
                    {
                        is_favourite = false
                    };
                    var attach_root = await attachFileESModelESRepository.GetByDataidAndType(result.product_main.supplier_id, (int)AttachmentType.Supplier_Cert_RootProduct);
                    var attach_product = await attachFileESModelESRepository.GetByDataidAndType(result.product_main.supplier_id, (int)AttachmentType.Supplier_Cert_Product);
                    var attach_supply = await attachFileESModelESRepository.GetByDataidAndType(result.product_main.supplier_id, (int)AttachmentType.Supplier_Cert_Supply);
                    var attach_confirm = await attachFileESModelESRepository.GetByDataidAndType(result.product_main.supplier_id, (int)AttachmentType.Supplier_Cert_Confirm);
                    if (attach_root != null && attach_root.Count > 0)
                    {
                        result.cert.root_product = attach_root.Select(x => x.Path).ToList();
                    }
                    if (attach_product != null && attach_product.Count > 0)
                    {
                        result.cert.product = attach_product.Select(x => x.Path).ToList();
                    }
                    if (attach_supply != null && attach_supply.Count > 0)
                    {
                        result.cert.supply = attach_supply.Select(x => x.Path).ToList();
                    }
                    if (attach_confirm != null && attach_confirm.Count > 0)
                    {
                        result.cert.confirm = attach_confirm.Select(x => x.Path).ToList();
                    }
                    //favourites:
                    if (request.token != null && request.token.Trim() != "")
                    {
                        long account_client_id = await clientServices.GetAccountClientIdFromToken(request.token);
                        if (account_client_id > 0)
                        {
                            var exists = await _productFavouritesMongoAccess.GetByAccountAndProduct(request.id, account_client_id);
                            if (exists != null && exists._id != null && exists._id.Trim() != "")
                            {
                                result.favourite.is_favourite = true;
                            }

                        }
                    }
                    result.favourite.count = await _productFavouritesMongoAccess.CountByProductId(request.id);
                    if (result.product_main.products_buy_with != null && result.product_main.products_buy_with.Count > 0)
                    {
                        result.product_buy_with = await _productDetailService.ListByProducts(result.product_main.products_buy_with);
                        string static_url = _configuration["config_value:ImageStatic"];
                        if (result.product_buy_with != null && result.product_buy_with.Count > 0)
                        {
                            result.product_buy_with_output = result.product_buy_with.Select(x => new ProductDetailResponseModelProductBuyWith()
                            {
                                _id = x._id,
                                amount = (x.amount_min == null ? x.amount : (double)x.amount_min),
                                name = x.name,
                                code = x.code,
                                avatar = (!x.avatar.Contains(static_url) && !x.avatar.Contains("data:image") && !x.avatar.Contains("http")) ? (static_url + x.avatar) : x.avatar,
                                variation_detail = ProductVariationHelper.RenderVariationDetail(x.attributes, x.attributes_detail, x.variation_detail),
                                exists_flashsale_id=x.exists_flashsale_id,
                                exists_flashsale_name=x.exists_flashsale_name,
                                amount_after_flashsale= x.amount_after_flashsale,
                                flash_sale_fromdate= x.flash_sale_fromdate,
                                flash_sale_todate=x.flash_sale_todate,
                                flashsale_badge_type=x.flashsale_badge_type
                            }).ToList();
                        }
                    }

                   
                    //--Get Label:
                    if (result.product_main != null && result.product_main.label_id > 0)
                    {
                        var cache_name_label = CacheType.LABEL + result.product_main.label_id;

                        var j_data_label = await _redisService.GetAsync(cache_name_label, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        if (j_data_label != null && j_data_label.Trim() != "")
                        {
                            label = JsonConvert.DeserializeObject<Label>(j_data_label);
                        }
                        else
                        {
                            label = await _labelRepository.GetById(result.product_main.label_id);
                            if (label != null && label.Id > 0)
                            {
                                _redisService.Set(cache_name_label, JsonConvert.SerializeObject(label), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));

                            }
                        }
                    }
                    //--Get group:
                    if (result.product_main != null && result.product_main.group_product_id !=null && result.product_main.group_product_id.Trim()!="")
                    {
                        result.groups = new List<GroupProductESModel>();
                        try
                        {
                            var split = result.product_main.group_product_id.Split(",");
                            if(split!=null && split.Count() > 0)
                            {
                                foreach (var item in split)
                                {
                                    try
                                    {
                                        var g = groupProductESService.GetById(Convert.ToInt32(item));
                                        if(g!=null && g.Id > 0)
                                        {
                                            result.groups.Add(g);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                    }
                    _redisService.Set(cache_name, JsonConvert.SerializeObject(result), DateTime.Now.AddDays(1), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = new
                        {
                            product_main=result.product_main,
                            product_sub= result.product_sub
                        },
                        cert = result.cert,
                        favourite = result.favourite,
                        buywith = result.product_buy_with_output,
                        label_detail = label == null ? null : new
                        {
                            label.Id,
                            label.LabelName,
                            label.LabelCode,
                            label.Icon,
                            label.Banner,
                            label.Description,
                        },
                        groups = (result.groups == null || result.groups.Count <= 0) ? null : result.groups.Select(x => new {
                            x.Id,
                            x.ParentId,
                            x.ImagePath,
                            x.Name
                        })
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
                    string cache_name = "ARTICLE_B2C_CATEGORY_MENU_FOOTER" + request.group_id;
                    string j_data = null;
                    List<ArticleGroupViewModel> data = null;

                    try
                    {
                        j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"],
                            "GroupProductController - Redis GET failed: " + ex);
                    }
                    if (j_data != null)
                    {
                        data = JsonConvert.DeserializeObject<List<ArticleGroupViewModel>>(j_data);
                    }
                    else
                    {
                        data = await _newsBusiness.GetFooterCategoryByParentID(request.group_id);
                        if (data != null && data.Count > 0)
                        {
                            try
                            {
                                 _redisService.Set(cache_name, JsonConvert.SerializeObject(data), Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                            }
                            catch (Exception ex)
                            {
                                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"],
                                    "GroupProductController - Redis SET failed: " + ex);
                            }
                        }
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
        [HttpPost("search")]
        public async Task<IActionResult> ProductSearch([FromBody] APIRequestGenericModel input)
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

                    // ✅ Chuẩn hóa keyword: bỏ ký tự đặc biệt + giữ dấu + normalize cho search
                    string rawKeyword = StringHelper.RemoveSpecialCharacterExceptVietnameseCharacter(request.keyword);
                    string normalizedKeyword = StringHelper.NormalizeKeyword(rawKeyword); // Dùng cho no_space_name
                    ProductListResponseModel data = new ProductListResponseModel();
                    var list = await _productESRepository.SearchByKeywordAsync(rawKeyword, normalizedKeyword);
                    if (list != null && list.Count > 0)
                    {
                        data.count = list.Count;
                        data.items = list.Select(x => new ProductMongoDbModel()
                        {
                            amount = x.amount,
                            _id = x.product_id,
                            code = x.product_code,
                            description = x.description,
                            name = x.name,
                            avatar = x.avatar,
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
                    var product = await _productDetailService.SubListing(request.id);
                    if (product != null && product.Count > 0)
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
            //var model_input = new { id = "684c451c5f4484629df83786" };
            //input = new APIRequestGenericModel()
            //{
            //    token = CommonHelper.Encode(JsonConvert.SerializeObject(model_input), _configuration["KEY:private_key"])
            //};
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
                    var data = await productRaitingService.GetListByFilter(request);
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
                    var data = await _productDetailService.GlobalSearch(request.keyword, 0, "", "", 1, 500);
                    List<ProductSpecificationDetailMongoDbModel> brands = new List<ProductSpecificationDetailMongoDbModel>();
                    List<GroupProductESModel> groups = new List<GroupProductESModel>();
                    ProductListResponseModel items = new ProductListResponseModel();
                    if (data != null && data.items != null && data.items.Count > 0)
                    {
                        var value = string.Join(",", data.items.Select(x => x.group_product_id));
                        var ids = value.Split(",").Where(x => x != null && x.Trim() != "").Select(x => Convert.ToInt64(x)).ToList();
                        groups = groupProductESService.GetGroupProductByIDs(ids);
                        brands = data.items.Where(x => x.specification != null && x.specification.Count > 0).SelectMany(x => x.specification).Where(x => x.attribute_id == 1).Distinct().ToList();
                        brands = brands.Where(x => x.value != null && x.value != "null" && x.value.Trim() != "").DistinctBy(x => x.value).ToList();
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
                        data = items,
                        brands = brands,
                        groups = groups
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
                    var data = await _productDetailService.GlobalSearch(request.keyword, request.stars, request.group_product_id, request.brands, (int)request.page_index, (int)request.page_size);

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
                    if (request == null || request.supplier_id == null || request.supplier_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    // Kiểm tra các tham số giá
                    if (request.price_from < 0 || request.price_from == null) request.price_from = 0; // Mặc định là 0 nếu không có giá trị
                    if (request.price_to < 0 || request.price_to == null) request.price_to = 0; // Mặc định là giá trị tối đa
                    if (request.keyword == null) request.keyword = "";
                    if (request.rating == null) request.rating = 0;
                    if (request.page_size <= 0) request.page_size = 10;
                    if (request.page_index < 1) request.page_index = 1;
                    ProductListResponseFEModel result = null;

                    // Nếu không lọc theo giá, sử dụng cache Redis
                    if (request.price_from == 0 && request.price_to <= 0 && request.rating <= 0)
                    {
                        var cache_name = CacheType.PRODUCT_LISTING + (request.keyword ?? "") + request.group_id + request.label_id;
                        var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        if (j_data != null && j_data.Trim() != "")
                        {
                            result = JsonConvert.DeserializeObject<ProductListResponseFEModel>(j_data);

                        }
                        if (result == null || result.items == null || result.items.Count <= 0)
                        {
                            request.label_id = -1;
                            result = await _productDetailService.ProductListingByLabelAndSupplier(request);
                        }
                        if (result != null && result.items.Count > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(result), DateTime.Now.AddDays(1), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
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
                                x.discount,
                                x.exists_flashsale_id,
                                x.exists_flashsale_name,
                                x.amount_after_flashsale,
                                x.flash_sale_fromdate,
                                x.flash_sale_todate,
                                x.flashsale_badge_type
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
                    result = await _productDetailService.ProductListingByLabelAndSupplier(request);
                    if (result != null && result.items.Count > 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = ResponseMessages.Success,
                            data = new
                            {
                                items = result.items,
                                count = result.count
                            }
                        });
                    }
                    //var page_index = request.page_index;
                    //var page_size = request.page_size;
                    //int skip = (request.page_index - 1) * request.page_size;
                    //int size = request.page_size;
                    //int max_size = 300;
                    //// Nếu không lọc theo giá, sử dụng cache Redis
                    //if (request.price_from <= 0 && request.price_to <= 0)
                    //{
                    //    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    //    if (j_data != null && j_data.Trim() != "" && (skip + size) <= max_size)
                    //    {
                    //        result = JsonConvert.DeserializeObject<ProductListResponseFEModel>(j_data);
                    //        if (result != null && result.items.Count >= (skip + size))
                    //        {
                    //            result.items = result.items.Skip(skip).Take(size).ToList();
                    //        }
                    //    }
                    //    if (result == null || result.items == null || result.items.Count <= 0)
                    //    {
                    //        result = await _productDetailService.ProductListingByLabelAndSupplier(request);
                    //    }
                    //}
                    //if ((result == null || result.items == null || result.items.Count <= 0) && (skip + size) <= max_size)
                    //{
                    //    request.page_index = 1;
                    //    request.page_size = max_size;
                    //    result = await _productDetailService.ProductListingByLabelAndSupplier(request);
                    //    var expire_time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 4, 0, 0);
                    //    _redisService.Set(cache_name, JsonConvert.SerializeObject(result), expire_time.AddDays(1), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    //    var product_filtered = _productDetailService.FilterProducts(result.items, request.price_from, request.price_to, page_index, page_size);
                    //    if (product_filtered == null || product_filtered.Count <= 0)
                    //    {
                    //        result.items = result.items.Skip(skip).Take(size).ToList();
                    //    }
                    //    else
                    //    {
                    //        result.items = product_filtered;
                    //    }
                    //}
                    //if (result != null && result.items != null && result.items.Count > 0)
                    //{
                    //    var list = result.items.Select(x => new
                    //    {
                    //        x._id,
                    //        x.code,
                    //        x.name,
                    //        x.avatar,
                    //        x.price,
                    //        x.amount,
                    //        x.amount_min,
                    //        x.amount_max,
                    //        x.rating,
                    //        x.star,
                    //        x.total_sold,
                    //        x.review_count,
                    //        x.old_price,
                    //        x.discount,
                    //        x.exists_flashsale_id,
                    //        x.exists_flashsale_name,
                    //        x.amount_after_flashsale,
                    //        x.flash_sale_fromdate,
                    //        x.flash_sale_todate
                    //    });
                    //    return Ok(new
                    //    {
                    //        status = (int)ResponseType.SUCCESS,
                    //        msg = ResponseMessages.Success,
                    //        data = new
                    //        {
                    //            items = list,
                    //            count = result.count
                    //        }
                    //    });
                    //}
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "No Items"
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
        [HttpPost("list-by-label")]
        public async Task<IActionResult> ProductListingByLabel([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //var model_json = new
                //{
                //    label_id = 19,
                //    page_size = 10,
                //    page_index = 1
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(model_json), _configuration["KEY:private_key"])
                //};
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
                    if (request.price_to == 0 || request.price_to == null) request.price_to = 0; // Mặc định là giá trị tối đa
                    if (request.keyword == null) request.keyword = "";
                    if (request.rating == null) request.rating = 0;


                    if (request.page_size <= 0) request.page_size = 10;
                    if (request.page_index < 1) request.page_index = 1;
                    ProductListResponseFEModel result = null;
                    List<ProductMongoDbModelFEResponseCollection> list = new List<ProductMongoDbModelFEResponseCollection>();
                    Label label = new Label();
                    //--Get Label:
                    var cache_name_label = CacheType.LABEL + request.label_id;
                    var j_data_label = await _redisService.GetAsync(cache_name_label, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    if (j_data_label != null && j_data_label.Trim() != "")
                    {
                        label = JsonConvert.DeserializeObject<Label>(j_data_label);
                    }
                    else
                    {
                        label = await _labelRepository.GetById((int)request.label_id);

                        if (label != null && label.Id > 0)
                        {
                            _redisService.Set(cache_name_label, JsonConvert.SerializeObject(label), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));

                        }
                        else
                        {

                            LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], "/api/product/list-by-label Cannot find LabelID=" + (int)request.label_id);

                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = ResponseMessages.DataInvalid,
                            });
                        }
                    }
                    // Nếu không lọc theo giá, sử dụng cache Redis
                    if (request.price_from == 0 && request.price_to <= 0 && request.rating <= 0)
                    {
                        var cache_name = CacheType.PRODUCT_LISTING + (request.keyword ?? "") + request.group_id + request.label_id + request.page_index + request.page_size;
                        var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        if (j_data != null && j_data.Trim() != "")
                        {
                            result = JsonConvert.DeserializeObject<ProductListResponseFEModel>(j_data);

                        }
                        if (result == null || result.items == null || result.items.Count <= 0)
                        {
                            request.supplier_id = -1;
                            result = await _productDetailService.ProductListingByLabelAndSupplier(request);
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        }
                        if (result != null && result.items != null && result.items.Count > 0)
                        {
                            list = JsonConvert.DeserializeObject<List<ProductMongoDbModelFEResponseCollection>>(JsonConvert.SerializeObject(result.items));

                        }
                    }
                    if (list == null || list.Count <= 0)
                    {
                        result = await _productDetailService.ProductListingByLabelAndSupplier(request);
                        if (result != null && result.items.Count > 0)
                        {
                            list = JsonConvert.DeserializeObject<List<ProductMongoDbModelFEResponseCollection>>(JsonConvert.SerializeObject(result.items));


                        }
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = new
                        {
                            items = list,
                            count = result == null ? 0 : result.count,
                            label_detail = new
                            {
                                label.Id,
                                label.LabelName,
                                label.LabelCode,
                                label.Icon,
                                label.Banner,
                                label.Description,
                                label.Avatar,
                                label.BannerMain,
                                BannerSub = (label.BannerSub!=null && label.BannerSub.Trim().Contains("[")?JsonConvert.DeserializeObject<List<string>>(label.BannerSub): new List<string>())

                            }
                        }
                    });
                    //var page_index = request.page_index;
                    //var page_size = request.page_size;
                    //int skip = (request.page_index - 1) * request.page_size;
                    //int size = request.page_size;
                    //int max_size = 300;
                    //// Nếu không lọc theo giá, sử dụng cache Redis
                    //if (request.price_from <= 0 && request.price_to <= 0)
                    //{
                    //    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    //    if (j_data != null && j_data.Trim() != "" && (skip + size) <= max_size)
                    //    {
                    //        result = JsonConvert.DeserializeObject<ProductListResponseFEModel>(j_data);
                    //        if (result != null && result.items.Count >= (skip + size))
                    //        {
                    //            result.items = result.items.Skip(skip).Take(size).ToList();
                    //        }
                    //    }
                    //    if (result == null || result.items == null || result.items.Count <= 0)
                    //    {
                    //        result = await _productDetailService.ProductListingByLabelAndSupplier(request);
                    //    }
                    //}
                    //if ((result == null || result.items == null || result.items.Count <= 0) && (skip + size) <= max_size)
                    //{
                    //    request.page_index = 1;
                    //    request.page_size = max_size;
                    //    result = await _productDetailService.ProductListingByLabelAndSupplier(request);
                    //    var expire_time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 4, 0, 0);
                    //    _redisService.Set(cache_name, JsonConvert.SerializeObject(result), expire_time.AddDays(1), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    //    var product_filtered = _productDetailService.FilterProducts(result.items, request.price_from, request.price_to, page_index, page_size);
                    //    if (product_filtered == null || product_filtered.Count <= 0)
                    //    {
                    //        result.items = result.items.Skip(skip).Take(size).ToList();
                    //    }
                    //    else
                    //    {
                    //        result.items = product_filtered;
                    //    }
                    //}
                    //if (result != null && result.items != null && result.items.Count > 0)
                    //{
                    //    var list = result.items.Select(x => new
                    //    {
                    //        x._id,
                    //        x.code,
                    //        x.name,
                    //        x.avatar,
                    //        x.price,
                    //        x.amount,
                    //        x.amount_min,
                    //        x.amount_max,
                    //        x.rating,
                    //        x.star,
                    //        x.total_sold,
                    //        x.review_count,
                    //        x.old_price,
                    //        x.discount,
                    //        x.exists_flashsale_id,
                    //        x.exists_flashsale_name,
                    //        x.amount_after_flashsale,
                    //        x.flash_sale_fromdate,
                    //        x.flash_sale_todate,

                    //    });
                    //    return Ok(new
                    //    {
                    //        status = (int)ResponseType.SUCCESS,
                    //        msg = ResponseMessages.Success,
                    //        data = new
                    //        {
                    //            items = list,
                    //            count = result.count,
                    //            label_detail = new
                    //            {
                    //                label.Id,
                    //                label.LabelName,
                    //                label.LabelCode,
                    //                label.Icon,
                    //                label.Banner,
                    //                label.Description,
                    //                label.Avatar

                    //            }
                    //        }
                    //    });
                    //}

                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "No Items"
                });
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
                    if (request == null || request.token == null || request.token.Trim() == "")
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
                    if (result != null && result.items != null && result.items.Count > 0)
                    {
                        //_redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        result.items = (request.page_size * request.page_index) >= result.count ? result.items.Skip(skip).Take(request.page_size).ToList() : new List<ProductsFavouritesMongoDbModel>();
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = ResponseMessages.Success,
                            data = result.items,
                            total = result.count
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
                    var detail = await _productDetailService.GetByID(request.product_id);
                    if (detail == null || detail._id == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var id = await _productFavouritesMongoAccess.AddNewAsync(new ProductsFavouritesMongoDbModel()
                    {
                        product_id = request.product_id,
                        updated_last = DateTime.Now,
                        account_client_id = account_client_id,
                        detail = detail,
                    });
                    var cache_name = CacheType.PRODUCT_DETAIL + request.product_id;
                    _redisService.clear(cache_name,  Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
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
                    var id = await _productFavouritesMongoAccess.DeleteAsync(account_client_id, request.product_id);
                    var cache_name = CacheType.PRODUCT_DETAIL + request.product_id;
                    _redisService.clear(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
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
        [HttpPost("search-listing")]
        public async Task<IActionResult> ProductSearchListing([FromBody] APIRequestGenericModel input)
        {
            try
            {

                //var request_json = new ProductGlobalSearchRequestModel()
                //{
                //    keyword = "men vi sinh"
                //};
                //var token = CommonHelper.Encode(JsonConvert.SerializeObject(request_json), _configuration["KEY:private_key"]);
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

                    // ✅ Chuẩn hóa keyword: bỏ ký tự đặc biệt + giữ dấu + normalize cho search
                    string rawKeyword = StringHelper.RemoveSpecialCharacterExceptVietnameseCharacter(request.keyword);
                    string normalizedKeyword = StringHelper.NormalizeKeyword(rawKeyword); // Dùng cho no_space_name
                    ProductListResponseModel data = new ProductListResponseModel();
                    var list = await _productESRepository.SearchByKeywordAsync(rawKeyword, normalizedKeyword);
                    if (list != null && list.Count > 0)
                    {
                        var list_product_mongo = await _productDetailService.ListByProducts(list.Select(x => x.product_id).ToList());
                        var list_data = list_product_mongo.Select(x => new
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
                            x.discount,
                            x.exists_flashsale_id,
                            x.exists_flashsale_name,
                            x.amount_after_flashsale,
                            x.flash_sale_fromdate,
                            x.flash_sale_todate,
                            x.flashsale_badge_type

                        });
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = ResponseMessages.Success,
                            data = new
                            {
                                items = list_data,
                                count = list.Count
                            }
                        });
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = ResponseMessages.Success,
                        data = new
                        {
                            items = new List<dynamic>(),
                            count = 0
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
                msg = ResponseMessages.DataInvalid,
            });
        }

    }

}