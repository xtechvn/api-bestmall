using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuloToys_Service.Models.Address
{
    public class CommentsModel
    {
        public long AccountClientId { get; set; }
        public string Content { get; set; }
        public string Email { get; set; }
        public long Type_Queue { get; set; }
    }
}
