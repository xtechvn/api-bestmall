using Entities.ViewModels;
using Entities.ViewModels.Products;

namespace HuloToys_Front_End.Models.Products
{
    public class ProductDetailResponseModel
    {
       public ProductMongoDbModel product_main { get; set; }
       public List<ProductMongoDbModel> product_sub { get; set; }
        public ProductDetailResponseModelCertificate cert { get; set; }
        public ProductDetailResponseModelFavourite favourite { get; set; }
        public List<ProductMongoDbModel> product_buy_with { get; set; }
        public List<ProductDetailResponseModelProductBuyWith> product_buy_with_output { get; set; }
        public ProductMongoDbModelFEResponse flashsale_main { get; set; }
        public List<ProductMongoDbModelFEResponse>  flashsale_sub { get; set; }

    }
    public class ProductDetailResponseModelCertificate {
        public List<string> root_product { get; set; }
        public List<string> product { get; set; }
        public List<string> supply { get; set; }
        public List<string> confirm { get; set; }

    } 
    public class ProductDetailResponseModelFavourite
    {
       public bool is_favourite { get; set; }
       public long count { get; set; }

    }
    public class ProductDetailResponseModelProductBuyWith
    {
        public string _id { get; set; }
        public string code { get; set; }

        public double amount { get; set; }

        public string name { get; set; }

        public string avatar { get; set; }
        public string variation_detail { get; set; }

    }
}
