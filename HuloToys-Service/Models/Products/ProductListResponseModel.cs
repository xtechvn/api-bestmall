using Entities.ViewModels.Products;

namespace HuloToys_Front_End.Models.Products
{
    public class ProductListResponseModel
    {
        public List<ProductMongoDbModel> items { get; set; }
        public long count { get; set; }
    }
    public class ProductListResponseFEModel
    {
        public List<ProductMongoDbModelFEResponse> items { get; set; }
        public long count { get; set; }
    }
}
