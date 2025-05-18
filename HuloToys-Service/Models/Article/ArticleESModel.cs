using Nest;

namespace HuloToys_Service.Models.Article
{
    public class ArticleESModel
    {
        [PropertyName("Id")]
        public long Id { get; set; }
        [PropertyName("Title")]

        public string Title { get; set; }
        [PropertyName("Lead")]

        public string Lead { get; set; }
        [PropertyName("Body")]

        public string Body { get; set; }
        [PropertyName("Status")]

        public int Status { get; set; }
        [PropertyName("ArticleType")]

        public int ArticleType { get; set; }
        [PropertyName("PageView")]

        public int? PageView { get; set; }
        [PropertyName("PublishDate")]

        public DateTime? PublishDate { get; set; }
        [PropertyName("AuthorId")]

        public int? AuthorId { get; set; }
        [PropertyName("Image169")]

        public string Image169 { get; set; }
        [PropertyName("Image43")]

        public string Image43 { get; set; }
        [PropertyName("Image11")]

        public string Image11 { get; set; }
        [PropertyName("CreatedOn")]

        public DateTime? CreatedOn { get; set; }
        [PropertyName("ModifiedOn")]

        public DateTime? ModifiedOn { get; set; }
        [PropertyName("DownTime")]

        public DateTime? DownTime { get; set; }
        [PropertyName("UpTime")]

        public DateTime? UpTime { get; set; }
        [PropertyName("Position")]

        public short? Position { get; set; }

    }
}
