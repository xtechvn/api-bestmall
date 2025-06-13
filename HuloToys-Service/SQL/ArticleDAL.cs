using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using Utilities;
using Utilities.Contants;
using Microsoft.Data.SqlClient;

namespace HuloToys_Service.Controllers.SQL
{
    public class ArticleDAL : GenericService<Article>
    {
        private static DbWorker _DbWorker;
        public ArticleDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public DataTable GetPagingList(ArticleSearchModel searchModel, int currentPage, int pageSize)
        {
            try
            {
                DateTime _FromDate = DateTime.MinValue;
                DateTime _ToDate = DateTime.MinValue;
                string _ArrCategoryId = string.Empty;

                if (!string.IsNullOrEmpty(searchModel.FromDate))
                {
                    _FromDate = DateTime.ParseExact(searchModel.FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(searchModel.ToDate))
                {
                    _ToDate = DateTime.ParseExact(searchModel.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (searchModel.ArrCategoryId != null && searchModel.ArrCategoryId.Length > 0)
                {
                    _ArrCategoryId = string.Join(",", searchModel.ArrCategoryId);
                }

                SqlParameter[] objParam = new SqlParameter[10];
                objParam[0] = new SqlParameter("@Title", searchModel.Title ?? string.Empty);
                objParam[1] = new SqlParameter("@ArticleId", searchModel.ArticleId);

                if (_FromDate != DateTime.MinValue)
                    objParam[2] = new SqlParameter("@FromDate", _FromDate);
                else
                    objParam[2] = new SqlParameter("@FromDate", DBNull.Value);

                if (_ToDate != DateTime.MinValue)
                    objParam[3] = new SqlParameter("@ToDate", _ToDate);
                else
                    objParam[3] = new SqlParameter("@ToDate", DBNull.Value);

                objParam[4] = new SqlParameter("@AuthorId", searchModel.AuthorId);
                objParam[5] = new SqlParameter("@Status", searchModel.ArticleStatus);
                objParam[6] = new SqlParameter("@ArrCategoryId", _ArrCategoryId);
                objParam[7] = new SqlParameter("@SearchType", searchModel.SearchType);
                objParam[8] = new SqlParameter("@CurentPage", currentPage);
                objParam[9] = new SqlParameter("@PageSize", pageSize);

                return _DbWorker.GetDataTable(ProcedureConstants.ARTICLE_SEARCH, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ArticleDAL: " + ex);
            }
            return null;
        }

        public async Task<long> SaveArticle(ArticleModel model)
        {
            try
            {
                long articleId = model.Id;

                if (model.Id > 0)
                {
                    var entity = await FindAsync(model.Id);

                    entity.Title = model.Title;
                    entity.Lead = model.Lead;
                    entity.Body = model.Body;
                    entity.Image11 = model.Image11 ?? string.Empty;
                    entity.Image169 = model.Image169 ?? string.Empty;
                    entity.Image43 = model.Image43 ?? string.Empty;
                    entity.Status = model.Status;
                    //entity.ArticleType = model.ArticleType;
                    entity.ModifiedOn = DateTime.Now;
                    entity.PublishDate = model.PublishDate == DateTime.MinValue ? (DateTime?)null : model.PublishDate;
                    entity.UpTime = model.PublishDate == DateTime.MinValue ? (DateTime?)null : model.PublishDate;
                    entity.DownTime = model.DownTime == DateTime.MinValue ? (DateTime?)null : model.DownTime;
                    entity.Position = (short?)model.Position;
                    await UpdateAsync(entity);
                }
                else
                {
                    var entity = new Article
                    {
                        Id = model.Id,
                        Title = model.Title,
                        Lead = model.Lead,
                        Body = model.Body,
                        Status = model.Status,
                        Image11 = model.Image11 ?? string.Empty,
                        Image169 = model.Image169 ?? string.Empty,
                        Image43 = model.Image43 ?? string.Empty,
                        //ArticleType = model.ArticleType,
                        AuthorId = model.AuthorId,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now,
                        PublishDate = model.PublishDate == DateTime.MinValue ? (DateTime?)null : model.PublishDate,
                        UpTime = model.PublishDate == DateTime.MinValue ? (DateTime?)null : model.PublishDate,
                        DownTime = model.DownTime == DateTime.MinValue ? (DateTime?)null : model.DownTime,
                        Position = (short?)model.Position
                    };
                    articleId = await CreateAsync(entity);
                }
                return articleId;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<ArticleModel> GetArticleDetail(long Id)
        {
            try
            {
                var model = new ArticleModel();
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var article = await _DbContext.Articles.FindAsync(Id);
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

                            var TagIds = await _DbContext.ArticleTags.Where(s => s.ArticleId == article.Id).Select(s => s.TagId).ToListAsync();
                            model.Tags = await _DbContext.Tags.Where(s => TagIds.Contains(s.Id)).Select(s => s.TagName).ToListAsync();
                            model.Categories = await _DbContext.ArticleCategories.Where(s => s.ArticleId == article.Id).Select(s => (int)s.CategoryId).ToListAsync();
                            model.MainCategory = model.Categories != null || model.Categories.Count > 0 ? model.Categories[0] : -1;
                            model.RelatedArticleIds = await _DbContext.ArticleRelateds.Where(s => s.ArticleId == article.Id).Select(s => (long)s.ArticleRelatedId).ToListAsync();

                            if (model.RelatedArticleIds != null && model.RelatedArticleIds.Count > 0)
                            {
                                model.RelatedArticleList = await (from _article in _DbContext.Articles.AsNoTracking()
                                                                  join a in _DbContext.Users.AsNoTracking() on _article.AuthorId equals a.Id
                                                                  join b in _DbContext.ArticleCategories.AsNoTracking() on _article.Id equals b.ArticleId
                                                                  join c in _DbContext.GroupProducts.AsNoTracking() on b.CategoryId equals c.Id
                                                                  where model.RelatedArticleIds.Contains(_article.Id)
                                                                  select new ArticleRelationModel
                                                                  {
                                                                      Id = _article.Id,
                                                                      Image = _article.Image169 ?? _article.Image43 ?? _article.Image11,
                                                                      Title = _article.Title,
                                                                      publish_date = _article.PublishDate ?? DateTime.Now,
                                                                      category_name = c.Name ?? "Tin tức"
                                                                  }).ToListAsync();
                                model.RelatedArticleList = model.RelatedArticleList.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
                return model;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> MultipleInsertArticleTag(long ArticleId, List<long> ListTagId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var ExistList = await _DbContext.ArticleTags.Where(s => s.ArticleId == ArticleId).ToListAsync();
                            if (ExistList != null && ExistList.Count > 0)
                            {
                                foreach (var item in ExistList)
                                {
                                    var deleteModel = await _DbContext.ArticleTags.FindAsync(item.Id);
                                    _DbContext.ArticleTags.Remove(deleteModel);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            if (ListTagId != null && ListTagId.Count > 0)
                            {
                                foreach (var item in ListTagId)
                                {
                                    var model = new ArticleTag
                                    {
                                        TagId = item,
                                        ArticleId = ArticleId
                                    };
                                    await _DbContext.ArticleTags.AddAsync(model);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return 0;
                        }
                    }
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> MultipleInsertArticleCategory(long ArticleId, List<int> ListCateId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var ExistList = await _DbContext.ArticleCategories.Where(s => s.ArticleId == ArticleId).ToListAsync();
                            if (ExistList != null && ExistList.Count > 0)
                            {
                                foreach (var item in ExistList)
                                {
                                    var deleteModel = await _DbContext.ArticleCategories.FindAsync(item.Id);
                                    _DbContext.ArticleCategories.Remove(deleteModel);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            if (ListCateId != null && ListCateId.Count > 0)
                            {
                                foreach (var item in ListCateId)
                                {
                                    var model = new ArticleCategory
                                    {
                                        CategoryId = item,
                                        ArticleId = ArticleId
                                    };
                                    await _DbContext.ArticleCategories.AddAsync(model);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return 0;
                        }
                    }
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> MultipleInsertArticleRelation(long ArticleId, List<long> ListArticleId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var ExistList = await _DbContext.ArticleRelateds.Where(s => s.ArticleId == ArticleId).ToListAsync();
                            if (ExistList != null && ExistList.Count > 0)
                            {
                                foreach (var item in ExistList)
                                {
                                    var deleteModel = await _DbContext.ArticleRelateds.FindAsync(item.Id);
                                    _DbContext.ArticleRelateds.Remove(deleteModel);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            if (ListArticleId != null && ListArticleId.Count > 0)
                            {
                                foreach (var item in ListArticleId)
                                {
                                    var model = new ArticleRelated
                                    {
                                        ArticleRelatedId = item,
                                        ArticleId = ArticleId
                                    };
                                    await _DbContext.ArticleRelateds.AddAsync(model);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return 0;
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("MultipleInsertArticleRelation - ArticleDAL: " + ex);
                return 0;
            }
        }

        public async Task<long> DeleteArticle(long Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var ExistCategory = await _DbContext.ArticleCategories.Where(s => s.ArticleId == Id).ToListAsync();
                            if (ExistCategory != null && ExistCategory.Count > 0)
                            {
                                foreach (var item in ExistCategory)
                                {
                                    var deleteModel = await _DbContext.ArticleCategories.FindAsync(item.Id);
                                    _DbContext.ArticleCategories.Remove(deleteModel);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            var ExistRelated = await _DbContext.ArticleRelateds.Where(s => s.ArticleId == Id).ToListAsync();
                            if (ExistRelated != null && ExistRelated.Count > 0)
                            {
                                foreach (var item in ExistRelated)
                                {
                                    var deleteModel = await _DbContext.ArticleRelateds.FindAsync(item.Id);
                                    _DbContext.ArticleRelateds.Remove(deleteModel);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            var ExistTag = await _DbContext.ArticleTags.Where(s => s.ArticleId == Id).ToListAsync();
                            if (ExistTag != null && ExistTag.Count > 0)
                            {
                                foreach (var item in ExistTag)
                                {
                                    var deleteModel = await _DbContext.ArticleTags.FindAsync(item.Id);
                                    _DbContext.ArticleTags.Remove(deleteModel);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            var article = await _DbContext.Articles.FindAsync(Id);
                            _DbContext.Articles.Remove(article);
                            await _DbContext.SaveChangesAsync();

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return -1;
                        }
                    }
                }
                return Id;
            }
            catch
            {
                return -1;
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
                var model = new ArticleModel();
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {


                            var list_article = await (from _article in _DbContext.Articles.AsNoTracking()
                                                      join a in _DbContext.ArticleCategories.AsNoTracking() on _article.Id equals a.ArticleId into af
                                                      from detail in af.DefaultIfEmpty()
                                                      where detail.CategoryId == (cate_id == -1 ? detail.CategoryId : cate_id) && _article.Status == ArticleStatus.PUBLISH
                                                      orderby _article.PublishDate descending
                                                      select new ArticleFeModel
                                                      {
                                                          id = _article.Id,
                                                          title = _article.Title,
                                                          lead = _article.Lead,
                                                          image_169 = _article.Image169,
                                                          image_43 = _article.Image43,
                                                          image_11 = _article.Image11,
                                                          publish_date = (DateTime)_article.PublishDate,
                                                          body=_article.Body
                                                      }
                                                     ).ToListAsync();

                            transaction.Commit();
                            list_article = list_article.Where(x => x.body != null && x.body.Trim() != "" && x.lead != null && x.lead.Trim() != "" && x.title != null && x.title.Trim() != "").ToList();

                            return list_article;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getArticleListByCategoryId - ArticleDAL: " + ex);
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
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var article = await _DbContext.Articles.FirstOrDefaultAsync(x => x.Status == ArticleStatus.PUBLISH && x.Id == article_id);
                    if (article == null) return null;
                    model = new ArticleFeDetailModel
                    {
                        id = article.Id,
                        title = article.Title,
                        lead = article.Lead,
                        body = article.Body,
                        status = article.Status,
                        article_type = article.ArticleType,
                        image_11 = article.Image11,
                        image_43 = article.Image43,
                        image_169 = article.Image169,
                        publish_date = article.PublishDate ?? DateTime.Now,
                        author_id = (int)article.AuthorId
                    };
                   
                    model.Categories = await _DbContext.ArticleCategories.Where(s => s.ArticleId == article.Id).Select(s => (int)s.CategoryId).ToListAsync();
                    model.category_id = model.Categories != null || model.Categories.Count > 0 ? model.Categories[0] : -1;
                    model.MainCategory = model.Categories != null || model.Categories.Count > 0 ? model.Categories[0] : -1;
                    var group_product = await _DbContext.GroupProducts.Where(x => x.Status == 0 && x.Id == model.MainCategory).FirstOrDefaultAsync();
                    if (group_product == null || group_product.Id < 0) return null;
                    var author = await _DbContext.Users.FirstOrDefaultAsync(x => x.Id == (int)article.AuthorId);
                    if (author != null)
                    {
                        model.AuthorName = author.FullName == null ? author.UserName : author.FullName;
                    }
                    if (model.MainCategory > 0)
                    {
                        model.category_name = await _DbContext.GroupProducts.Where(s => s.Id == model.MainCategory).Select(x => x.Name).FirstOrDefaultAsync();
                    }
                    else
                    {
                        model.category_name = "Tin tức";
                    }
                    model.RelatedArticleIds = await _DbContext.ArticleRelateds.Where(s => s.ArticleId == article.Id).Select(s => (long)s.ArticleRelatedId).ToListAsync();

                    if (model.RelatedArticleIds != null && model.RelatedArticleIds.Count > 0)
                    {
                        model.RelatedArticleList = await (from _article in _DbContext.Articles.AsNoTracking()
                                                          join a in _DbContext.Users.AsNoTracking() on _article.AuthorId equals a.Id
                                                          join b in _DbContext.ArticleCategories.AsNoTracking() on _article.Id equals b.ArticleId
                                                          join c in _DbContext.GroupProducts.AsNoTracking() on b.CategoryId equals c.Id
                                                          where model.RelatedArticleIds.Contains(_article.Id)
                                                          select new ArticleRelationModel
                                                          {
                                                              Id = _article.Id,
                                                              Image = _article.Image169 ?? _article.Image43 ?? _article.Image11,
                                                              Title = _article.Title,
                                                              publish_date = _article.PublishDate ?? DateTime.Now,
                                                              category_name = c.Name ?? "Tin tức"
                                                          }).ToListAsync();
                        model.RelatedArticleList = model.RelatedArticleList.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[article_id = " + article_id + "]GetArticleDetailLite - ArticleDAL: " + ex);
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
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var arr_cate_child_help_id = _DbContext.GroupProducts.Where(n => n.Id == parent_cate_faq_id || n.ParentId == parent_cate_faq_id).Select(x => x.Id).ToArray();

                            if (arr_cate_child_help_id.Count() > 0)
                            {
                                list_article = await (from _article in _DbContext.Articles.AsNoTracking()
                                                      join a in _DbContext.ArticleCategories on _article.Id equals a.ArticleId
                                                      join c in _DbContext.GroupProducts.AsNoTracking() on a.CategoryId equals c.Id
                                                      where arr_cate_child_help_id.Contains(a.CategoryId ?? -1) && _article.Status == ArticleStatus.PUBLISH && _article.Title.ToUpper().Contains(title.ToUpper())
                                                      select new ArticleRelationModel
                                                      {
                                                          Id = _article.Id,
                                                          Image = _article.Image169 ?? _article.Image43 ?? _article.Image11,
                                                          Title = _article.Title,
                                                          publish_date = _article.PublishDate ?? DateTime.Now,
                                                          category_name = c.Name ?? "Tin tức"
                                                      }).ToListAsync();
                                if (list_article.Count > 0)
                                    list_article = list_article.GroupBy(x => x.Id).Select(x => x.First()).OrderByDescending(x => x.publish_date).ToList();

                            }
                            else
                            {
                                return null;
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogHelper.InsertLogTelegram("[title = " + title + "] FindArticleByTitle - ArticleDAL: transaction.Commit " + ex);
                            return null;
                        }
                    }
                }
                return list_article;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[title = " + title + "] FindArticleByTitle - ArticleDAL: " + ex);
                return null;
            }
        }
        public async Task<List<int>> GetArticleCategoryIdList(long ArticleId)
        {
            var ListRs = new List<int>();
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    ListRs = await _DbContext.ArticleCategories.Where(s => s.ArticleId == ArticleId).Select(s => (int)s.CategoryId).ToListAsync();
                }
            }
            catch
            {

            }
            return ListRs;
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
                var model = new ArticleModel();
                var list_postion_pinned = new List<short?> { 1, 2, 3, 4, 5, 6, 7 };
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {

                            var list_article = await (from _article in _DbContext.Articles.AsNoTracking()
                                                      join a in _DbContext.ArticleCategories.AsNoTracking() on _article.Id equals a.ArticleId into af
                                                      from detail in af.DefaultIfEmpty()
                                                      where detail.CategoryId == cate_id && _article.Status == ArticleStatus.PUBLISH && _article.PublishDate <= DateTime.Now
                                                      orderby _article.PublishDate descending
                                                      select new ArticleFeModel
                                                      {
                                                          id = _article.Id,
                                                          category_name = category_name,
                                                          title = _article.Title,
                                                          lead = _article.Lead,
                                                          image_169 = _article.Image169,
                                                          image_43 = _article.Image43,
                                                          image_11 = _article.Image11,
                                                          publish_date = (DateTime)_article.PublishDate,
                                                          article_type = _article.ArticleType,
                                                          update_last = (DateTime)_article.ModifiedOn
                                                      }
                                                     ).ToListAsync();
                            var list_pinned = await (from a in _DbContext.ArticleCategories.AsNoTracking()
                                                     join _article in _DbContext.Articles.AsNoTracking() on a.ArticleId equals _article.Id
                                                     where _article.Status == ArticleStatus.PUBLISH && _article.PublishDate <= DateTime.Now /*&& _article.DownTime > DateTime.Now*/ && _article.Position != null && _article.Position > 0
                                                     orderby _article.Position ascending
                                                     select new ArticleFeModel
                                                     {
                                                         id = _article.Id,
                                                         category_name = category_name,
                                                         title = _article.Title,
                                                         lead = _article.Lead,
                                                         image_169 = _article.Image169,
                                                         image_43 = _article.Image43,
                                                         image_11 = _article.Image11,
                                                         publish_date = (DateTime)_article.PublishDate,
                                                         position = _article.Position,
                                                         article_type = _article.ArticleType,
                                                         update_last = (DateTime)_article.ModifiedOn

                                                     }).ToListAsync();

                            transaction.Commit();
                            var result = new ArticleFEModelPagnition();
                            list_article = list_article.OrderByDescending(x => x.publish_date).ToList();
                            list_pinned = list_pinned.OrderByDescending(x => x.update_last).GroupBy(gr => gr.position).Select(x => x.First()).ToList();

                            foreach (var pinned in list_pinned)
                            {
                                if (pinned.position != null && pinned.position > 0)
                                {
                                    list_article.RemoveAll(x => x.title == pinned.title && x.lead == pinned.lead);
                                    //list_article.Insert(((int)pinned.position - 1), pinned);
                                }
                            }

                            result.list_article_fe = list_article.Skip(skip).Take(take).ToList();
                            result.list_article_pinned = list_pinned;
                            result.total_item_count = list_article.Count;
                            return result;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getArticleListByCategoryId - ArticleDAL: " + ex);
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
                var model = new ArticleModel();
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var list_article = await (from _article in _DbContext.Articles.AsNoTracking()
                                              join b in _DbContext.ArticleTags.AsNoTracking() on _article.Id equals b.ArticleId
                                              join c in _DbContext.Tags.AsNoTracking() on b.TagId equals c.Id
                                              where c.TagName.ToLower().Replace("#","") == tag.ToLower().Replace("#", "") && _article.Status == ArticleStatus.PUBLISH
                                              orderby _article.PublishDate descending
                                              select new ArticleFeModel
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
                                              }).ToListAsync();


                    var result = new ArticleFEModelPagnition();
                    result.list_article_fe = list_article.Skip(skip).Take(take).ToList();
                    result.total_item_count = list_article.Count;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getArticleListByCategoryId - ArticleDAL: " + ex);
                return null;
            }
        }
        /// <summary>
        /// Minh: Lấy ra bài viết được pinn trong time
        /// </summary>
        /// <param name="cate_id"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="category_name"></param>
        /// <returns></returns>
        public async Task<ArticleFeModel> getPinnedArticleByposition(int cate_id, string category_name, int position)
        {
            try
            {
                var model = new ArticleModel();
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {

                            var article = await (from _article in _DbContext.Articles.AsNoTracking()
                                                 join a in _DbContext.ArticleCategories.AsNoTracking() on _article.Id equals a.ArticleId into af
                                                 from detail in af.DefaultIfEmpty()
                                                 where detail.CategoryId == cate_id && _article.Status == ArticleStatus.PUBLISH && _article.UpTime <= DateTime.Now && _article.Position == position
                                                 select new ArticleFeModel
                                                 {
                                                     category_name = category_name,
                                                     title = _article.Title,
                                                     lead = _article.Lead,
                                                     image_169 = _article.Image169,
                                                     image_43 = _article.Image43,
                                                     image_11 = _article.Image11,
                                                     publish_date = (DateTime)_article.PublishDate,
                                                 }
                                                     ).FirstOrDefaultAsync();


                            transaction.Commit();
                            return article;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getArticleListByCategoryId - ArticleDAL: " + ex);
                return null;
            }
        }
        /// <summary>
        /// minh.nq
        /// Lấy ra danh sách các bài thuộc 1 chuyên mục footer, phân trang+ sắp xếp theo ngày mới nhất
        /// </summary>
        /// <param name="cate_id"></param>
        /// <returns></returns>
        public async Task<ArticleFEModelPagnition> getFooterArticleListByCategory(int cate_id, int skip, int take, string category_name)
        {
            try
            {
                var model = new ArticleModel();
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        var group = await _DbContext.GroupProducts.Where(x => x.Id == cate_id && x.IsShowFooter == true).FirstOrDefaultAsync();
                        if(group==null || group.Id <= 0)
                        {
                            var result = new ArticleFEModelPagnition();
                            result.list_article_fe = new List<ArticleFeModel>();
                            result.total_item_count = 0;
                            return result;
                        }
                        try
                        {

                            var list_article = await (from _article in _DbContext.Articles.AsNoTracking()
                                                      join a in _DbContext.ArticleCategories.AsNoTracking() on _article.Id equals a.ArticleId into af
                                                      from detail in af.DefaultIfEmpty()
                                                      where detail.CategoryId == cate_id && _article.Status == ArticleStatus.PUBLISH
                                                      orderby _article.PublishDate descending
                                                      select new ArticleFeModel
                                                      {
                                                          id = _article.Id,
                                                          category_name = category_name,
                                                          title = _article.Title,
                                                          lead = _article.Lead,
                                                          image_169 = _article.Image169,
                                                          image_43 = _article.Image43,
                                                          image_11 = _article.Image11,
                                                          publish_date = (DateTime)_article.PublishDate,
                                                          article_type = _article.ArticleType,
                                                          update_last = (DateTime)_article.ModifiedOn
                                                      }
                                                     ).ToListAsync();

                            transaction.Commit();
                            var result = new ArticleFEModelPagnition();
                            list_article = list_article.OrderByDescending(x => x.publish_date).ToList();
                            result.list_article_fe = list_article.Skip(skip).Take(take).ToList();
                            result.total_item_count = list_article.Count;
                            return result;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getArticleListByCategoryId - ArticleDAL: " + ex);
                return null;
            }
        }
    }
}
