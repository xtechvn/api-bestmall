using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class BankOnePay
{
    public int Id { get; set; }

    public string BankName { get; set; } = null!;

    public string Code { get; set; } = null!;

    public byte Type { get; set; }

    public string Logo { get; set; } = null!;

    public byte? Status { get; set; }

    public string? FullnameEn { get; set; }

    public string? FullnameVi { get; set; }
}
