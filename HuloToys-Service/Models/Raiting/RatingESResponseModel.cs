using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models;

public partial class RatingESResponseModel: RatingESModel
{
    public string client_avatar { get; set; }
    public string client_name { get; set; }
    public string variation_detail { get; set; }
    public List<RatingResponseComment> replies { get; set; }
}
