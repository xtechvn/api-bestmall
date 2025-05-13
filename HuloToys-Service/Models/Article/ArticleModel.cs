using Nest;

namespace HuloToys_Service.Models.Article
{
    public class ArticleModel2:CategoryArticleModel // Kế thừa thuộc tính của box tin
    {
        [PropertyName("Body")]
        public string body { get; set; }
    }
}
