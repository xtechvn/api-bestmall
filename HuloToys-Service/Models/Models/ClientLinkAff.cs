using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class ClientLinkAff
{
    public int Id { get; set; }

    public DateTime CreateDate { get; set; }

    public string LinkAff { get; set; } = null!;

    public long ClientId { get; set; }
}
