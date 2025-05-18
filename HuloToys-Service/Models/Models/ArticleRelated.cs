using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Models;

public partial class ArticleRelated
{
    public long Id { get; set; }

    public long? ArticleId { get; set; }

    public long? ArticleRelatedId { get; set; }

    public DateTime? UpdateLast { get; set; }

    public virtual Article? Article { get; set; }
}
