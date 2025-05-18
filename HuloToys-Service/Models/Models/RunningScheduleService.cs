using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class RunningScheduleService
{
    public int Id { get; set; }

    public int? PriceId { get; set; }

    public DateTime? LogDate { get; set; }

    public virtual Campaign? Price { get; set; }
}
