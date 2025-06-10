using DAL.Generic;
using Entities.Models;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class GroupProductDAL : GenericService<GroupProduct>
    {
        public GroupProductDAL(string connection) : base(connection)
        {
        }

        public List<GroupProduct> GetByParentId(long parent_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.GroupProducts.Where(s => s.ParentId == parent_id && s.Status==(int)ArticleStatus.PUBLISH).ToList();
                }
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByParentId - GroupProductDAL: " + ex);

            }
            return null;
        }
        public GroupProduct GetById(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.GroupProducts.Where(s => s.Id == id && s.Status == (int)ArticleStatus.PUBLISH).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - GroupProductDAL: " + ex);

            }
            return null;
        }
    }
}
