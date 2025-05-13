using Entities.Models;
using HuloToys_Service.Models.NinjaVan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.APIRequest
{
    public class CartConfirmRequestModel
    {
        public string token  { get; set; }
        public int payment_type { get; set; }
        public ShippingFeeRequestModel delivery_detail { get; set; }
        public List<CartConfirmItemRequestModel> carts { get; set; }
        public AddressClientFEModel address { get; set; }
        public long address_id { get; set; }

    }
    public class CartConfirmItemRequestModel
    {
        public string id { get; set; }
        public int quanity { get; set; }
    }
}
