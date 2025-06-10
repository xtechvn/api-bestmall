using DAL.Generic;
using DAL.StoreProcedure;
using Microsoft.Data.SqlClient;
using Utilities.Contants;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;

namespace HuloToys_Service.Controllers.SQL
{
    public class AddressClientDAL : GenericService<AddressClient>
    {
        private static DbWorker _DbWorker;
        public AddressClientDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public int Insert(AddressClient addressClient)
        {
            try
            {
                SqlParameter clientIdParam = new SqlParameter("@ClientId", addressClient.ClientId);
                SqlParameter receiverNameParam = new SqlParameter("@ReceiverName", addressClient.ReceiverName ?? (object)DBNull.Value); // Xử lý null
                SqlParameter phoneParam = new SqlParameter("@Phone", addressClient.Phone);
                SqlParameter provinceIdParam = new SqlParameter("@ProvinceId", addressClient.ProvinceId ?? (object)DBNull.Value); // Xử lý null
                SqlParameter districtIdParam = new SqlParameter("@DistrictId", addressClient.DistrictId ?? (object)DBNull.Value); // Xử lý null
                SqlParameter wardIdParam = new SqlParameter("@WardId", addressClient.WardId ?? (object)DBNull.Value); // Xử lý null
                SqlParameter addressParam = new SqlParameter("@Address", addressClient.Address ?? (object)DBNull.Value); // Xử lý null
                SqlParameter statusParam = new SqlParameter("@Status", addressClient.Status ?? (object)DBNull.Value);   // Xử lý null
                SqlParameter isActiveParam = new SqlParameter("@IsActive", addressClient.IsActive);
                SqlParameter[] parameters = new SqlParameter[]
              {
                    clientIdParam, receiverNameParam, phoneParam, provinceIdParam, districtIdParam, wardIdParam, addressParam, statusParam, isActiveParam
              };

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertAddressClient, parameters);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateAccountClient - AccountClientDAL: " + ex);
                return 0;
            }
        }
    }
}