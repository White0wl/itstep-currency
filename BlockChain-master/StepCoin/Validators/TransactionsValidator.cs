using StepCoin.BaseClasses;
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
        public static bool IsValidTransaction(ITransaction someTransaction, IEnumerable<ITransaction> transactions)
        {
            bool result = false;
            if (!IsValidAddresses(someTransaction.Sender, someTransaction.Recipient)) return result;

            decimal recieved = ReceivedTransactions(someTransaction.Sender, transactions).Sum(t => t.Amount);
            decimal sent = SentTransactions(someTransaction.Sender, transactions).Sum(t => t.Amount);
            result = ((recieved - sent - someTransaction.Amount + BlockChainConfigurations.StartBalance) >= 0 && someTransaction.Amount > 0);

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

        public static IEnumerable<ITransaction> SentTransactions(HashCode sender, IEnumerable<ITransaction> transactions) =>
            transactions.Where(t => t.Sender == sender);

        public static IEnumerable<ITransaction> ReceivedTransactions(HashCode recipient, IEnumerable<ITransaction> transactions) =>
            transactions.Where(t => t.Recipient == recipient);

        public static IEnumerable<ITransaction> ConfirmedTransactions(IEnumerable<PendingConfirmChainElement> pendingConfirmElements) =>
            pendingConfirmElements
            .Where(pe => pe.Element is ITransaction)//Нахождение всех ожидающих транзакций, исключая блоки
            .Where(pe => pe.Confirmations.Where(c => c.Value).Count() >= BlockChainConfigurations.TransactionCountConfirmations)//Проверка кол.подтверждений
            .Where(pe => (DateTime.Now - pe.PendingStartTime) >= BlockChainConfigurations.TransactionConfirmationTime)//Проверка времени распространения
            .Select(pe => pe.Element as ITransaction);
    }
}