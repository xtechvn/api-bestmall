using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class LocationProduct
{
    public long LocationProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public int GroupProductId { get; set; }

    public int OrderNo { get; set; }

    public DateTime CreateOn { get; set; }

    public DateTime UpdateLast { get; set; }

    public int UserId { get; set; }
}
