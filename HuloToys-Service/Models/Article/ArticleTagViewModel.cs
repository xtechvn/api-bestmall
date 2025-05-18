using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Article;

public partial class ArticleTagViewModel
{
    public long Id { get; set; }

    public long? TagId { get; set; }

    public long? ArticleId { get; set; }

}
