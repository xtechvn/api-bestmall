using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using HuloToys_Service.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class TagDAL : GenericService<Tag>
    {
        private static DbWorker _DbWorker;
        public TagDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<List<long>> MultipleInsertTag(List<string> TagList)
        {
            var ListResult = new List<long>();
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            if (TagList != null && TagList.Count >= 0)
                            {
                                foreach (var item in TagList)
                                {
                                    var tagItemModel = await _DbContext.Tags.FirstOrDefaultAsync(s => s.TagName == item.Trim());
                                    if (tagItemModel == null)
                                    {
                                        var tagModel = new Tag()
                                        {
                                            TagName = item,
                                            CreatedOn = DateTime.Now
                                        };
                                        await _DbContext.Tags.AddAsync(tagModel);
                                        await _DbContext.SaveChangesAsync();
                                        ListResult.Add(tagModel.Id);
                                    }
                                    else
                                    {
                                        ListResult.Add(tagItemModel.Id);
                                    }
                                }
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return null;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return ListResult;
        }

        public async Task<List<string>> GetSuggestionTag(string name)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Tags.Where(s => s.TagName.Trim().ToLower().Contains(name.ToLower())).Select(s => s.TagName).Take(10).ToListAsync();
                }
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<string>> GetTagByListID(List<long> tag_id_list)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Tags.Where(s => tag_id_list.Contains(s.Id)).Select(s=>s.TagName).ToListAsync();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
