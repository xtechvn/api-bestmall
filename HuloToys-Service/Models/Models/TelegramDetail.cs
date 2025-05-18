using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class TelegramDetail
{
    public int Id { get; set; }

    public string GroupLog { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string GroupChatId { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public int ProjectType { get; set; }

    public int Status { get; set; }
}
