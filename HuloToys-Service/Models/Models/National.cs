using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class National
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? NameVn { get; set; }

    public string? Code { get; set; }

    public DateTime? UpdateTime { get; set; }
}
