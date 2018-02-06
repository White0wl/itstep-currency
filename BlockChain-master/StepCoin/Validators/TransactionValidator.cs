using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepCoin.Validators
{
    public static class TransactionValidator
    {
        public static bool IsValidTransaction(BaseTransaction someTransaction, IEnumerable<BaseTransaction> transactions)
        {
            if (!IsValidAddresses(someTransaction.Sender, someTransaction.Recipient)) return false;

            var baseTransactions = transactions as BaseTransaction[] ?? transactions.ToArray();
            var recieved = ReceivedTransactions(someTransaction.Sender, baseTransactions).Sum(t => t.Amount);
            var sent = SentTransactions(someTransaction.Sender, baseTransactions).Sum(t => t.Amount);
            var result = ((recieved - sent - someTransaction.Amount + BlockChainConfigurations.StartBalance) >= 0 && someTransaction.Amount > 0);

            return result;
        }

        public static bool IsValidAddresses(params HashCode[] hashArray)
        {
            var result = false;
            foreach (var hash in hashArray)
            {
                result = !HashCode.IsNullOrWhiteSpace(hash);
                if (result) result = AccountList.Accounts.Contains(hash) ;
                if (!result) break;
            }
            return result;
        }

        public static IEnumerable<BaseTransaction> SentTransactions(HashCode sender, IEnumerable<BaseTransaction> transactions) =>
            transactions.Where(t => t.Sender == sender);

        public static IEnumerable<BaseTransaction> ReceivedTransactions(HashCode recipient, IEnumerable<BaseTransaction> transactions) =>
            transactions.Where(t => t.Recipient == recipient);

        public static IEnumerable<BaseTransaction> ConfirmedTransactions(IEnumerable<PendingConfirmChainElement> pendingConfirmElements) =>
            pendingConfirmElements
            .Where(pe => pe.Element is BaseTransaction)//Нахождение всех ожидающих транзакций, исключая блоки
            .Where(pe => pe.Confirmations.Count(c => c.Value) >= BlockChainConfigurations.TransactionCountConfirmations)//Проверка кол.подтверждений
            .Where(pe => (DateTime.Now - pe.PendingStartTime) >= BlockChainConfigurations.TransactionConfirmationTime)//Проверка времени распространения
            .Select(pe => pe.Element as BaseTransaction);
    }
}