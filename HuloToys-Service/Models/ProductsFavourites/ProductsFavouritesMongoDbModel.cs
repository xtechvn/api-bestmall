using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Entities.ViewModels.Products;

namespace HuloToys_Service.Models.ProductsFavourites
{
    public class ProductsFavouritesMongoDbModel
    {
        [BsonElement("_id")]
        public string _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }
        public long account_client_id { get; set; }
        public string product_id { get; set; }
        public DateTime updated_last { get; set; }
        public ProductMongoDbModel detail { get; set; }
    }
}
