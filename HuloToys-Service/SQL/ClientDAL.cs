using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class ClientDAL : GenericService<Client>
    {
        private static DbWorker _DbWorker;
        public ClientDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<AccountClient> GetAccountClientDetai(long clientId)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AccountClients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == clientId);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientDetai - ClientDAL: " + ex.ToString());
                return null;
            }
        }
        public async Task<AccountClient> GetAccountClientIdDetai(long clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AccountClients.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientIdDetai - ClientDAL: " + ex.ToString());
                return null;
            }
        }

        public async Task<AccountClient> GetAccountClientByID(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AccountClients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientByID - ClientDAL: " + ex.ToString());
                return null;
            }
        }
        public async Task<List<AccountClient>> GetAccountClientAsync()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.AccountClients.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientAsync - ClientDAL: " + ex.ToString());
                return new List<AccountClient>();
            }
        }

        public async Task<Client> GetClientDetail(long clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == clientId);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientDetail - ClientDAL: " + ex.ToString());
                return null;
            }
        }

        public List<Client> GetClientByIds(List<long> clientIds)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var clients = _DbContext.Clients.AsNoTracking().Where(x => clientIds.Contains(x.Id)).ToList();
                    if (clients != null)
                    {
                        return clients;
                    }
                }
                return new List<Client>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByIds - ClientDAL: " + ex.ToString());
                return new List<Client>();
            }
        }

        public async Task<List<Client>> GetClientType(int Type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var clients = _DbContext.Clients.AsNoTracking().Where(x => x.ClientType == Type).ToList();
                    if (clients != null)
                    {
                        return clients;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientType - ClientDAL: " + ex.ToString());
                return null;
            }
        }
        public List<Client> GetAllClient()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var clients = _DbContext.Clients.AsNoTracking().OrderByDescending(s => s.JoinDate).ToList();
                    if (clients != null)
                    {
                        return clients;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllClient - ClientDAL: " + ex.ToString());
                return null;
            }
        }

        public async Task<List<Client>> GetClientInfo(List<long> listIdAccountClient)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var listAccountClient = _DbContext.AccountClients.AsNoTracking().Where(n => listIdAccountClient.Contains(n.Id)).ToList();
                    var listClientId = listAccountClient.Select(n => n.ClientId).ToList();
                    var listClient = _DbContext.Clients.AsNoTracking().Where(n => listClientId.Contains(n.Id)).ToList();
                    foreach (var item in listClient)
                    {
                        var accountClient = listAccountClient.FirstOrDefault(n => n.ClientId == item.Id);
                        item.ClientMapId = accountClient?.Id;
                    }
                    return listClient;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientInfo - ClientDAL: " + ex);
                return new List<Client>();
            }
        }
  
        public int SetUpClient(Client model)
        {
            try
            {
                
                if (model.Id == 0)
                {
                    var check = GetClientByClientCode(model.ClientCode);
                    if (check == null )
                    {
                        return 2;
                    }
                    else
                    {

                        SqlParameter[] objParam = new SqlParameter[23];
                        objParam[0] = new SqlParameter("@ClientMapId", model.ClientMapId);
                        objParam[1] = new SqlParameter("@SaleMapId", model.SaleMapId);
                        objParam[2] = new SqlParameter("@ClientType", model.ClientType);
                        objParam[3] = new SqlParameter("@ClientName", model.ClientName);
                        objParam[4] = new SqlParameter("@Email", model.Email);
                        objParam[5] = new SqlParameter("@Gender", model.Gender);
                        objParam[6] = new SqlParameter("@Status", model.Status);
                        objParam[7] = new SqlParameter("@Note", model.Note);
                        objParam[8] = new SqlParameter("@Avartar", model.Avartar);
                        objParam[9] = new SqlParameter("@JoinDate", model.JoinDate);
                        objParam[10] = new SqlParameter("@isReceiverInfoEmail  ", model.IsReceiverInfoEmail);
                        objParam[11] = new SqlParameter("@Phone", model.Phone);
                        objParam[12] = new SqlParameter("@Birthday", model.Birthday);
                        objParam[13] = new SqlParameter("@UpdateTime", model.UpdateTime);
                        objParam[14] = new SqlParameter("@TaxNo", model.TaxNo);
                        objParam[15] = new SqlParameter("@AgencyType", model.AgencyType);
                        objParam[16] = new SqlParameter("@PermisionType", model.PermisionType);
                        objParam[17] = new SqlParameter("@BusinessAddress", model.BusinessAddress);
                        objParam[18] = new SqlParameter("@ExportBillAddress", model.ExportBillAddress);
                        objParam[19] = new SqlParameter("@ClientCode", model.ClientCode);
                        objParam[20] = new SqlParameter("@IsRegisterAffiliate", model.IsRegisterAffiliate);
                        objParam[21] = new SqlParameter("@ReferralId", model.ReferralId);
                        objParam[22] = new SqlParameter("@ParentId", model.ParentId);


                        var dt = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertClient, objParam);


                        return 1;
                    }

                }
                else
                {
                    var data2 = GetClientByEmail(model.Email);


                    if (data2 != null && data2.Id == model.Id )
                    {

                        SqlParameter[] objParam = new SqlParameter[24];
                        objParam[0] = new SqlParameter("@Id ", model.Id);
                        objParam[1] = new SqlParameter("@ClientMapId", model.ClientMapId);
                        objParam[2] = new SqlParameter("@SaleMapId", model.SaleMapId);
                        objParam[3] = new SqlParameter("@ClientType", model.ClientType);
                        objParam[4] = new SqlParameter("@ClientName", model.ClientName);
                        objParam[5] = new SqlParameter("@Email", model.Email);
                        objParam[6] = new SqlParameter("@Gender", model.Gender);
                        objParam[7] = new SqlParameter("@Status", model.Status);
                        objParam[8] = new SqlParameter("@Note", model.Note);
                        objParam[9] = new SqlParameter("@Avartar", model.Avartar);
                        objParam[10] = new SqlParameter("@JoinDate", model.JoinDate);
                        objParam[11] = new SqlParameter("@isReceiverInfoEmail  ", model.IsReceiverInfoEmail);
                        objParam[12] = new SqlParameter("@Phone", model.Phone);
                        objParam[13] = new SqlParameter("@Birthday", model.Birthday);
                        objParam[14] = new SqlParameter("@UpdateTime", model.UpdateTime);
                        objParam[15] = new SqlParameter("@TaxNo", model.TaxNo);
                        objParam[16] = new SqlParameter("@AgencyType", model.AgencyType);
                        objParam[17] = new SqlParameter("@PermisionType", model.PermisionType);
                        objParam[18] = new SqlParameter("@BusinessAddress", model.BusinessAddress);
                        objParam[19] = new SqlParameter("@ExportBillAddress", model.ExportBillAddress);
                        objParam[20] = new SqlParameter("@ClientCode", model.ClientCode);
                        objParam[21] = new SqlParameter("@IsRegisterAffiliate", model.IsRegisterAffiliate);
                        objParam[22] = new SqlParameter("@ReferralId", model.ReferralId);
                        objParam[23] = new SqlParameter("@ParentId", model.ParentId);


                        var dt = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateClient, objParam);

                    }
                    else
                    {
                        return 2;
                    }

                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpClient - ClientDAL: " + ex.ToString());
                return 0;
            }
        }
        public Client GetClientByEmail(string email)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@Email", email);
                objParam[1] = new SqlParameter("@TaxNo", DBNull.Value);


                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListClient, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<Client>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByEmail - ClientDAL: " + ex);
                return null;
            }
        }
        public Client GetClientByTaxNo(string TaxNo)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@Email", DBNull.Value);
                objParam[1] = new SqlParameter("@TaxNo", TaxNo);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListClient, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<Client>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByTaxNo - ClientDAL: " + ex);
                return null;
            }
        }
        private DateTime CheckDate(string dateTime)
        {
            DateTime _date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateTime))
            {
                _date = DateTime.ParseExact(dateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return _date != DateTime.MinValue ? _date : DateTime.MinValue;
        }
        public async Task<List<Client>> GetClientSuggesstion(string txt_search)
        {
            try
            {
                List<Client> result = new List<Client>();

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (txt_search == null || txt_search.Trim() == "")
                    {
                        result = await _DbContext.Clients.AsNoTracking().OrderByDescending(x => x.JoinDate).Take(20).ToListAsync();
                    }
                    else
                    {
                        txt_search = txt_search.Trim();
                        result = await _DbContext.Clients.AsNoTracking().Where(x => x.Email.Contains(txt_search) || x.ClientName.Contains(txt_search) || x.Phone.Contains(txt_search)).OrderByDescending(x => x.JoinDate).Take(20).ToListAsync();
                    }

                }
                return result;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientSuggesstion - ClientDAL: " + ex);
                return new List<Client>();
            }
        }
        public async Task<DataTable> getClientid(long Client)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientID", Client);

                return _DbWorker.GetDataTable(StoreProcedureConstant.GetClientByID, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientid - ClientDal: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetAmountRemainOfContractByClientId(long Client)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientId", Client);

                return _DbWorker.GetDataTable(StoreProcedureConstant.Sp_GetAmountRemainOfContractPayByClientId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAmountRemainOfContractByClientId - ClientDal: " + ex);
            }
            return null;
        }
        public async Task<Client> GetClientByClientCode(string client_code)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.ClientCode.ToLower() == client_code);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByClientCode - ClientDAL: " + ex);
                return null;
            }
        }
        public int countClientTypeUse(int client_type)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientType", client_type);

                DataTable tb = new DataTable();
                return _DbWorker.ExecuteNonQuery("Sp_CountClientByType", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("countClientTypeUse - ClientDAL: " + ex.ToString());
                return -1;
            }
        }
    }
}