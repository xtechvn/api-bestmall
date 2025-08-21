using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ENTITIES.ViewModels.Notify
{    
   public class ReceiverMessageViewModel
    {
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId();
        }
       
        public int seen_status { get; set; } // trạng thái xem notify 0: chua xem, 1: da xem tong quan, 2 da xem detail
       
        public string notify_id { get; set; } // Thông tin notify
       
        public double seen_date { get; set; } // Ngày mà user đó vào xem notify
       
        public int user_receiver_id { get; set; } // user sẽ nhận notify
       
        public string link_redirect { get; set; } // link se gắn vào item noti khi click vào để chuyển hướng
       
        public string content { get; set; }
       
        public int company_type { get; set; }

    }
}
