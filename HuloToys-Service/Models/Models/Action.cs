﻿using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Action
{
    public int Id { get; set; }

    public string ControllerName { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public int? PermissionId { get; set; }

    public int? MenuId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual Menu? Menu { get; set; }

    public virtual Permission? Permission { get; set; }
}
