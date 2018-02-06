using StepCoin.BaseClasses;
using System;
using System.Collections.ObjectModel;
using StepCoin.Hash;

namespace StepCoin.User
{
    public static class AccountList
    {
        public static ObservableCollection<HashCode> Accounts { get;  } = new ObservableCollection<HashCode>();

        static AccountList()
        {
            //Accounts.CollectionChanged += ListOfAllAccounts_CollectionChanged;
        }

        private static void ListOfAllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BlockChainConfigurations.TransactionCountConfirmations = Convert.ToInt32(Accounts.Count * 0.5);
            BlockChainConfigurations.BlockCountConfirmations = Convert.ToInt32(Accounts.Count * 0.5);
        }

        //public void DepositeWithdraw(Transaction transaction)
        //{
        //    Accounts.FirstOrDefault(a => a.Code == transaction.Sender)?.OutcomingTransactions.Add(transaction);
        //    Accounts.FirstOrDefault(a => a.Code == transaction.Recipient)?.IncomingTransactions.Add(transaction);
        //}
    }
}