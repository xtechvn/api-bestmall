using Entities.Models;
using Entities.ViewModels.Products;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models.MongoDb
{
    public class ClientContactMongoDbModel
    {
        [BsonElement("_id")]
        public string _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId(DateTime.Now).ToString();
        }
        public string email { get; set; }
        public string phone { get; set; }
        public string message { get; set; }
        public DateTime created_date { get; set; }
        

    }
}
