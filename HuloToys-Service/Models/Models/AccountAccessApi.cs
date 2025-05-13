using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class AccountAccessApi
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public short Status { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateLast { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<AccountAccessApiPermission> AccountAccessApiPermissions { get; set; } = new List<AccountAccessApiPermission>();
}
