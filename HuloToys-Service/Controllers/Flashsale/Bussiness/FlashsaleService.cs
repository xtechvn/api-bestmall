using Entities.ViewModels.Products;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.Models.Flashsale;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Flashsale.Bussiness
{
    public class FlashsaleService
    {
        private readonly IConfiguration _configuration;
        private readonly ProductDetailService productDetailService;

        public FlashsaleService(IConfiguration configuration)
        {
            _configuration = configuration;
            productDetailService=new ProductDetailService(configuration);
        }
        public FlashSaleProductResposeModel CombineModel(FlashSaleProductESModel flashsale_product, ProductMongoDbModelFEResponse product)
        {
            if (flashsale_product == null|| product==null)
            {
                return null;

            }
           
            return new FlashSaleProductResposeModel()
            {
                amount = ((product.amount_min != null && product.amount_min > 0) ? (double)product.amount_min : product.amount),
                amount_after_flashsale = product.amount_after_flashsale,
                discountvalue = flashsale_product.discountvalue,
                position = flashsale_product.position,
                total_discount = product.discount,
                valuetype = flashsale_product.valuetype,
                _id = product._id,
                avatar = product.avatar,
                name = product.name,
                code = product.code
            };
        }
    }
}
