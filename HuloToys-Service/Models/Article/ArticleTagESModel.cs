using Nest;

namespace API_CORE.Controllers.Models.Article
{
    public class ArticleTagESModel
    {
        [PropertyName("Id")]

        public long Id { get; set; }
        [PropertyName("TagId")]

        public long? TagId { get; set; }
        [PropertyName("ArticleId")]

        public long? ArticleId { get; set; }



    }
}
