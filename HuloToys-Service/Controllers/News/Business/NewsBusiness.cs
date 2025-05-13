using Entities.ConfigModels;
using Entities.Models;
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Models.Products;
using HuloToys_Service.Utilities.Lib;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.News.Business
{
    public partial class NewsBusiness
    {
        public IConfiguration configuration;
        public ArticleESService articleESService;
        public ArticleTagESService articleTagESService;
        public TagESService tagESService;
        public ArticleCategoryESService articleCategoryESService;
        public ArticleRelatedESService articleRelatedESService;
        public GroupProductESService groupProductESService;
        private readonly DataMSContext _dbContext;
        private readonly string _UrlStaticImage = "https://static-image.adavigo.com";
        public NewsBusiness( IConfiguration _configuration, DataMSContext dbContext)
        {

            configuration = _configuration;
            articleESService = new ArticleESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            articleTagESService = new ArticleTagESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            tagESService = new TagESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            articleCategoryESService = new ArticleCategoryESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            articleRelatedESService = new ArticleRelatedESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            groupProductESService = new GroupProductESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            _dbContext = dbContext;
            
        }

        public async Task<ResultModel<long>> SaveArticle(ArticleModel model)
        {
            var result = new ResultModel<long>();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xử lý ảnh
                    Task<string> TBody = Task.FromResult(model.Body);
                    if (model.ArticleType == 0)
                    {
                        TBody = StringHelpers.ReplaceEditorImage(model.Body, _UrlStaticImage);
                        var t11 = UpLoadHelper.UploadBase64Src(model.Image11, _UrlStaticImage);
                        var t43 = UpLoadHelper.UploadBase64Src(model.Image43, _UrlStaticImage);
                        var t169 = UpLoadHelper.UploadBase64Src(model.Image169, _UrlStaticImage);
                        await Task.WhenAll(TBody, t11, t43, t169);

                        model.Body = TBody.Result;
                        model.Image11 = t11.Result;
                        model.Image43 = t43.Result;
                        model.Image169 = t169.Result;
                    }

                    if (!string.IsNullOrEmpty(model.Image11) && !model.Image11.Contains(_UrlStaticImage))
                        model.Image11 = _UrlStaticImage + model.Image11;
                    if (!string.IsNullOrEmpty(model.Image43) && !model.Image43.Contains(_UrlStaticImage))
                        model.Image43 = _UrlStaticImage + model.Image43;
                    if (!string.IsNullOrEmpty(model.Image169) && !model.Image169.Contains(_UrlStaticImage))
                        model.Image169 = _UrlStaticImage + model.Image169;

                    if (model.PublishDate == DateTime.MinValue)
                        model.PublishDate = DateTime.Now;
                    if (model.DownTime == DateTime.MinValue)
                        model.DownTime = model.PublishDate.AddHours(1);

                    long articleId;

                    // Create hoặc Update Article
                    if (model.Id > 0)
                    {
                        var entity = await _dbContext.Articles.FindAsync(model.Id);
                        if (entity == null)
                        {
                            result.IsSuccess = false;
                            result.Message = "Không tìm thấy bài viết để cập nhật.";
                            return result;
                        }

                        entity.Title = model.Title;
                        entity.Lead = model.Lead;
                        entity.Body = model.Body;
                        entity.Image11 = model.Image11 ?? string.Empty;
                        entity.Image43 = model.Image43 ?? string.Empty;
                        entity.Image169 = model.Image169 ?? string.Empty;
                        entity.Status = model.Status;
                        entity.ArticleType = (int)model.ArticleType;
                        entity.ModifiedOn = DateTime.Now;
                        entity.PublishDate = model.PublishDate;
                        entity.UpTime = model.PublishDate;
                        entity.DownTime = model.DownTime;
                        entity.Position = (short?)model.Position;

                        _dbContext.Articles.Update(entity);
                        await _dbContext.SaveChangesAsync();

                        articleId = entity.Id;
                    }
                    else
                    {
                        var entity = new Article
                        {
                            Title = model.Title,
                            Lead = model.Lead,
                            Body = model.Body,
                            Status = model.Status,
                            Image11 = model.Image11 ?? string.Empty,
                            Image43 = model.Image43 ?? string.Empty,
                            Image169 = model.Image169 ?? string.Empty,
                            ArticleType = (int)model.ArticleType,
                            AuthorId = model.AuthorId,
                            CreatedOn = DateTime.Now,
                            ModifiedOn = DateTime.Now,
                            PublishDate = model.PublishDate,
                            UpTime = model.PublishDate,
                            DownTime = model.DownTime,
                            Position = (short?)model.Position
                        };

                        await _dbContext.Articles.AddAsync(entity);
                        await _dbContext.SaveChangesAsync();

                        articleId = entity.Id;
                    }

                    // ⬇ Lưu Tags
                    var tagIds = new List<long>();
                    if (model.Tags != null)
                    {
                        foreach (var tagName in model.Tags)
                        {
                            var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.TagName == tagName.Trim());
                            if (tag == null)
                            {
                                tag = new Tag
                                {
                                    TagName = tagName.Trim(),
                                    CreatedOn = DateTime.Now
                                };
                                await _dbContext.Tags.AddAsync(tag);
                                await _dbContext.SaveChangesAsync();
                            }
                            tagIds.Add(tag.Id);
                        }
                    }

                    // ⬇ Xóa & Thêm ArticleTag
                    var oldTags = _dbContext.ArticleTags.Where(x => x.ArticleId == articleId);
                    _dbContext.ArticleTags.RemoveRange(oldTags);
                    await _dbContext.SaveChangesAsync();

                    foreach (var tagId in tagIds)
                    {
                        var newTag = new ArticleTag
                        {
                            ArticleId = articleId,
                            TagId = tagId,
                            UpdateLast = DateTime.Now
                        };
                        await _dbContext.ArticleTags.AddAsync(newTag);
                    }
                    await _dbContext.SaveChangesAsync();

                    // ⬇ Xóa & Thêm ArticleCategory
                    var oldCategories = _dbContext.ArticleCategories.Where(x => x.ArticleId == articleId);
                    _dbContext.ArticleCategories.RemoveRange(oldCategories);
                    await _dbContext.SaveChangesAsync();

                    foreach (var cateId in model.Categories ?? new List<int>())
                    {
                        var cate = new ArticleCategory
                        {
                            ArticleId = articleId,
                            CategoryId = cateId,
                            UpdateLast = DateTime.Now
                        };
                        await _dbContext.ArticleCategories.AddAsync(cate);
                    }
                    await _dbContext.SaveChangesAsync();

                    // ⬇ Xóa & Thêm ArticleRelated
                    var oldRelations = _dbContext.ArticleRelateds.Where(x => x.ArticleId == articleId);
                    _dbContext.ArticleRelateds.RemoveRange(oldRelations);
                    await _dbContext.SaveChangesAsync();

                    foreach (var relatedId in model.RelatedArticleIds ?? new List<long>())
                    {
                        var relation = new ArticleRelated
                        {
                            ArticleId = articleId,
                            ArticleRelatedId = relatedId,
                            UpdateLast = DateTime.Now
                        };
                        await _dbContext.ArticleRelateds.AddAsync(relation);
                    }
                    await _dbContext.SaveChangesAsync();

                    // Commit all
                    await transaction.CommitAsync();

                    result.IsSuccess = true;
                    result.Data = articleId;
                    result.Message = "Lưu bài viết thành công";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    LogHelper.InsertLogTelegram("SaveArticle - Transaction Rollback: " + ex);
                    result.IsSuccess = false;
                    result.Message = "Lỗi khi lưu bài viết: " + ex.Message;
                }
            }

            return result;
        }





        public async Task<List<CategoryArticleModel>> getListNews(int category_id, int take)
        {
            var list_article = new List<CategoryArticleModel>();
            try
            {
                // Lấy ra danh sách id các bài viết mới nhất
                var obj_top_story = articleESService.getListNews(category_id, take);

                return obj_top_story;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "getArticleListByCategoryId - ArticleDAL: " + ex);
                return list_article;
            }
        }
        public async Task<ArticleModel2> getArticleDetail(long article_id)
        {
            try
            {
                var article_list = articleESService.GetArticleDetailById(article_id);
                return article_list;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetArticleCategoryByParentID -GroupProductRepository : " + ex);
            }
            return null;
        }
        public async Task<int> getTotalItemNewsByCategoryId(int cate_id)
        {
            var list_article = new List<CategoryArticleModel>();
            try
            {
                // Lấy ra danh sách id các bài viết theo chuyên mục
                var total = articleESService.getTotalItemNewsByCategoryId(cate_id);

                return total;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "getArticleListByCategoryId - ArticleDAL: " + ex);
                return 0;
            }
        }



        public async Task<ArticleModel> GetArticleDetail(long Id)
        {
            try
            {
                var model = new ArticleModel();

                try
                {
                    var article = articleESService.GetDetailById(Id);
                    model = new ArticleModel
                    {
                        Id = article.Id,
                        Title = article.Title,
                        Lead = article.Lead,
                        Body = article.Body,
                        Status = article.Status,
                        ArticleType = article.ArticleType,
                        Image11 = article.Image11,
                        Image43 = article.Image43,
                        Image169 = article.Image169,
                        PublishDate = article.PublishDate ?? DateTime.MinValue,
                        DownTime = article.DownTime ?? DateTime.MinValue,
                        Position = article.Position ?? 0
                    };
                    var data_ArticleTag_By_ArticleId = articleTagESService.GetListArticleTagByArticleId(article.Id);
                    var TagIds = data_ArticleTag_By_ArticleId.Select(s => s.TagId).ToList();
                    var List_tag = tagESService.GetListTag();
                    var List_articleCategory = articleCategoryESService.GetByArticleId(article.Id);
                    var List_relatedArticleIds = articleRelatedESService.GetListArticleRelatedByArticleId(article.Id);
                    model.Tags = List_tag.Where(s => TagIds.Contains(s.Id)).Select(s => s.TagName).ToList();
                    model.Categories = List_articleCategory.Select(s => (int)s.categoryid).ToList();
                    model.MainCategory = model.Categories != null || model.Categories.Count > 0 ? model.Categories[0] : -1;
                    model.RelatedArticleIds = List_relatedArticleIds.Select(s => (long)s.ArticleRelatedId).ToList();

                    if (model.RelatedArticleIds != null && model.RelatedArticleIds.Count > 0)
                    {
                        foreach (var item in model.RelatedArticleIds)
                        {
                            var groupProductName = string.Empty;
                            var detail_article = articleESService.GetDetailById(item);
                            var articleCategory = articleCategoryESService.GetByArticleId(detail_article.Id);
                            if (articleCategory != null && articleCategory.Count > 0)
                            {
                                foreach (var item2 in articleCategory)
                                {
                                    var groupProduct = groupProductESService.GetDetailGroupProductById((long)item2.categoryid);
                                    groupProductName += groupProduct.Name + ",";
                                }
                            }

                            var ArticleRelation = new ArticleRelationModel
                            {
                                Id = detail_article.Id,
                                Image = detail_article.Image169 ?? detail_article.Image43 ?? detail_article.Image11,
                                Title = detail_article.Title,
                                publish_date = detail_article.PublishDate ?? DateTime.Now,
                                category_name = groupProductName ?? "Tin tức"
                            };
                            model.RelatedArticleList.Add(ArticleRelation);
                        }
                        model.RelatedArticleList = model.RelatedArticleList.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                    }


                }
                catch
                {

                    return null;
                }

                return model;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// cuonglv
        /// Lấy ra danh sách các bài thuộc 1 chuyên mục
        /// </summary>
        /// <param name="cate_id"></param>
        /// <returns></returns>
        public async Task<List<ArticleFeModel>> getArticleListByCategoryId(int cate_id)
        {
            try
            {
                var list_article = new List<ArticleFeModel>();

                try
                {
                    var List_articleCategory = articleCategoryESService.GetByCategoryId(cate_id);
                    if (List_articleCategory != null && List_articleCategory.Count > 0)
                    {
                        foreach (var item in List_articleCategory)
                        {
                            var _article = articleESService.GetDetailById((long)item.articleid);
                            if (_article == null) continue;
                          
                            if (_article.Status == ArticleStatus.PUBLISH)
                            {
                                var detail_model = new ArticleFeModel
                                {
                                    id = _article.Id,
                                    title = _article.Title,
                                    lead = _article.Lead,
                                    image_169 = _article.Image169,
                                    image_43 = _article.Image43,
                                    image_11 = _article.Image11,
                                    publish_date = (DateTime)_article.PublishDate,
                                    body = _article.Body
                                };
                                list_article.Add(detail_model);
                            }
                               
                        }
                    }
                    if(list_article!=null && list_article.Count > 0)
                    {
                        list_article = list_article.OrderBy(x => x.modifiedon).ToList();
                    }
            
                    return list_article;
                }
                catch
                {

                    return null;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "getArticleListByCategoryId - ArticleDAL: " + ex);
                return null;
            }
        }

        /// <summary>
        /// cuonglv
        /// Lấy ra chi tiết bài viết
        /// </summary>
        /// <param name="article_id"></param>
        /// <returns></returns>
        public async Task<ArticleFeDetailModel> GetArticleDetailLite(long article_id)
        {
            try
            {
                var model = new ArticleFeDetailModel();

                var article = articleESService.GetDetailById(article_id);
                if (article == null) return null;
                model = new ArticleFeDetailModel
                {
                    id = article.Id,
                    title = article.Title,
                    lead = article.Lead,
                    body = article.Body,
                    status = article.Status,
                    article_type = article.ArticleType,
                    image_11 = article.Image11 ?? string.Empty,
                    image_43 = article.Image43 ?? string.Empty,
                    image_169 = article.Image169 ?? string.Empty,
                    publish_date = article.PublishDate ?? DateTime.Now,
                    author_id = article.AuthorId ?? -1,
                };

                var data_ArticleTag_By_ArticleId = articleTagESService.GetListArticleTagByArticleId(article.Id);
                var TagIds = data_ArticleTag_By_ArticleId.Select(s => s.TagId).ToList();
                var List_tag = tagESService.GetListTag();
                var List_articleCategory = articleCategoryESService.GetByArticleId(article.Id);
                var List_relatedArticleIds = articleRelatedESService.GetListArticleRelatedByArticleId(article.Id);



                model.Categories = List_articleCategory.Select(s => (int)s.categoryid).ToList();
                model.category_id = model.Categories != null || model.Categories.Count > 0 ? model.Categories[0] : -1;
                model.MainCategory = model.Categories != null || model.Categories.Count > 0 ? model.Categories[0] : -1;
                var group_product = groupProductESService.GetDetailGroupProductById(model.MainCategory);
                if (group_product == null || group_product.Id < 0) return null;

                if (model.MainCategory > 0)
                {
                    model.category_name = group_product.Name;
                }
                else
                {
                    model.category_name = "Tin tức";
                }
                model.RelatedArticleIds = List_relatedArticleIds != null ? List_relatedArticleIds.Select(s => (long)s.ArticleRelatedId).ToList() : null;

                if (model.RelatedArticleIds != null && model.RelatedArticleIds.Count > 0)
                {
                    foreach (var item in model.RelatedArticleIds)
                    {
                        var groupProductName = string.Empty;
                        var detail_article = articleESService.GetDetailById(item);
                        var articleCategory = articleCategoryESService.GetByArticleId(detail_article.Id);
                        if (articleCategory != null && articleCategory.Count > 0)
                        {
                            foreach (var item2 in articleCategory)
                            {
                                var groupProduct = groupProductESService.GetDetailGroupProductById((long)item2.categoryid);
                                groupProductName += groupProduct.Name + ",";
                            }
                        }

                        var ArticleRelation = new ArticleRelationModel
                        {
                            Id = detail_article.Id,
                            Image = detail_article.Image169 ?? detail_article.Image43 ?? detail_article.Image11,
                            Title = detail_article.Title,
                            publish_date = detail_article.PublishDate ?? DateTime.Now,
                            category_name = groupProductName ?? "Tin tức"
                        };
                        model.RelatedArticleList ??= new List<ArticleRelationModel>();
                        model.RelatedArticleList.Add(ArticleRelation);

                    }
                    model.RelatedArticleList = model.RelatedArticleList.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                }

                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[article_id = " + article_id + "]GetArticleDetailLite - ArticleDAL: " + ex);
                return null;
            }
        }

        /// <summary>
        /// cuonglv
        /// Lọc những bài faq theo title
        /// </summary>
        /// <param name="article_id"></param>
        /// <returns></returns>
        public async Task<List<ArticleRelationModel>> FindArticleByTitle(string title, int parent_cate_faq_id)
        {
            try
            {
                var list_article = new List<ArticleRelationModel>();

                try
                {
                    var arr_cate_child_help_id = new List<int>();
                    var group_product_detail = groupProductESService.GetDetailGroupProductById(parent_cate_faq_id);
                    if (group_product_detail == null)
                    {
                        var group_product_list = groupProductESService.GetListGroupProductByParentId(parent_cate_faq_id);
                        arr_cate_child_help_id = group_product_list.Select(x => x.Id).ToList();
                    }
                    arr_cate_child_help_id.Add(group_product_detail.Id);



                    if (arr_cate_child_help_id.Count() > 0)
                    {
                        foreach (var item in arr_cate_child_help_id)
                        {
                            var groupProductName = string.Empty;
                            var DetailGroupProductById = groupProductESService.GetDetailGroupProductById(item);
                            if (DetailGroupProductById.IsShowHeader == true)
                            {
                                groupProductName += DetailGroupProductById.Name + ",";
                            }
                            var List_articleCategory = articleCategoryESService.GetByCategoryId(DetailGroupProductById.Id);
                            if (List_articleCategory != null && List_articleCategory.Count > 0)
                            {
                                foreach (var item2 in List_articleCategory)
                                {
                                    var detail_article = articleESService.GetDetailById((long)item2.articleid);
                                    if (detail_article != null)
                                    {
                                        var ArticleRelation = new ArticleRelationModel
                                        {
                                            Id = detail_article.Id,
                                            Lead = detail_article.Lead,
                                            Image = detail_article.Image169 ?? detail_article.Image43 ?? detail_article.Image11,
                                            Title = detail_article.Title,
                                            publish_date = detail_article.PublishDate ?? DateTime.Now,
                                            category_name = groupProductName ?? "Tin tức"
                                        };
                                        list_article.Add(ArticleRelation);
                                    }

                                }
                            }

                        }
                        if (list_article.Count > 0)
                            list_article = list_article.Where(s => s.Title.ToUpper().Contains(title.ToUpper())).GroupBy(x => x.Id).Select(x => x.First()).OrderByDescending(x => x.publish_date).ToList();

                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {

                    LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[title = " + title + "]FindArticleByTitle - ArticleDAL: transaction.Commit " + ex);
                    return null;
                }

                return list_article;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[title = " + title + "]FindArticleByTitle - ArticleDAL:" + ex);

                return null;
            }
        }

        /// <summary>
        /// minh.nq
        /// Lấy ra danh sách các bài thuộc 1 chuyên mục, phân trang+ sắp xếp theo ngày mới nhất
        /// </summary>
        /// <param name="cate_id"></param>
        /// <returns></returns>
        public async Task<ArticleFEModelPagnition> getArticleListByCategoryIdOrderByDate(int cate_id, int skip, int take, string category_name)
        {
            try
            {

                var list_postion_pinned = new List<short?> { 1, 2, 3 };

                try
                {
                    var list_article = new List<ArticleFeModel>();
                    var list_article2 = new List<ArticleFeModel>();
                    var list_pinned = new List<ArticleFeModel>();
                    var data = articleCategoryESService.GetByCategoryId(cate_id);
                    if (data != null && data.Count > 0)
                    {
                        data = data.GroupBy(s => s.articleid).Select(s => s.First()).ToList();
                        data = data.Where(s => s.categoryid == cate_id).ToList();
                        foreach (var item in data)
                        {
                            var groupProductName = string.Empty;
                            var groupProductId = string.Empty;
                            var article_Category = articleCategoryESService.GetByArticleId((long)item.articleid);
                            if (article_Category != null)
                            {
                                article_Category = article_Category.GroupBy(s => s.categoryid).Select(s => s.First()).ToList();
                                foreach (var item2 in article_Category)
                                {
                                    var groupProduct = groupProductESService.GetDetailGroupProductById((long)item2.categoryid);
                                    if (groupProduct != null && groupProduct.ParentId > 0)
                                    {
                                        groupProductName += groupProduct.Name + ",";
                                        groupProductId += groupProduct.Id + ",";
                                    }
                                }
                            }

                            var _article = articleESService.GetDetailById((long)item.articleid);
                            if (_article != null && _article.Status == ArticleStatus.PUBLISH)
                            {
                                var model = new ArticleFeModel
                                {
                                    id = _article.Id,
                                    category_name = groupProductName,
                                    title = _article.Title,
                                    lead = _article.Lead,
                                    image_169 = _article.Image169,
                                    image_43 = _article.Image43,
                                    image_11 = _article.Image11,
                                    publish_date = (DateTime)_article.PublishDate,
                                    article_type = _article.ArticleType,
                                    update_last = (DateTime)_article.ModifiedOn,
                                    position = _article.Position,
                                    category_id = groupProductId,
                                };
                                list_article.Add(model);
                            }

                        }
                    }


                    var article = articleESService.GetListArticlePosition();
                    article = article.Where(S => S.Status == ArticleStatus.PUBLISH).ToList();
                    if (article != null && article.Count > 0)
                    {
                        foreach (var _article in article)
                        {
                            var groupProductName = string.Empty;
                            var groupProductId = string.Empty;
                            var article_Category = articleCategoryESService.GetByArticleId(_article.Id);
                            if (article_Category != null)
                            {
                                article_Category = article_Category.GroupBy(s => s.categoryid).Select(s => s.First()).ToList();
                                foreach (var item2 in article_Category)
                                {
                                    var groupProduct = groupProductESService.GetDetailGroupProductById((long)item2.categoryid);
                                    if (groupProduct != null && groupProduct.ParentId > 0)
                                    {
                                        groupProductName += groupProduct.Name + ",";
                                        groupProductId += groupProduct.Id + ",";
                                    }
                                    else
                                    {
                                        groupProductId += groupProduct.Id + ",";
                                    }

                                }
                            }
                            var model = new ArticleFeModel
                            {
                                id = _article.Id,
                                category_name = groupProductName,
                                title = _article.Title,
                                lead = _article.Lead,
                                image_169 = _article.Image169,
                                image_43 = _article.Image43,
                                image_11 = _article.Image11,
                                publish_date = (DateTime)_article.PublishDate,
                                position = _article.Position,
                                article_type = _article.ArticleType,
                                update_last = (DateTime)_article.ModifiedOn,
                                category_id = groupProductId,
                            };

                            if (groupProductId.Contains(cate_id.ToString()))
                                list_pinned.Add(model);

                        }
                    }

                    var result = new ArticleFEModelPagnition();
                    list_article = list_article.OrderByDescending(x => x.publish_date).GroupBy(gr => gr.id).Select(x => x.First()).ToList();
                    list_pinned = list_pinned.OrderByDescending(x => x.update_last).GroupBy(gr => gr.position).Select(x => x.First()).ToList();

                    foreach (var pinned in list_pinned)
                    {

                        if (pinned.position != null && pinned.position > 0)
                        {
                            list_article.RemoveAll(x => x.title == pinned.title && x.lead == pinned.lead);
                            //list_article.Insert(((int)pinned.position - 1), pinned);
                        }
                    }
                    if (list_pinned == null || list_pinned.Count < 3)
                    {

                        foreach (var item in list_article)
                        {
                            list_pinned.Add(item);
                            list_article2.Add(item);
                            if (list_pinned.Count == 3) break;
                        }
                        foreach (var item in list_article2)
                        {

                            list_article.Remove(item);
                        }
                        foreach (var pinned in list_pinned)
                        {
                            if (list_postion_pinned.Contains(pinned.position))
                            {
                                list_postion_pinned.Remove(pinned.position);
                            }
                            else
                            {
                                if (list_postion_pinned.Count > 0)
                                    pinned.position = list_postion_pinned[0];
                                list_postion_pinned.Remove(list_postion_pinned[0]);
                            }
                        }
                    }


                    result.list_article_fe = list_article.Skip(skip).Take(take).ToList();
                    result.list_article_pinned = list_pinned;
                    result.total_item_count = list_article.Count;
                    return result;
                }
                catch (Exception ex)
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[getArticleListByCategoryId - ArticleDAL:" + ex);
                return null;
            }
        }
        /// <summary>
        /// minh.nq
        /// Lấy ra danh sách các bài thuộc 1 chuyên mục, phân trang+ sắp xếp theo ngày mới nhất
        /// </summary>
        /// <param name="cate_id"></param>
        /// <returns></returns>
        public async Task<ArticleFEModelPagnition> GetArticleListByTag(string tag, int skip, int take)
        {
            try
            {


                var list_article = new List<ArticleFeModel>();
                var ListTag = tagESService.GetListTagByTagName(tag);
                if (ListTag != null && ListTag.Count > 0)
                {
                    foreach (var item in ListTag)
                    {
                        var articleTag = articleTagESService.GetListArticleTagByTagid(item.Id);
                        if (articleTag != null && articleTag.Count > 0)
                        {
                            articleTag = articleTag.GroupBy(s => s.ArticleId).Select(s => s.First()).ToList();
                            foreach (var item2 in articleTag)
                            {
                                var _article = articleESService.GetDetailById((long)item2.ArticleId);
                                var model = new ArticleFeModel
                                {
                                    id = _article.Id,
                                    category_name = "",
                                    title = _article.Title,
                                    lead = _article.Lead,
                                    image_169 = _article.Image169,
                                    image_43 = _article.Image43,
                                    image_11 = _article.Image11,
                                    publish_date = (DateTime)_article.PublishDate,
                                    article_type = _article.ArticleType,
                                    position = _article.Position
                                };
                                list_article.Add(model);
                            }
                        }
                    }

                }


                var result = new ArticleFEModelPagnition();
                result.list_article_fe = list_article.Skip(skip).Take(take).ToList();
                result.total_item_count = list_article.Count;
                return result;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[getArticleListByCategoryId - ArticleDAL:" + ex);
                return null;
            }
        }
        public List<long> GetTagIDByArticleID(long articleID)
        {
            try
            {

                var data = articleTagESService.GetListArticleTagByArticleId(articleID);
                var List_TagId = data.Select(s => s.TagId);
                if (List_TagId != null && List_TagId.Count() > 0)
                {
                    var json = JsonConvert.SerializeObject(List_TagId.Distinct().ToList());
                    return JsonConvert.DeserializeObject<List<long>>(json);
                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return null;
        }
        public List<GroupProductESModel> GetByParentId(long parent_id)
        {
            try
            {
                var data = groupProductESService.GetListGroupProductByParentId(parent_id);
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetByParentId-GroupProductDAL" + ex);
            }
            return null;
        }
        public GroupProductESModel GetById(long id)
        {
            try
            {
                var data = groupProductESService.GetDetailGroupProductById(id);
                return data;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetById-GroupProductDAL" + ex);

            }
            return null;
        }
        public async Task<List<string>> GetTagByListID(List<long> tag_id_list)
        {
            try
            {
                var list_name = new List<string>();
                if (tag_id_list != null && tag_id_list.Count > 0)
                {
                    foreach (var item in tag_id_list)
                    {
                        var tag = tagESService.GetListTagById(item);
                        if (tag != null) { list_name.Add(tag.TagName); }
                    }
                }
                return list_name;

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return null;
            }
        }


        public async Task<ArticleFeModel> GetMostViewedArticle(long article_id)
        {
            try
            {
                var detail = await GetArticleDetailLite(article_id);

                if (detail != null)
                {
                    var group = GetById(detail.category_id);
                    if (!group.IsShowHeader) return null;
                    var fe_detail = new ArticleFeModel()
                    {
                        id = detail.id,
                        lead = detail.lead,
                        publish_date = detail.publish_date,
                        title = detail.title,
                        image_11 = detail.image_11,
                        image_43 = detail.image_43,
                        image_169 = detail.image_169,
                        article_type = detail.article_type,
                        category_name = detail.category_name,

                    };
                    return fe_detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[API]ArticleRepository - GetMostViewedArticle: " + ex);
            }
            return null;
        }



        public async Task<ArticleFEModelPagnition> getArticleListByTags(string tag, int skip, int take)
        {
            try
            {
                return await GetArticleListByTag(tag, skip, take);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[API]getArticleListByTags - GetArticleDetail: " + ex);
                return null;
            }
        }
        public async Task<string> GetGroupProductNameAsync(int cateID)
        {
            string group_name = null;
            try
            {
                var dataModel = GetById(cateID);
                if (dataModel == null || dataModel.Name == null) return "";
                group_name = dataModel.Name;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetGroupProductNameAsync -GroupProductRepository : " + ex);
            }
            return group_name;
        }

        public async Task<List<ArticleGroupViewModel>> GetArticleCategoryByParentID(long parent_id)
        {
            try
            {
                var group = GetByParentId(parent_id);
                group = group.Where(x => x.IsShowHeader == true).ToList();
                var list = new List<ArticleGroupViewModel>();
                //list.Add(new ArticleGroupViewModel()
                //{
                //    id = parent_id,
                //    name = "Mới nhất",
                //    order_no = -1,
                //    image_path = "",
                //    url_path = "tin-tuc-" + parent_id
                //});
                list.AddRange(group.Select(x => new ArticleGroupViewModel() { id = x.Id, image_path = x.ImagePath, name = x.Name, order_no = (int)x.OrderNo, url_path = x.Path }).OrderBy(x => x.order_no).ToList());
                return list;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetArticleCategoryByParentID -GroupProductRepository : " + ex);
            }
            return null;
        }
        public async Task<List<ArticleGroupViewModel>> GetFooterCategoryByParentID(long parent_id)
        {
            try
            {
                var group = GetByParentId(parent_id);
                group = group.Where(x => x.IsShowFooter == true).ToList();
                var list = new List<ArticleGroupViewModel>();

                list.AddRange(group.Select(x => new ArticleGroupViewModel() { id = x.Id, image_path = x.ImagePath, name = x.Name, order_no = (int)x.OrderNo, url_path = x.Path }).OrderBy(x => x.order_no).ToList());
                return list;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetFooterCategoryByParentID -GroupProductRepository : " + ex);
            }
            return null;
        }
        public async Task<List<ProductGroupViewModel>> GetProductGroupByParentID(long parent_id, string url_static)
        {
            try
            {
                var group = GetByParentId(parent_id);
                var list = new List<ProductGroupViewModel>();
                list.AddRange(group.Select(x => new ProductGroupViewModel() { id = x.Id, image = url_static + x.ImagePath, name = x.Name, link = CommonHelper.RemoveSpecialCharacters(CommonHelper.RemoveUnicode(x.Name.ToLower())).Replace(" ", "-").Replace("--", "-") }).ToList());
                return list;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetProductGroupByParentID -GroupProductRepository : " + ex);
            }
            return null;
        }
        public async Task<List<string>> GetAllTagByArticleID(long articleID)
        {
            var tag_id_list = GetTagIDByArticleID(articleID);
            return await GetTagByListID(tag_id_list);
        }
        public async Task<List<ArticleRelationModel>> FindArticleByBody(string title, string parent_cate_faq_id)
        {
            try
            {
                var list_article = new List<ArticleRelationModel>();
                var list_articleid = new List<long?>();

                try
                {
                   
                    var arr_cate_child_help_id = parent_cate_faq_id.Split(',').ToList();
                    var ListArticleByBody = articleESService.GetListArticleByBody( title);
                    if (arr_cate_child_help_id.Count() > 0)
                    {
                        foreach (var item in arr_cate_child_help_id)
                        {
                            var groupProductName = string.Empty;
                            var DetailGroupProductById = groupProductESService.GetDetailGroupProductById(Convert.ToInt32(item));
                            if (DetailGroupProductById.IsShowHeader == true)
                            {
                                groupProductName += DetailGroupProductById.Name + ",";
                            }
                            var List_articleCategory = articleCategoryESService.GetByCategoryId(DetailGroupProductById.Id);
                            list_articleid.AddRange(List_articleCategory.Select(s => s.articleid).ToList());
                        }
                        string articleid = string.Join(',', list_articleid);
                        list_article = ListArticleByBody.Where(s => articleid.Contains(s.Id.ToString())).ToList();
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {

                    LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[title = " + title + "]FindArticleByBody - ArticleDAL: transaction.Commit " + ex);
                    return null;
                }

                return list_article;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "[title = " + title + "]FindArticleByBody - ArticleDAL:" + ex);

                return null;
            }
        }

    }
}
