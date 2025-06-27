using Nest;
using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Province
{
    [PropertyName("Id")]

    public int Id { get; set; }
    [PropertyName("ProvinceId")]

    public string ProvinceId { get; set; } = null!;
    [PropertyName("Name")]

    public string Name { get; set; } = null!;
    [PropertyName("NameNonUnicode")]

    public string? NameNonUnicode { get; set; }
    [PropertyName("Type")]

    public string Type { get; set; } = null!;
    [PropertyName("Status")]

    public short? Status { get; set; }
    [PropertyName("CreatedDate")]

    public DateTime? CreatedDate { get; set; }
}
