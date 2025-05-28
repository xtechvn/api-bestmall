namespace HuloToys_Service.Models.Article
{
    public partial class ArticleGroupViewModel
    {
        public long? id { get; set; }
        public int parentid { get; set; }
        public int? positionid { get; set; }

        public string name { get; set; }
        public string image_path { get; set; }
        public string url_path { get; set; }
        public int order_no { get; set; }
        public int? status { get; set; }

        public DateTime? createdon { get; set; }

        public DateTime? modifiedon { get; set; }


        public string? description { get; set; }

        public bool isshowheader { get; set; }
        public bool isshowfooter { get; set; }

        public List<ArticleGroupViewModel> group_product_child { get; set; }

    }
    public class ProductGroupViewModel
    {
        public int id { get; set; }
        public int code { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public string image { get; set; }

    }
}
