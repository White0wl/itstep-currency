using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StepCoin.User
{
    public static class AccountList
    {
        public static ObservableCollection<Account> ListOfAllAccounts { get; set; } = new ObservableCollection<Account>();

        static AccountList()
        {
            ListOfAllAccounts.CollectionChanged += ListOfAllAccounts_CollectionChanged;
        }

        private static void ListOfAllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Configurations.TransactionCountConfirmations = Convert.ToInt32(ListOfAllAccounts.Count * 0.5);
            Configurations.BlockCountConfirmations = Convert.ToInt32(ListOfAllAccounts.Count * 0.5);
        }

        //public void DepositeWithdraw(Transaction transaction)
        //{
        //    ListOfAllAccounts.FirstOrDefault(a => a.Code == transaction.Sender)?.OutcomingTransactions.Add(transaction);
        //    ListOfAllAccounts.FirstOrDefault(a => a.Code == transaction.Recipient)?.IncomingTransactions.Add(transaction);
        //}
    }
}