using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ENTITIES.ViewModels.Notify
{    
    public class MessageViewModel
    {
        [BsonElement("_id")]
        public string _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }
        
        public string content { get; set; } // nội dung notify
        
        public double send_date { get; set; } // thời gian gửi
        
        public string user_name_send { get; set; } //người gửi
        
        public int user_id_send { get; set; } //id nguoi gui
        
        public string code { get; set; } //  mã đối tượng gửi. Là khóa chính của module
        
        public int module_type { get; set; } // loại module 
    }
}
