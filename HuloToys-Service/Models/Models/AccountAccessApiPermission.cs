using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class AccountAccessApiPermission
{
    public int Id { get; set; }

    public int? AccountAccessApiId { get; set; }

    public int? ProjectType { get; set; }

    public virtual AccountAccessApi? AccountAccessApi { get; set; }

    public virtual AllCode? ProjectTypeNavigation { get; set; }
}
