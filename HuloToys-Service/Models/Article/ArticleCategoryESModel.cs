using Nest;

namespace API_CORE.Controllers.Models.Article
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
