using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public DateTime CreateDate { get; set; }

    public long ClientId { get; set; }

    public int ServiceType { get; set; }

    public double Amount { get; set; }

    public string ContractNo { get; set; } = null!;

    public int Status { get; set; }

    public int UserVerifyId { get; set; }

    public DateTime VerifyDate { get; set; }

    public string BankReference { get; set; } = null!;

    public int PaymentType { get; set; }

    public string Description { get; set; } = null!;

    public string? TransactionNo { get; set; }
}
