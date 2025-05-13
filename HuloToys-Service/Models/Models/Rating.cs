using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Rating
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public string? ProductId { get; set; }

    public string? ProductDetailId { get; set; }

    public decimal? Star { get; set; }

    public string? Comment { get; set; }

    public string? ImgLink { get; set; }

    public string? VideoLink { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }
}
