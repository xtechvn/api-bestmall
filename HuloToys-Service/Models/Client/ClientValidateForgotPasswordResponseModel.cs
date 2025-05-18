using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuloToys_Service.Models.Client
{
    public class ClientValidateForgotPasswordResponseModel
    {
        public string email { get; set; }
        public long account_client_id { get; set; }
        public long client_id { get; set; }
    }
}
