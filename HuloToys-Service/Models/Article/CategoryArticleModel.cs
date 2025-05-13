using Nest;

namespace HuloToys_Service.Models.Article
{
    public class CategoryArticleModel // box tin
    {
        [PropertyName("Id")]
        public long id { get; set; }

        [PropertyName("Title")]
        public string title { get; set; }

        [PropertyName("Lead")]
        public string lead { get; set; }



        [PropertyName("Status")]
        public int status { get; set; }

        [PropertyName("Image169")]
        public string image_169 { get; set; } = null!;

        [PropertyName("Image43")]
        public string image_43 { get; set; } = null!;

        [PropertyName("Image11")]
        public string image_11 { get; set; } = null!;

        [PropertyName("PublishDate")]
        public DateTime publish_date { get; set; }
        [PropertyName("Position")]
        public int? position { get; set; }



        [PropertyName("PageView")]
        public int? pageview { get; set; }

        [PropertyName("ListCategoryId")]
        public string list_category_id { get; set; }

        [PropertyName("ListCategoryName")]
        public string list_category_name { get; set; }
    }
}
