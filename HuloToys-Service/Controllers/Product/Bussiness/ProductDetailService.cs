using Azure.Core;
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Entities.ViewModels.Products;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Flashsale.Bussiness;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.Raiting;
using HuloToys_Service.MongoDb;
using HuloToys_Service.Utilities.lib;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;
using Utilities.Contants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HuloToys_Service.Controllers.Product.Bussiness
{
    public class ProductDetailService
    {
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly CartMongodbService _cartMongodbService;
        private readonly RaitingESService _raitingESService;
        private readonly ClientESService _clientESService;
        private readonly IConfiguration _configuration;
        private readonly GroupProductESService groupProductESService;
        private readonly OrderDetailESService orderDetailESService;
        private readonly ProductFavouritesMongoAccess _productFavouritesMongoAccess;
        private readonly FlashSaleESRepository flashSaleESRepository;
        private readonly FlashSaleProductESRepository flashSaleProductESRepository;
        private readonly FlashsaleService flashsaleService;
        public ProductDetailService(IConfiguration configuration)
        {
            _productDetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _cartMongodbService = new CartMongodbService(configuration);
            groupProductESService = new GroupProductESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _clientESService = new ClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _configuration = configuration;
            orderDetailESService = new OrderDetailESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _productFavouritesMongoAccess = new ProductFavouritesMongoAccess(configuration);
            flashSaleESRepository = new FlashSaleESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            flashSaleProductESRepository = new FlashSaleProductESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            flashsaleService = new FlashsaleService(configuration);

        }
        public async Task<ProductListFEResponseModel> ProductListing(ProductListRequestModel request)
        {
            ProductListFEResponseModel result = new ProductListFEResponseModel();
            try
            {
                // Chuẩn hóa từ khóa tìm kiếm
                request.keyword = StringHelper.ValidateTextForSearch(request.keyword);


                var data = await _productDetailMongoAccess.ResponseListing(request.keyword, request.group_id, request.page_index, request.page_size, request.price_from, request.price_to, request.rating);
                result = JsonConvert.DeserializeObject<ProductListFEResponseModel>(JsonConvert.SerializeObject(data));
                if (result != null && result.items != null && result.items.Count > 0)
                {
                    result.items_flashsale= await UpdateProductDetail(data.items);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return result;
        }
        public async Task<ProductListFEResponseModel> ProductListingByLabelAndSupplier(ProductListByIdRequestModel request)
        {
            ProductListFEResponseModel result = new ProductListFEResponseModel();
            try
            {
                // Chuẩn hóa từ khóa tìm kiếm
                request.keyword = StringHelper.ValidateTextForSearch(request.keyword);


                var data = await _productDetailMongoAccess.ResponseListing(request.keyword, request.group_id, request.page_index, request.page_size, request.price_from, request.price_to, request.rating, request.supplier_id, request.label_id);
                result = JsonConvert.DeserializeObject<ProductListFEResponseModel>(JsonConvert.SerializeObject(data));
                if (result != null && result.items != null && result.items.Count > 0)
                {
                    result.items_flashsale = await UpdateProductDetail(data.items);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return result;
        }
        public async Task<ProductMongoDbModelFEResponse> GetByID(string id)
        {
            ProductMongoDbModelFEResponse result = new ProductMongoDbModelFEResponse();
            try
            {
                var detail = await _productDetailMongoAccess.GetByID(id);
                if (detail != null)
                {
                    result= await UpdateProductDetail(detail);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return result;
        }
        public async Task<ProductDetailResponseModel> GetFullProductById(string id)
        {
            ProductDetailResponseModel result = new ProductDetailResponseModel();
            try
            {
                result = await _productDetailMongoAccess.GetFullProductById(id);
                if (result != null && result.product_main != null && result.product_main._id.Trim() != "")
                {
                    result.flashsale_main=await UpdateProductDetail(result.product_main);
                    if (result != null && result.product_sub != null && result.product_sub.Count > 0)
                    {
                        result.flashsale_sub = await UpdateProductDetail(result.product_sub);

                        result.flashsale_main.amount_min = result.flashsale_sub.Min(x => x.amount);
                        result.flashsale_main.amount_max = result.flashsale_sub.Max(x => x.amount);

                    }
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return result;
        }
        public async Task<List<ProductMongoDbModelFEResponse>> ListByProducts(List<string> ids)
        {
            List<ProductMongoDbModelFEResponse> result = new List<ProductMongoDbModelFEResponse>();
            try
            {
                var list_product_mongo = await _productDetailMongoAccess.ListByProducts(ids);
                if (list_product_mongo != null && list_product_mongo.Count > 0)
                {
                    result =  await UpdateProductDetail(list_product_mongo);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return result;
        }
        public async Task<ProductMongoDbModelFEResponse> UpdateProductDetail(ProductMongoDbModel product)
        {
            ProductMongoDbModelFEResponse item = new ProductMongoDbModelFEResponse();
            try
            {

                item = JsonConvert.DeserializeObject<ProductMongoDbModelFEResponse>(JsonConvert.SerializeObject(product));
                var active_flashsale = await flashSaleESRepository.SearchActiveFlashSales();
                List<FlashSaleProductESModel> list_item = new List<FlashSaleProductESModel>();
                if (active_flashsale != null && active_flashsale.Count > 0)
                {
                    list_item = await flashSaleProductESRepository.GetByListFlashsaleId(active_flashsale.Select(x => x.flashsale_id).ToList());
                    UpdateProductItem(item, active_flashsale, list_item);
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return item;
        }

        public async Task<List<ProductMongoDbModelFEResponse>> UpdateProductDetail(List<ProductMongoDbModel> products_original)
        {
            List<ProductMongoDbModelFEResponse> products=new List<ProductMongoDbModelFEResponse> ();
            try
            {
                products=JsonConvert.DeserializeObject<List<ProductMongoDbModelFEResponse>>(JsonConvert.SerializeObject(products_original));
                if (products == null || products.Count <= 0) return products;
                var active_flashsale = await flashSaleESRepository.SearchActiveFlashSales();
                List<FlashSaleProductESModel> list_item = new List<FlashSaleProductESModel>();
                if (active_flashsale != null && active_flashsale.Count > 0)
                {
                    list_item = await flashSaleProductESRepository.GetByListFlashsaleId(active_flashsale.Select(x => x.flashsale_id).ToList());
                }
                List<ProductMongoDbModelFEResponse> output=new List<ProductMongoDbModelFEResponse>();
                foreach (var item in products)
                {
                    UpdateProductItem(item, active_flashsale, list_item);
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return products;
        }
        private bool UpdateProductItem(ProductMongoDbModelFEResponse item, List<FlashSaleESModel> active_flashsale, List<FlashSaleProductESModel> list_item)
        {
            try
            {
                if (item == null || item._id == null) return false;
                var raiting = _raitingESService.GetListByFilter(new Models.Raiting.ProductRaitingRequestModel()
                {
                    id = item._id,
                    has_comment = false,
                    has_media = false,
                    page_index = 1,
                    page_size = 500,
                    stars = 0
                });
                if (raiting != null && raiting.Count > 0)
                {
                    var sum_raiting = raiting.Average(x => x.Star);
                    item.star = sum_raiting == null ? 5 : (float)sum_raiting;
                    item.review_count = raiting.Count;
                    item.rating = (sum_raiting == null ? 5 : (float)sum_raiting);
                }
                item.total_sold = (item.total_sold == null) ? 0 : (long)item.total_sold;
                var total_sold = orderDetailESService.SumQuantityByProductId(new List<string>() { item._id });
                item.total_sold += total_sold;
                if (active_flashsale != null && active_flashsale.Count > 0 && list_item != null && list_item.Count > 0)
                {
                    var exists_flash_sale_product = list_item.FirstOrDefault(x => x.productid == (item.parent_product_id != null && item.parent_product_id.Trim() != "" ? item.parent_product_id : item._id));
                    if (exists_flash_sale_product != null && exists_flash_sale_product.flashsale_id != null)
                    {
                        var exists_flash_sale = active_flashsale.First(x => x.flashsale_id == exists_flash_sale_product.flashsale_id);
                        double total_discount = 0;

                        double percent = Convert.ToDouble(exists_flash_sale_product.discountvalue);
                        var amount_product = item.amount;
                        if (item.amount <= 0 && item.amount_min != null && item.amount_min > 0)
                        {
                            amount_product = (double)item.amount_min;

                        }
                        double old_price = item.price;
                        if (old_price <= 0)
                        {
                            old_price = amount_product;
                        }
                        switch (exists_flash_sale_product.valuetype)
                        {
                            case 1:
                                total_discount += (amount_product * Convert.ToDouble(percent / 100));
                                break;
                            case 0:
                                total_discount += percent;
                                break;

                            default: break;
                        }
                        total_discount = Math.Round(total_discount, 0);
                        item.exists_flashsale_id = exists_flash_sale.flashsale_id;
                        item.flash_sale_fromdate = exists_flash_sale.fromdate;
                        item.flash_sale_todate = exists_flash_sale.todate;
                        item.exists_flashsale_name = exists_flash_sale.name;
                        item.amount_after_flashsale = amount_product - total_discount;
                        item.profit -= total_discount;
                        if (item.amount <= 0 && item.amount_min != null && item.amount_min > 0)
                        {
                            item.amount_min -= total_discount;
                        }
                        if (item.amount <= 0 && item.amount_max != null && item.amount_max > 0)
                        {
                            item.amount_max -= total_discount;
                        }
                        item.discount = Math.Round(((old_price - (double)item.amount_after_flashsale) / old_price * 100), 0);
                        item.discount = item.discount <= 0 ? 0 : item.discount;
                    }
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                return false;
            }
            return true;
        }

    }
}
