using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class MenuPermission
{
    public int Id { get; set; }

    public int MenuId { get; set; }

    public int PermissionId { get; set; }
}
