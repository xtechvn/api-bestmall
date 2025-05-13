using DAL.Generic;
using DAL.StoreProcedure;
using Elasticsearch.Net;
using Entities.Models;
using HuloToys_Service.Models.Label;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using static Nest.JoinField;

namespace DAL
{
   public class LabelDAL : GenericService<Label>
    {
        private static DbWorker _DbWorker;
        public LabelDAL(string connection) : base(connection) {
            _DbWorker = new DbWorker(connection);
        }
        //public async Task< List<Label>> Listing()
        //{
        //    try
        //    {
        //        var _DbContext = new EntityDataContext(_connection);
        //        var list = await _DbContext.Labels.AsNoTracking().Where(x => x.Status == 0).ToListAsync();
        //        return list;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("Listing - LabelDAL: " + ex);
        //        return null;
        //    }
        //}
        public async Task<List<LabelListingModel>> Listing(int status=-1, string label_name=null, int page_index = -1, int page_size = 100)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@Status", status<-1?-1:status);
                objParam[1] = new SqlParameter("@LabelName", label_name==null?DBNull.Value: label_name);
                objParam[2] = new SqlParameter("@PageIndex", page_index<0?-1:page_index);
                objParam[3] = new SqlParameter("@PageSize", page_size);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.GetListLabels, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<LabelListingModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Listing - LabelDAL: " + ex);
            }
            return null;
        }
        public int Insert(Label model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[11];
                objParam[0] = new SqlParameter("@LabelName ", model.LabelName);
                objParam[1] = new SqlParameter("@LabelCode", model.LabelCode);
                objParam[2] = new SqlParameter("@SupplierId", model.SupplierId);
                objParam[3] = new SqlParameter("@Icon", model.Icon);
                objParam[4] = new SqlParameter("@ParentId", model.ParentId);
                objParam[5] = new SqlParameter("@Level", model.Level);
                objParam[6] = new SqlParameter("@Description", model.Description);
                objParam[7] = new SqlParameter("@Status", model.Status);
                objParam[8] = new SqlParameter("@CreateTime", model.CreateTime);
                objParam[9] = new SqlParameter("@UpdateTime", model.UpdateTime);
                objParam[10] = new SqlParameter("@CreatedBy", model.CreatedBy);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.InsertLabel, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - LabelDAL: " + ex);
                return 0;
            }
        }
        public int Update(Label model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[12];
                objParam[0] = new SqlParameter("@LabelName ", model.LabelName);
                objParam[1] = new SqlParameter("@LabelCode", model.LabelCode);
                objParam[2] = new SqlParameter("@SupplierId", model.SupplierId);
                objParam[3] = new SqlParameter("@Icon", model.Icon);
                objParam[4] = new SqlParameter("@ParentId", model.ParentId);
                objParam[5] = new SqlParameter("@Level", model.Level);
                objParam[6] = new SqlParameter("@Description", model.Description);
                objParam[7] = new SqlParameter("@Status", model.Status);
                objParam[8] = new SqlParameter("@CreateTime", model.CreateTime);
                objParam[9] = new SqlParameter("@UpdateTime", model.UpdateTime);
                objParam[10] = new SqlParameter("@UpdatedBy", model.UpdatedBy);
                objParam[11] = new SqlParameter("@Id ", model.Id);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.UpdateLabel, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - LabelDAL: " + ex);
                return 0;
            }
        }
    }
}
