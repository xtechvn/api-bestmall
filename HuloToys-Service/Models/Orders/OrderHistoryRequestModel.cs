using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuloToys_Service.Models.Orders
{
    public class OrderHistoryRequestModel
    {
        public string token { get; set; }
        public string order_no { get; set; }
        public string status { get; set; }
        public int page_index { get; set; }
        public int page_size { get; set; }
    }
}
