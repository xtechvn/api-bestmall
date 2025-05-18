using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class Position
{
    public int Id { get; set; }

    public string PositionName { get; set; } = null!;

    public int Width { get; set; }

    public int Height { get; set; }
}
