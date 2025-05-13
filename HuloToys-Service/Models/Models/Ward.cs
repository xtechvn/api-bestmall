using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Ward
{
    public int Id { get; set; }

    public string WardId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? NameNonUnicode { get; set; }

    public string Type { get; set; } = null!;

    public string? Location { get; set; }

    public string DistrictId { get; set; } = null!;

    public short? Status { get; set; }

    public DateTime? CreatedDate { get; set; }
}
