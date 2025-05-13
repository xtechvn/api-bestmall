using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Article;

public partial class ArticleCategoryViewModel
{
    public long Id { get; set; }

    public int? CategoryId { get; set; }

    public long? ArticleId { get; set; }
}
