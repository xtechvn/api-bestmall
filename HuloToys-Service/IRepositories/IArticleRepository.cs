using Entities.ViewModels;
using ENTITIES.ViewModels.ArticleViewModels;
using HuloToys_Service.Models.Article;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HuloToys_Service.Controllers.IRepositories
{
    public interface IArticleRepository
    {
        GenericViewModel<ArticleViewModel> GetPagingList(ArticleSearchModel searchModel, int currentPage, int pageSize);
        Task<long> SaveArticle(ArticleModel model, string url_upload, string key_encrypt);
        Task<ArticleModel> GetArticleDetail(long Id);
        Task<long> ChangeArticleStatus(long Id, int Status);
        Task<List<string>> GetSuggestionTag(string name);
        Task<List<ArticleFeModel>> getArticleListByCategoryId(int cate_id);
        Task<ArticleFeDetailModel> GetArticleDetailLite(long article_id);
        Task<List<ArticleRelationModel>> FindArticleByTitle(string title,int parent_cate_faq_id);
        Task<List<int>> GetArticleCategoryIdList(long Id);
        Task<long> DeleteArticle(long Id);
        Task<ArticleFEModelPagnition> getArticleListByCategoryIdOrderByDate(int cate_id, int skip, int take, string category_name);
        Task<ArticleFeModel> GetArticleDetailLiteFE(long article_id);
        Task<ArticleFeModel> GetMostViewedArticle(long article_id);
        Task<ArticleFeModel> GetPinnedArticleByposition(int cate_id, string category_name, int position);
        Task<ArticleFEModelPagnition> getArticleListByTags(string tag, int skip, int take);
        Task<ArticleFEModelPagnition> getFooterArticleListByCategory(int cate_id, int skip, int take, string category_name);
    }
}
