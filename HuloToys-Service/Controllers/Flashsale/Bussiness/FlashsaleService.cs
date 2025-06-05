using Entities.ViewModels.Products;
using HuloToys_Service.Models.Flashsale;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Flashsale.Bussiness
{
    public class FlashsaleService
    {
        private readonly IConfiguration _configuration;
        public FlashsaleService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public FlashSaleProductResposeModel CombineModel(FlashSaleProductESModel flashsale_product, ProductMongoDbModel product)
        {
            if (flashsale_product == null|| product==null)
            {
                return null;

            }
            double total_discount = 0;
            var amount = (product.amount_min != null && product.amount_min > 0) ? (double)product.amount_min : product.amount;
            switch (flashsale_product.valuetype)
            {
                case (int)FlashSaleValueType.VND:
                    {
                        total_discount = flashsale_product.discountvalue==null?0: (double)flashsale_product.discountvalue;
                    }break;
                case (int)FlashSaleValueType.PERCENT:
                    {
                        var percent = flashsale_product.discountvalue == null ? 0 : (double)flashsale_product.discountvalue;
                        
                        total_discount=amount*percent/(double)100;
                        if (total_discount > 0) total_discount = Math.Round(total_discount, 0);
                    }
                    break;
            }
            return new FlashSaleProductResposeModel()
            {
                amount = amount,
                amount_after_flashsale = amount- total_discount,
                discountvalue = flashsale_product.discountvalue,
                position = flashsale_product.position,
                total_discount = total_discount,
                valuetype = flashsale_product.valuetype,
                _id = product._id,
                avatar = product.avatar,
                name = product.name,
                code = product.code
            };
        }
    }
}
