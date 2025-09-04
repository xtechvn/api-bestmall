using DAL;
using Entities.ConfigModels;
using Entities.Models;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Repositories.Repositories
{
    public class BankingAccountRepository : IBankingAccountRepository
    {
        private readonly BankingAccountDAL bankingAccountDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public BankingAccountRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            bankingAccountDAL = new BankingAccountDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }

        public List<BankingAccount> GetBankAccountByClientId(long clientId)
        {
            List<BankingAccount> data = new List<BankingAccount>();
            try
            {
                var dt = bankingAccountDAL.GetBankAccountByClientId(clientId);
                LogHelper.InsertLogTelegram("GetBankAccountByClientId - BankingAccountRepository ["+clientId+"]["+ (dt != null && dt.Rows != null && dt.Rows.Count > 0?dt.Rows.Count:"0/NULL") + "] ");

                if (dt != null &&dt.Rows!=null && dt.Rows.Count>0) {
                    data = dt.ToList<BankingAccount>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetBankAccountByClientId - BankingAccountRepository: " + ex);
            }
            return data;
        }
        public async Task<List<BankingAccount>> GetByClientId(long client_id)
        {
            return await bankingAccountDAL.GetByClientId(client_id);
        }

        public BankingAccount GetById(int bankAccountId)
        {
            return bankingAccountDAL.GetById(bankAccountId);
        }
        public int UpsertBankingAccount(BankingAccount model)
        {
            try
            {
                if (model.Id > 0)
                {
                    return bankingAccountDAL.UpdateBankingAccount(model);
                }
                else
                {
                    return bankingAccountDAL.InsertBankingAccount(model);
                }
            }
            catch
            {
                throw;
            }
        }
        public int Insert(BankingAccount model)
        {
            try
            {
                if (model.Id <= 0)
                {
                    return bankingAccountDAL.InsertBankingAccount(model);
                }
            }
            catch
            {
            }
            return -1;
        }
        public int Update(BankingAccount model)
        {
            try
            {
                if (model.Id > 0)
                {
                    return bankingAccountDAL.UpdateBankingAccount(model);
                }
            }
            catch
            {
            }
            return -1;
        }
    }
}
