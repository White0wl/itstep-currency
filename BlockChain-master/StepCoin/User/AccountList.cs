using StepCoin.BaseClasses;
using System;
using System.Collections.ObjectModel;
using StepCoin.Hash;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;

namespace StepCoin.User
{
    public static class AccountList
    {
        public static readonly List<BaseAccount> Accounts;
        public static readonly string DefaultPath = "accounts.xml";
        private static string _path = ConfigurationManager.AppSettings["AccountsFilePath"];
        static AccountList()
        {
            CheckPath();
            try
            { Accounts = new List<BaseAccount>(LoadAccounts()); }
            catch
            { Accounts = new List<BaseAccount>(); }
        }

        public static void AddAccount(BaseAccount account)
        {
            CheckAccountOnNull(account);
            if (Contains(account))
                throw new ArgumentException("User with such a keys is already registered");
            Accounts.Add(account);
            SaveAccounts();
        }

        public static bool Contains(BaseAccount account) =>
            Accounts.FirstOrDefault(a => a.PublicCode == account.PublicCode && a.SecretCode == account.SecretCode) != null;

        public static int Contains(HashCode publicKey) =>
            Accounts.Count(a => a.PublicCode == publicKey);

        private static void CheckAccountOnNull(BaseAccount account)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));
            if (HashCode.IsNullOrWhiteSpace(account.PublicCode)) throw new ArgumentNullException(nameof(account.PublicCode));
            if (HashCode.IsNullOrWhiteSpace(account.PublicCode)) throw new ArgumentNullException(nameof(account.SecretCode));
        }

        private static void SaveAccounts()
        {
            CheckPath();
            using (var fileStram = new FileStream(_path, FileMode.Create, FileAccess.ReadWrite))
                new DataContractSerializer(typeof(BaseAccount[])).WriteObject(fileStram, Accounts.ToArray());
        }

        private static void CheckPath() => _path = string.IsNullOrWhiteSpace(_path) ? DefaultPath : _path;

        private static IEnumerable<BaseAccount> LoadAccounts()
        {
            CheckPath();
            BaseAccount[] array;
            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read))
                array = new DataContractSerializer(typeof(BaseAccount[])).ReadObject(stream) as BaseAccount[];
            return array;
        }

        //public void DepositeWithdraw(Transaction transaction)
        //{
        //    Accounts.FirstOrDefault(a => a.Code == transaction.Sender)?.OutcomingTransactions.Add(transaction);
        //    Accounts.FirstOrDefault(a => a.Code == transaction.Recipient)?.IncomingTransactions.Add(transaction);
        //}
    }
}