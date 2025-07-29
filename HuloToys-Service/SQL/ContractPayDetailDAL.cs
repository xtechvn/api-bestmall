using DAL;
using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.EntityFrameworkCore;

namespace HuloToys_Service.SQL
{
    public class ContractPayDetailDAL : GenericService<ContractPayDetail>
    {
        private static DbWorker _DbWorker;
        private static OrderDAL orderDAL;
        public ContractPayDetailDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
            orderDAL = new OrderDAL(connection);
        }
        public ContractPayDetail GetByServiceCode(string service_code)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ContractPayDetails.AsNoTracking().FirstOrDefault(x => x.ServiceCode == service_code);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ContractPayDAL: " + ex);
                return null;
            }
        }
    }
}
