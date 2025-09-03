using DAL;
using Entities.Models;
using HuloToys_Service.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories.IRepositories
{
    public interface IBankingAccountRepository
    {
        BankingAccount GetById(int bankAccountId);
        List<BankingAccount> GetBankAccountByClientId(long clientId);
        public int UpsertBankingAccount(BankingAccount model);
        public int Insert(BankingAccount model);
        public int Update(BankingAccount model);
    }
}
