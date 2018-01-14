using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepCoin.Validators
{
    public static class TransactionsValidator
    {
        public static bool IsValidTransaction(Transaction someTransaction, IEnumerable<Transaction> transactions)
        {
            bool result = false;
            if (IsValidAddresses(someTransaction.Sender, someTransaction.Recipient))
            {
                decimal amountRecieved = RecipentTransactions(someTransaction.Recipient, transactions).Sum(t => t.Amount);
                decimal amountSent = SenderTransactions(someTransaction.Sender, transactions).Sum(t => t.Amount);
                result = ((amountRecieved - amountSent - someTransaction.Amount + Configurations.StartBalance) >= 0 && someTransaction.Amount > 0);
            }
            return result;
        }

        public static bool IsValidAddresses(params HashCode[] hashArray)
        {
            bool result = false;
            foreach (var hash in hashArray)
            {
                result = !HashCode.IsNullOrWhiteSpace(hash);
                if (result) result = AccountList.ListOfAllAccounts.FirstOrDefault(a => a.PublicAddress == hash) != null;
                if (!result) break;
            }
            return result;
        }

        public static IEnumerable<Transaction> SenderTransactions(HashCode sender, IEnumerable<Transaction> transactions) =>
            transactions.Where(t => t.Sender == sender);

        public static IEnumerable<Transaction> RecipentTransactions(HashCode recipient, IEnumerable<Transaction> transactions) =>
            transactions.Where(t => t.Recipient == recipient);

        public static IEnumerable<Transaction> ConfirmedTransactions(IEnumerable<PendingConfirmChainElement> pendingConfirmElements) =>
            pendingConfirmElements
            .Where(pe => pe.Element is Transaction)//Нахождение всех ожидающих транзакций, исключая блоки
            .Where(pe => pe.Confirmations.Where(c => c.Value).Count() >= Configurations.TransactionCountConfirmations)//Проверка кол.подтверждений
            .Where(pe => (DateTime.Now - pe.PendingStartTime) >= Configurations.TransactionConfirmationTime)//Проверка времени распространения
            .Select(pe => pe.Element as Transaction);
    }
}