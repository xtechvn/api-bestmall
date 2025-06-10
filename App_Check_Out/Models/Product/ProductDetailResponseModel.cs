using Entities.ViewModels;
using Entities.ViewModels.Products;

namespace APP_CHECKOUT.Models.Models
{
    public class ProductDetailResponseModel
    {
       public ProductMongoDbModel product_main { get; set; }
       public List<ProductMongoDbModel> product_sub { get; set; }

    }
}
