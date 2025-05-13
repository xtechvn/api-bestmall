using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_Push_Consummer.Model.Comments
{
    public class ProductRaitingPushQueueModel
    {
        public long OrderId { get; set; }
        public string ProductId { get; set; }
        public string ProductDetailId { get; set; }
        public float Star { get; set; }
        public string Comment { get; set; }
        public string ImgLink { get; set; }
        public string VideoLink { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
