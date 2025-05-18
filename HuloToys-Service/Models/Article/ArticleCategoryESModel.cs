using Nest;

namespace HuloToys_Service.Models.Article
{
    public class ArticleCategoryESModel
    {
        [PropertyName("Id")]

        public long Id { get; set; }
        [PropertyName("CategoryId")]

        public int? categoryid { get; set; }
        [PropertyName("ArticleId")]

        public long? articleid { get; set; }
    }
}
