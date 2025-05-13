using Entities.ViewModels.Products;

namespace HuloToys_Front_End.Models.Products
{
    public class ProductAddToCartRequestModel
    {
        public string product_id { get; set; }
        public string token { get; set; }
        public int quanity { get; set; }
    }
}
