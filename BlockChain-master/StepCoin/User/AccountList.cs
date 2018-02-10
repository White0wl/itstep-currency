using StepCoin.BaseClasses;
using System;
using System.Collections.ObjectModel;
using StepCoin.Hash;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace StepCoin.User
{
    public static class AccountList
    {
        private static readonly ObservableCollection<HashCode> _accounts;
        private static string path = "accounts.xml";
        public static IEnumerable<HashCode> Accounts => _accounts.Select(a => a.Clone());
        static AccountList()
        {
            try
            {
                var v = LoadAccounts() as HashCode[];
                _accounts = new ObservableCollection<HashCode>(v);
            }
            catch { _accounts = new ObservableCollection<HashCode>(); }
            _accounts.CollectionChanged += ListOfAllAccounts_CollectionChanged;
        }

        public static void AddAccountKey(HashCode publicKey)
        {

            if (HashCode.IsNullOrWhiteSpace(publicKey)) throw new ArgumentNullException(nameof(publicKey));
            if (_accounts.Contains(publicKey)) throw new ArgumentException("User with such a key is already registered");
            _accounts.Add(publicKey);
            SaveAccounts();
        }

        private static void SaveAccounts()
        {
            using (FileStream fileStram = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(HashCode[]));
                serializer.WriteObject(fileStram, _accounts.ToArray());
                fileStram.Position = 0;
            }
        }

        private static HashCode[] LoadAccounts()
        {
            HashCode[] array;
            using (Stream stream = new MemoryStream())
            {
                byte[] data = File.ReadAllBytes(path);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(typeof(HashCode[]));
                array = deserializer.ReadObject(stream) as HashCode[];
            }
            return array;
        }

        private static void ListOfAllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var countConfirmations = Convert.ToInt32(Math.Ceiling(_accounts.Count * 0.5));
            BlockChainConfigurations.TransactionCountConfirmations = countConfirmations;
            BlockChainConfigurations.BlockCountConfirmations = countConfirmations;
        }

        //public void DepositeWithdraw(Transaction transaction)
        //{
        //    Accounts.FirstOrDefault(a => a.Code == transaction.Sender)?.OutcomingTransactions.Add(transaction);
        //    Accounts.FirstOrDefault(a => a.Code == transaction.Recipient)?.IncomingTransactions.Add(transaction);
        //}
    }
}