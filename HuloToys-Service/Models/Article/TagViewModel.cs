using Nest;
using System;
using System.Collections.Generic;

namespace API_CORE.Controllers.Models.Article;

public partial class TagViewModel
{
    [PropertyName("Id")]

    public long Id { get; set; }
    [PropertyName("TagName")]

    public string? TagName { get; set; }
    [PropertyName("CreatedOn")]

    public DateTime? CreatedOn { get; set; }

}
