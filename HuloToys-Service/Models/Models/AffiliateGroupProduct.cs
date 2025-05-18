using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class AffiliateGroupProduct
{
    public int Id { get; set; }

    public int GroupProductId { get; set; }

    public int AffType { get; set; }
}
