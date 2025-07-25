using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class FanpageArticleImage
{
    public long Id { get; set; }

    public long ArticleId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
}
