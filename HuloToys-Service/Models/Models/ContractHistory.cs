using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class ContractHistory
{
    public long Id { get; set; }

    public long ContractId { get; set; }

    public string Action { get; set; } = null!;

    public int? ActionBy { get; set; }

    public DateTime? ActionDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }
}
