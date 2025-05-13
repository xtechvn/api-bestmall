using Nest;
using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models;

public partial class RatingESModel
{
    [PropertyName("Id")]

    public int Id { get; set; }
    [PropertyName("OrderId")]

    public int? OrderId { get; set; }
    [PropertyName("ProductId")]

    public string ProductId { get; set; }
    [PropertyName("ProductDetailId")]

    public string ProductDetailId { get; set; }
    [PropertyName("Star")]

    public decimal? Star { get; set; }
    [PropertyName("Comment")]

    public string Comment { get; set; }
    [PropertyName("ImgLink")]

    public string ImgLink { get; set; }
    [PropertyName("VideoLink")]

    public string VideoLink { get; set; }
    [PropertyName("UserId")]

    public int? UserId { get; set; }
    [PropertyName("CreatedDate")]

    public DateTime? CreatedDate { get; set; }
    [PropertyName("UpdatedDate")]

    public DateTime? UpdatedDate { get; set; }
    [PropertyName("UpdatedBy")]

    public int? UpdatedBy { get; set; }
}
