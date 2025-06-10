using Entities.ViewModels.Products;

namespace HuloToys_Service.Models.Label
{
    public class ProductListingByLabelResponseModel
    {
        public HuloToys_Service.Models.Models.Label label_detail { get; set; }   
        public List<ProductMongoDbModel> items { get; set; }
        public long count { get; set; }
    }
}
