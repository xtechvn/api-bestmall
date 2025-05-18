using Nest;

namespace HuloToys_Service.Models.Article
{
    public class ArticleRelatedESmodel
    {
        [PropertyName("Id")]

        public long Id { get; set; }
        [PropertyName("ArticleId")]

        public long? ArticleId { get; set; }
        [PropertyName("ArticleRelatedId")]

        public long? ArticleRelatedId { get; set; }

    }
}
