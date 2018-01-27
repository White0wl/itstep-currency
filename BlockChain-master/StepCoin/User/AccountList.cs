using StepCoin.BaseClasses;
using System;
using System.Collections.ObjectModel;

namespace StepCoin.User
{
    public static class AccountList
    {
        public static ObservableCollection<IAccount> ListOfAllAccounts { get;  } = new ObservableCollection<IAccount>();

        static AccountList()
        {
            ListOfAllAccounts.CollectionChanged += ListOfAllAccounts_CollectionChanged;
        }

        private static void ListOfAllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BlockChainConfigurations.TransactionCountConfirmations = Convert.ToInt32(ListOfAllAccounts.Count * 0.5);
            BlockChainConfigurations.BlockCountConfirmations = Convert.ToInt32(ListOfAllAccounts.Count * 0.5);
        }

        //public void DepositeWithdraw(Transaction transaction)
        //{
        //    ListOfAllAccounts.FirstOrDefault(a => a.Code == transaction.Sender)?.OutcomingTransactions.Add(transaction);
        //    ListOfAllAccounts.FirstOrDefault(a => a.Code == transaction.Recipient)?.IncomingTransactions.Add(transaction);
        //}
    }
}