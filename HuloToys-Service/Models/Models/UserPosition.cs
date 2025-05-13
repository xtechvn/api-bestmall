using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class UserPosition
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public short Rank { get; set; }

    public short? OrderBy { get; set; }
}
