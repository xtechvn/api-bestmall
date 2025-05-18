using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class Tag
{
    public long Id { get; set; }

    public string? TagName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
