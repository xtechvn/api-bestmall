using Azure.Core;
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Entities.ViewModels.Products;
using HuloToys_Front_End.Models.Products;
using HuloToys_Service.Controllers.Flashsale.Bussiness;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.Models.Flashsale;
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities;
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
        public ProductDetailService(IConfiguration configuration, ProductDetailMongoAccess productDetailMongoAccess,
            CartMongodbService cartMongodbService, FlashsaleService _flashsaleService, ProductFavouritesMongoAccess productFavouritesMongoAccess)
        {
             _productDetailMongoAccess = productDetailMongoAccess;
            _cartMongodbService = cartMongodbService;
            groupProductESService = new GroupProductESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _raitingESService = new RaitingESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _clientESService = new ClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _configuration = configuration;
            orderDetailESService = new OrderDetailESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            _productFavouritesMongoAccess = productFavouritesMongoAccess;
            flashSaleESRepository = new FlashSaleESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            flashSaleProductESRepository = new FlashSaleProductESRepository(configuration["DataBaseConfig:Elastic:Host"], configuration);
            flashsaleService = _flashsaleService;
         
        }
        //public async Task<ProductListResponseModel> ProductListing(ProductListRequestModel request)
        //{
        //    ProductListResponseModel result = new ProductListResponseModel();
        //    try
        //    {
        //        // Chuẩn hóa từ khóa tìm kiếm
        //        request.keyword = StringHelper.ValidateTextForSearch(request.keyword);


        //        var data = await _productDetailMongoAccess.ResponseListing(request.keyword, request.group_id, request.page_index, request.page_size, request.price_from, request.price_to, request.rating);
        //        //if (data != null && data.items != null && data.items.Count > 0)
        //        //{
        //        //    //result = JsonConvert.DeserializeObject<ProductListResponseModel>(JsonConvert.SerializeObject(data));
        //        //    //result.items= await UpdateProductDetail(data.items);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //    }
        //    return result;
        //}
        //public async Task<ProductListResponseModel> ProductListingByLabelAndSupplier(ProductListByIdRequestModel request)
        //{
        //    ProductListResponseModel result = new ProductListResponseModel();
        //    try
        //    {
        //        // Chuẩn hóa từ khóa tìm kiếm
        //        request.keyword = StringHelper.ValidateTextForSearch(request.keyword);


        //        result = await _productDetailMongoAccess.ResponseListing(request.keyword, request.group_id, request.page_index, request.page_size, request.price_from, request.price_to, request.rating, request.supplier_id, request.label_id);
        //        //result = JsonConvert.DeserializeObject<ProductListResponseModel>(JsonConvert.SerializeObject(data));
        //        //if (result != null && result.items != null && result.items.Count > 0)
        //        //{
        //        //    result.items = await UpdateProductDetail(data.items);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //    }
        //    return result;
        //}
        public async Task<ProductMongoDbModel> GetByID(string id)
        {
            //ProductMongoDbModel result = new ProductMongoDbModel();
            //try
            //{
            //    var detail = await _productDetailMongoAccess.GetByID(id);
            //    if (detail != null)
            //    {
            //        result = await UpdateProductDetail(detail);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
            //    LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            //}
            //return result;
            return await _productDetailMongoAccess.GetByID(id);
        }
        public async Task<ProductDetailResponseModel> GetFullProductById(string id)
        {
            ProductDetailResponseModel result = new ProductDetailResponseModel();
            try
            {
                var data = await _productDetailMongoAccess.GetFullProductById(id);
                if (data != null && data.product_main != null && data.product_main._id.Trim() != "")
                {
                    //result = JsonConvert.DeserializeObject<ProductDetailResponseModel>(JsonConvert.SerializeObject(data));
                    //result.product_main=await UpdateProductDetail(result.product_main);
                    //if(data.product_sub!=null && data.product_sub.Count > 0)
                    //{
                    //    result.product_sub = await UpdateProductDetail(data.product_sub);
                    //}
                    result.product_main= data.product_main;
                    result.product_sub= data.product_sub;
                    result.product_main.flash_sale_amount_min = result.product_sub.Min(x => (x.amount_after_flashsale != null && x.amount_after_flashsale > 0 ? x.amount_after_flashsale : x.amount));
                    result.product_main.flash_sale_amount_max = result.product_sub.Max(x => (x.amount_after_flashsale != null && x.amount_after_flashsale > 0 ? x.amount_after_flashsale : x.amount));

                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return result;
        }
        //public async Task<ProductDetailResponseModel> UpdateFullProductById(ProductDetailResponseModel result)
        //{
        //    try
        //    {

        //        result.product_main = await UpdateProductDetail(result.product_main);
        //        if (result.product_sub != null && result.product_sub.Count > 0)
        //        {
        //            result.product_sub = await UpdateProductDetail(result.product_sub);
        //            result.product_main.amount_min = result.product_sub.Min(x => (x.amount_after_flashsale != null && x.amount_after_flashsale > 0 ? x.amount_after_flashsale : x.amount));
        //            result.product_main.amount_max = result.product_sub.Max(x => (x.amount_after_flashsale != null && x.amount_after_flashsale > 0 ? x.amount_after_flashsale : x.amount));

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //    }

        //    return result;
        //}
        public async Task<List<ProductMongoDbModel>> ListByProducts(List<string> ids)
        {
            List<ProductMongoDbModel> result = new List<ProductMongoDbModel>();
            try
            {
                var list_product_mongo = await _productDetailMongoAccess.ListByProducts(ids);
                //if (list_product_mongo != null && list_product_mongo.Count > 0)
                //{
                //    result =  await UpdateProductDetail(list_product_mongo);
                //}
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return result;
        }
        //public async Task<ProductMongoDbModel> UpdateProductDetail(ProductMongoDbModel product)
        //{
        //    ProductMongoDbModel item = new ProductMongoDbModel();
        //    try
        //    {

        //        item = JsonConvert.DeserializeObject<ProductMongoDbModel>(JsonConvert.SerializeObject(product));

        //        //var active_flashsale = await flashSaleESRepository.SearchActiveFlashSales();
        //        //List<FlashSaleProductESModel> list_item = new List<FlashSaleProductESModel>();
        //        //if (active_flashsale != null && active_flashsale.Count > 0)
        //        //{
        //        //    list_item = await flashSaleProductESRepository.GetByListFlashsaleId(active_flashsale.Select(x => x.flashsale_id).ToList());
        //        //    var group_type = groupProductESService.GetListGroupProductByParentId(109);
        //        //    UpdateProductItem(item, active_flashsale, list_item, group_type);
        //        //}

        //    }
        //    catch (Exception ex)
        //    {
        //        string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //    }
        //    return item;
        //}

        //    public async Task<List<ProductMongoDbModel>> UpdateProductDetail(List<ProductMongoDbModel> products_original)
        //    {
        //        List<ProductMongoDbModel> products=new List<ProductMongoDbModel> ();
        //        try
        //        {
        ////            products=JsonConvert.DeserializeObject<List<ProductMongoDbModel>>(JsonConvert.SerializeObject(products_original));
        ////            if (products == null || products.Count <= 0) return products;
        ////var active_flashsale = await flashSaleESRepository.SearchActiveFlashSales();
        ////            List<FlashSaleProductESModel> list_item = new List<FlashSaleProductESModel>();
        ////            if (active_flashsale != null && active_flashsale.Count > 0)
        ////            {
        ////                list_item = await flashSaleProductESRepository.GetByListFlashsaleId(active_flashsale.Select(x => x.flashsale_id).ToList());
        ////            }
        ////            List<ProductMongoDbModel> output=new List<ProductMongoDbModel>();
        ////            var group_type = groupProductESService.GetListGroupProductByParentId(109);
        ////            foreach (var item in products)
        ////            {
        ////                UpdateProductItem(item, active_flashsale, list_item, group_type);
        ////            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //            LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //        }
        //        return products;
        //    }
        //public async Task<List<ProductMongoDbModel>> UpdateProductDetail(List<ProductMongoDbModel> products)
        //{
        //    try
        //    {
        //        if (products == null || products.Count <= 0) return products;
        //        var active_flashsale = await flashSaleESRepository.SearchActiveFlashSales();
        //        List<FlashSaleProductESModel> list_item = new List<FlashSaleProductESModel>();
        //        if (active_flashsale != null && active_flashsale.Count > 0)
        //        {
        //            list_item = await flashSaleProductESRepository.GetByListFlashsaleId(active_flashsale.Select(x => x.flashsale_id).ToList());
        //        }
        //        var group_type = groupProductESService.GetListGroupProductByParentId(109);

        //        List<ProductMongoDbModel> output = new List<ProductMongoDbModel>();
        //        foreach (var item in products)
        //        {
        //            UpdateProductItem(item, active_flashsale, list_item, group_type );
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //    }
        //    return products;
        //}

        public async Task<List<ProductMongoDbModel>> SubListing(string parent_id)
        {
            return await _productDetailMongoAccess.SubListing(parent_id);
        }
        public async Task<ProductListResponseModel> GlobalSearch(string keyword = "", int? stars = 0, string? group_product_id = "", string? brands = "", int page_index = 1, int page_size = 12)
        {
            return await _productDetailMongoAccess.GlobalSearch(keyword, stars, group_product_id, brands, page_index, page_size);
        }
        // public async Task<List<ProductMongoDbModel>> ListByProductNoExtend(List<string> ids)
        // {
        //     return await _productDetailMongoAccess.ListByProducts(ids);
        // }
        public async Task<List<FlashSaleProductResposeModel>> GetFlashSaleProductByProductIds(List<FlashSaleProductESModel> list_item)
        {
            List<FlashSaleProductResposeModel> result = new List<FlashSaleProductResposeModel>();
            try
            {

                if (list_item != null && list_item.Count > 0)
                {
                    var list_product_mongo = await _productDetailMongoAccess.ListByProducts(list_item.Select(x => x.productid).ToList());
                    if (list_product_mongo != null && list_product_mongo.Count > 0) result = list_product_mongo.Select(selected => new FlashSaleProductResposeModel()
                    {
                        amount = selected.amount_min !=null && selected.amount_min>0? (double)selected.amount_min: selected.amount,
                        amount_after_flashsale = selected.amount_after_flashsale,
                        discountvalue = selected.flash_sale_discount,
                        position = list_item.First(x => x.productid.Trim() == selected._id.Trim()).position,
                        total_discount = selected.flash_sale_discount,
                        _id = selected._id,
                        avatar = selected.avatar,
                        name = selected.name,
                        code = selected.code,
                        rating = selected.rating,
                        review_count = selected.review_count,
                        total_sold = selected.total_sold,
                        super_sale = list_item.First(x => x.productid.Trim() == selected._id.Trim()).supersale,
                        badge_type = selected.flashsale_badge_type
                    }).ToList();
                    //{
                    //    foreach (var selected in list_product_mongo)
                    //    {
                    //        ProductMongoDbModel extend_product = new ProductMongoDbModel()
                    //        {
                    //            _id = selected._id,
                    //        };
                    //        var item = list_item.First(x => x.productid.Trim() == selected._id.Trim());
                    //        var amount_product = selected.amount;
                    //        if (selected.amount <= 0 && selected.amount_min != null && selected.amount_min > 0)
                    //        {
                    //            amount_product = (double)selected.amount_min;

                    //        }
                    //        double old_price = selected.old_price == null || selected.old_price <= 0 ? amount_product : (double)selected.old_price;
                    //        if (old_price <= 0)
                    //        {
                    //            old_price = amount_product;
                    //        }
                    //        double total_discount = 0;
                    //        double percent = Convert.ToDouble(item.discountvalue);
                    //        switch (item.valuetype)
                    //        {
                    //            case 1:
                    //                total_discount += (amount_product * Convert.ToDouble(percent / 100));
                    //                break;
                    //            case 0:
                    //                total_discount += percent;
                    //                break;

                    //            default: break;
                    //        }
                    //        var discount_percent = Math.Round(total_discount / old_price * 100, 0);
                    //        discount_percent = discount_percent <= 0 ? 0 : discount_percent;
                    //        result.Add(new FlashSaleProductResposeModel()
                    //        {
                    //            amount = old_price,
                    //            amount_after_flashsale = Math.Ceiling(amount_product - total_discount),
                    //            discountvalue = discount_percent,
                    //            position = item.position ?? 0,
                    //            total_discount = total_discount,
                    //            _id = selected._id,
                    //            avatar = selected.avatar,
                    //            name = selected.name,
                    //            code = selected.code,
                    //            rating = extend_product.rating,
                    //            review_count = extend_product.review_count,
                    //            total_sold = selected.total_sold,
                    //            super_sale = item.supersale,
                    //            badge_type = selected.flashsale_badge_type
                    //        });
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
                LogHelper.InsertLogTelegram("GetFlashSaleProductByProductIds -1-  err=" + ex.ToString());

            }
            return result;
        }
        // public async Task<long> CountFlashSaleProductByProductIds(List<FlashSaleProductESModel> list_item)
        // {
        //     long count = 0;
        //     try
        //     {
        //         if (list_item != null && list_item.Count > 0)
        //         {
        //             count = await _productDetailMongoAccess.CountListByProducts(list_item.Select(x => x.productid).ToList());
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         LogHelper.InsertLogTelegram("CountFlashSaleProductByProductIds -1-  err=" + ex.ToString());

        //     }
        //     return count;
        // }
        //private async Task<bool> UpdateProductItem(ProductMongoDbModel item, List<FlashSaleESModel> active_flashsale, List<FlashSaleProductESModel> list_item, List<GroupProductESModel> group_types, bool ignore_raiting = false, bool ignore_total_sold = false)
        //{
        //    try
        //    {
        //        if (item == null || item._id == null) return false;
        //        //if (active_flashsale != null && active_flashsale.Count > 0 && list_item != null && list_item.Count > 0)
        //        //{
        //        //  await  UpdateProductFlashsale(item, active_flashsale, list_item, group_types);
        //        //}
        //        if (!ignore_raiting)
        //        {
        //            UpdateProductRaiting(item);
        //        }
        //        //if (!ignore_total_sold)
        //        //{
        //        //    UpdateProductTotalSold(item);

        //        //}

        //    }
        //    catch (Exception ex)
        //    {
        //        string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //        return false;
        //    }
        //    return true;
        //}

        //private bool UpdateProductTotalSold(ProductMongoDbModel item)
        //{
        //    try
        //    {
        //        if (item == null || item._id == null) return false;
        //        item.total_sold = (item.total_sold == null) ? 0 : (long)item.total_sold;
        //        var total_sold = orderDetailESService.SumQuantityByProductId(new List<string>() { item._id });
        //        item.total_sold = total_sold;
        //    }
        //    catch (Exception ex)
        //    {
        //        string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //        return false;
        //    }
        //    return true;
        //}
        //private async Task<bool> UpdateProductFlashsale(ProductMongoDbModel item, List<FlashSaleESModel> active_flashsale, List<FlashSaleProductESModel> list_item,List<GroupProductESModel> group_types)
        //{
        //    try
        //    {
        //        if (item == null || item._id == null) return false;

        //        //bool has_badge=false;
        //        if (active_flashsale != null && active_flashsale.Count > 0 && list_item != null && list_item.Count > 0)
        //        {
        //            string product_id_compare = item._id;
        //            if(item.parent_product_id!=null && item.parent_product_id.Trim() != "")
        //            {
        //                product_id_compare = item.parent_product_id;
        //            }
        //            var exists_flash_sale_product = list_item.FirstOrDefault(x => x.productid == product_id_compare && x.status==1);
        //            if (exists_flash_sale_product != null && exists_flash_sale_product.flashsale_id != null && exists_flash_sale_product.discountvalue!=null&& exists_flash_sale_product.valuetype!=null)
        //            {
        //                var exists_flash_sale = active_flashsale.First(x => x.flashsale_id == exists_flash_sale_product.flashsale_id);
        //                double total_discount = 0;

        //                double percent = Convert.ToDouble(exists_flash_sale_product.discountvalue);
        //                var amount_product = item.amount;
        //                if (item.amount <= 0 && item.amount_min != null && item.amount_min > 0)
        //                {
        //                    amount_product = (double)item.amount_min;

        //                }
        //                //double old_price = item.old_price == null || item.old_price <= 0 ? amount_product : (double)item.old_price;
        //                //if (old_price <= 0)
        //                //{
        //                //    old_price = amount_product;
        //                //}
        //                switch (exists_flash_sale_product.valuetype)
        //                {
        //                    case 1:
        //                        total_discount += (amount_product * Convert.ToDouble(percent / 100));
        //                        break;
        //                    case 0:
        //                        total_discount += percent;
        //                        break;

        //                    default: break;
        //                }
        //                total_discount = Math.Round(total_discount, 0);
        //                item.exists_flashsale_id = exists_flash_sale.flashsale_id;
        //                item.flash_sale_fromdate = exists_flash_sale.fromdate;
        //                item.flash_sale_todate = exists_flash_sale.todate;
        //                item.exists_flashsale_name = exists_flash_sale.name;
        //                item.amount_after_flashsale = amount_product - total_discount;
        //                //item.profit -= total_discount;
        //                if (item.amount <= 0 && item.amount_min != null && item.amount_min > 0)
        //                {
        //                    item.amount_min -= total_discount;
        //                    item.amount_min = Math.Ceiling((double)item.amount_min);

        //                }
        //                if (item.amount <= 0 && item.amount_max != null && item.amount_max > 0)
        //                {
        //                    item.amount_max -= total_discount;
        //                    item.amount_max = Math.Ceiling((double)item.amount_max);

        //                }
        //                item.discount = Math.Round(((amount_product - (double)item.amount_after_flashsale) / amount_product * 100), 0);
        //                item.discount = item.discount <= 0 ? 0 : item.discount;
        //               // item.price = amount_product- item.profit;
        //                item.old_price = amount_product;
        //                item.amount_after_flashsale = Math.Ceiling((double)item.amount_after_flashsale);
        //                //item.profit = NumberHelpers.RoundUpToHundredsDouble((double)item.profit);
        //                //item.flashsale_badge_type = exists_flash_sale_product.badgetype;
        //                //has_badge = true;
        //                item.flash_sale_unit = exists_flash_sale_product.valuetype;
        //                item.flash_sale_price_sales = Convert.ToDecimal(exists_flash_sale_product.discountvalue);
        //                return true;
        //            }

        //        }

        //        var base_product = await _productDetailMongoAccess.GetByID(item._id);
        //        if (base_product != null && base_product._id != null)
        //        {
        //            item = JsonConvert.DeserializeObject<ProductMongoDbModel>(JsonConvert.SerializeObject(base_product));

        //        }
        //        //if (!has_badge) {
        //        //    if (group_types != null && group_types.Count>0 && item.group_product_id!=null && item.group_product_id.Trim()!="" ) {
        //        //        try
        //        //        {
        //        //            var exists = group_types.FirstOrDefault(x => x.Id == Convert.ToInt32(item.group_product_id.Trim().Split(",")[0]));
        //        //            if (exists != null && exists.Id>0) { item.flashsale_badge_type = exists.Id; }
        //        //        }
        //        //        catch { }

        //        //    }
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //        LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //        return false;
        //    }
        //    return true;
        //}
        //public List<ProductMongoDbModel> FilterProducts(List<ProductMongoDbModel> products, double? price_from,
        //double? price_to,
        //int page_index,
        //int page_size)
        //  {
        //      List<ProductMongoDbModel> filteredProducts = products;

        //      try
        //      {
        //          // Bước 2: Áp dụng filter giá phức tạp trong bộ nhớ
        //          filteredProducts = products.Where(p =>
        //          {
        //              // Logic ưu tiên giá phức tạp của bạn
        //              double? priceToCheck = p.amount_after_flashsale;

        //              if (!priceToCheck.HasValue)
        //              {
        //                  priceToCheck = p.amount_min;
        //              }

        //              if (!priceToCheck.HasValue)
        //              {
        //                  priceToCheck = p.amount;
        //              }

        //              if (priceToCheck.HasValue)
        //              {
        //                  bool meetsMin = !price_from.HasValue || priceToCheck.Value >= price_from.Value;
        //                  bool meetsMax = !price_to.HasValue || priceToCheck.Value <= price_to.Value;
        //                  return meetsMin && meetsMax;
        //              }
        //              return false;
        //          }).ToList();

        //          // Áp dụng Skip và Take cho phân trang
        //          filteredProducts = filteredProducts
        //              .Skip((page_index - 1) * page_size)
        //              .Take(page_size)
        //              .ToList();

        //          return filteredProducts;
        //      }
        //      catch (Exception ex)
        //      {
        //          string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
        //          LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
        //          return null;

        //      }


        //  }


        public bool UpdateProductRaiting(ProductMongoDbModel item)
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
                    item.star = sum_raiting == null ? 5 : (float)Math.Round((float)sum_raiting, 1);
                    item.review_count = raiting.Count;
                    item.rating = (sum_raiting == null ? 5 : (float)Math.Round((float)sum_raiting, 1));
                }
                else
                {
                    item.star = 0;
                    item.review_count = 0;
                    item.rating = 0;
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
        public  async Task UpdateProductRaitingDelay(ProductMongoDbModel item, int delayMilliseconds = 5000)
        {

            await Task.Delay(delayMilliseconds);
            UpdateProductRaiting(item);
        }
    }
}
