namespace HuloToys_Service.Models.ProductsFavourites
{
    public class ProductsFavouritesListingResponseModel
    {
        public List<ProductsFavouritesMongoDbModel> items { get; set; }
        public long count { get; set; }
    }
}
