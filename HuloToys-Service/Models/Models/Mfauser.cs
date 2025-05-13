using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Mfauser
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public short Status { get; set; }

    public string SecretKey { get; set; } = null!;

    public string BackupCode { get; set; } = null!;

    public string? UserCreatedYear { get; set; }

    public DateTime? UpdateTime { get; set; }
}
