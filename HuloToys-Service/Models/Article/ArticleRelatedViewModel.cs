using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Entities;

public partial class ArticleRelatedViewModel
{
    public long Id { get; set; }

    public long? ArticleId { get; set; }

    public long? ArticleRelatedId { get; set; }


}
