using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Article
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Lead { get; set; } = null!;

    public string Body { get; set; } = null!;

    public int Status { get; set; }

    public int ArticleType { get; set; }

    public int? PageView { get; set; }

    public DateTime? PublishDate { get; set; }

    public int? AuthorId { get; set; }

    public string Image169 { get; set; } = null!;

    public string? Image43 { get; set; }

    public string? Image11 { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DownTime { get; set; }

    public DateTime? UpTime { get; set; }

    public short? Position { get; set; }

    public virtual ICollection<ArticleRelated> ArticleRelateds { get; set; } = new List<ArticleRelated>();

    public virtual ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
