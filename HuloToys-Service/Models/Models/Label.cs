using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Label
{
    public int Id { get; set; }

    public string? LabelName { get; set; }

    public string? LabelCode { get; set; }

    public int? SupplierId { get; set; }

    public string? Icon { get; set; }

    public int? ParentId { get; set; }

    public int? Level { get; set; }

    public string? Description { get; set; }

    public short? Status { get; set; }

    public DateTime? CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }
}
