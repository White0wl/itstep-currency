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
            if (someTransaction is null) throw new ArgumentNullException(nameof(someTransaction));
            if (someTransaction.Amount <= 0) return false;
            if (!IsValidAddresses(someTransaction.Sender, someTransaction.Recipient)) return false;
            var baseTransactions = transactions as BaseTransaction[] ?? transactions.ToArray();
            var result = ActualBalance(someTransaction.Sender, baseTransactions) - someTransaction.Amount >= 0;

            return result;
        }

        private static decimal ActualBalance(HashCode sender, BaseTransaction[] baseTransactions) =>
            ReceivedTransactions(sender, baseTransactions).Sum(t => t.Amount) -
            SentTransactions(sender, baseTransactions).Sum(t => t.Amount) +
            BlockChainConfigurations.StartBalance;

        public static bool IsValidAddresses(params HashCode[] hashArray)
        {
            var result = false;
            foreach (var hash in hashArray)
            {
                result = !HashCode.IsNullOrWhiteSpace(hash);
                if (result) result = AccountList.Accounts.Contains(hash);
                if (!result) break;
            }
            return result;
        }

        /// <summary>
        /// Нахождение отправленных транзакций
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="transactions">Массив для поиска</param>
        public static IEnumerable<BaseTransaction> SentTransactions(HashCode sender, IEnumerable<BaseTransaction> transactions) =>
            transactions.Where(t => t.Sender == sender);


        /// <summary>
        /// Нахождение полученных транзакций
        /// </summary>
        /// <param name="recipient">Получатель</param>
        /// <param name="transactions">Массив для поиска</param>
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