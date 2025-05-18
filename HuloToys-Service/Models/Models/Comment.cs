using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
