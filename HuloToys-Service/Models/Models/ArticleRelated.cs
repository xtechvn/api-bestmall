using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class ArticleRelated
{
    public long Id { get; set; }

    public long? ArticleId { get; set; }

    public long? ArticleRelatedId { get; set; }

    public DateTime? UpdateLast { get; set; }

    public virtual Article? Article { get; set; }
}
