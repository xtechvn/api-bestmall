using Entities.ViewModels;
using Entities.ViewModels.Products;

namespace HuloToys_Front_End.Models.Products
{
    public class ProductDetailResponseModel
    {
       public ProductMongoDbModel product_main { get; set; }
       public List<ProductMongoDbModel> product_sub { get; set; }

    }
}
