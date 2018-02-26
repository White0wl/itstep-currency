using LoggerLibrary;
using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Distribution;
using StepCoin.Hash;
using StepCoin.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StepCoin.User
{
    public class Node
    {
        public Account Account { get; }
        public Miner Miner { get; }
        public IDistribution Distribution
        {
            get => _distribution;
            set
            {
                if (_distribution != null)
                {
                    _distribution.BlockNotification -= NotificationNewBlock;
                    _distribution.PendingElementNotification -= NotificationPendingElement;
                    _distribution.RequestBlocks -= NotifyAboutAllBlocks;
                }

                _distribution = value;

                if (_distribution is null) return;
                _distribution.BlockNotification += NotificationNewBlock;
                _distribution.PendingElementNotification += NotificationPendingElement;
                _distribution.RequestBlocks += NotifyAboutAllBlocks;
            }
        }

        private void NotifyAboutAllBlocks() => BlockChain.Blocks.Where(b => b.Transactions.Any()).ToList().ForEach(b => Distribution.NotifyAboutBlock(b));
        private void NotifyAboutAllPendingElements() => _pendingConfirmElements.ForEach(pe => Distribution.NotifyAboutPendingElement(pe));
        public BlockChain BlockChain { get; }


        public IEnumerable<BaseTransaction> ReadyForMiningElements => FoundConfirmedTransactions().Where(t => !_miningTransactions.Contains(t));

        /// <summary>
        /// Подтвержденные транзакции, транслируемые другим узлам
        /// </summary>
        //public List<BaseTransaction> BroadcastTransactions { get; } = new List<BaseTransaction>();
        /// <summary>
        /// Транзакции участвующие в получении нового блока
        /// </summary>
        private List<BaseTransaction> _miningTransactions = new List<BaseTransaction>();
        /// <summary>
        /// Блоки и транзакции ожидающие подтверждения других пользователей
        /// </summary>
        /// 
        public decimal NodeBalance => BlockChain.GetBalance(Account.PublicCode);
        public IEnumerable<PendingConfirmChainElement> PendingConfirmElements => _pendingConfirmElements.Select(pe => pe.Clone());
        private readonly List<PendingConfirmChainElement> _pendingConfirmElements = new List<PendingConfirmChainElement>();
        public bool IsCanMine => (ReadyForMiningElements.Count() >= BlockChainConfigurations.TransactionsCountInBlock && !Miner.IsMining);
        private CancellationTokenSource _cancelTokenSource;
        private IDistribution _distribution;

        public Node(Account account, IDistribution distribution = null)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            Distribution = distribution ?? new OneComputerDistribution(account.PublicCode);
            BlockChain = new BlockChain();
            Miner = new Miner(Account.PublicCode);
            //if (baseNode != null)
            //    SubscribeToEvents(baseNode);
        }
        public BaseBlock StartMine()
        {
            if (!IsCanMine) return null;

            _miningTransactions = GetTransactionsToMine();
            _cancelTokenSource = new CancellationTokenSource();
            Logger.Instance.LogMessage($"{Account.PublicCode} start mining");
            var newBlock = Miner.MineBlock(_miningTransactions, BlockChain.Blocks.Last(), _cancelTokenSource.Token);
            Logger.Instance.LogMessage($"{Account.PublicCode} {(_cancelTokenSource.IsCancellationRequested ? "canceled" : "finished")} mining {newBlock?.Hash}");
            if (newBlock is null) return null;
            NotificationPendingElement(new PendingConfirmChainElement(newBlock));
            return newBlock;
        }

        private List<BaseTransaction> GetTransactionsToMine()
        {
            var toMine = new List<BaseTransaction>();
            foreach (var transaction in ReadyForMiningElements)
            {
                toMine.Add(transaction);
                if (toMine.Count == BlockChainConfigurations.TransactionsCountInBlock) break;
            }
            //foreach (var transaction in toMine)
            //{
            //    BroadcastTransactions.Remove(transaction);
            //}
            return toMine;
        }

        public void StopMine()
        {
            if (_cancelTokenSource is null) return;
            if (_cancelTokenSource.IsCancellationRequested) return;
            _cancelTokenSource.Cancel();
            //foreach (var transaction in _miningTransactions)
            //{
            //    BroadcastTransactions.Add(transaction);
            //}
            _miningTransactions.Clear();
        }

        public void NotificationPendingElement(PendingConfirmChainElement element)
        {
            if (element is null) return;

            var pendingElementOnList = _pendingConfirmElements.FirstOrDefault(pe => pe.Element.Hash == element.Element.Hash);

            if (pendingElementOnList is null)
            {
                AddPendingElement(element.Clone());
            }
            else
            {
                foreach (var confirmation in element.Confirmations.Where(c => c.Key != Account.PublicCode))
                {
                    pendingElementOnList.Confirmations[confirmation.Key] = confirmation.Value;
                }
                ConfirmOrDenyElement(pendingElementOnList);
            }
        }

        public void ClearPendingElements()
        {
            var list = _pendingConfirmElements.Select(pe => pe);
            foreach (var pendingElement in list)
            {
                _pendingConfirmElements.Remove(pendingElement);
            }
        }

        //Проверка елемента который уже есть в списке PendingConfirmElements и уведомление подписчиков об изменении результата подтверждения
        //Если текущий Node уже подтверждал/опровергал ожидающий елемент, подписчики будут уведомлены лишь в случае изменения решения
        private void ConfirmOrDenyElement(PendingConfirmChainElement pendingElement)
        {
            if (pendingElement is null) return;
            var oldValue = Confirm(pendingElement.Element);
            var newValue = oldValue;
            if (pendingElement.Confirmations.ContainsKey(Account.PublicCode))
                oldValue = pendingElement.Confirmations[Account.PublicCode];
            pendingElement.Confirmations[Account.PublicCode] = newValue;
            if (oldValue.Key != newValue.Key)
                Distribution.NotifyAboutPendingElement(pendingElement);

            if (pendingElement.CountConfirm >= BlockChainConfigurations.ConfirmationsCount && pendingElement.Element is BaseBlock)
            {
                NotificationNewBlock((BaseBlock)pendingElement.Element);
            }
        }

        private void AddPendingElement(PendingConfirmChainElement pendingElement)
        {
            if (pendingElement is null) return;
            if (pendingElement.Element is BaseBlock block)
            {
                if (block.PrevHash != BlockChain.Blocks.Last().Hash) return;
            }
            _pendingConfirmElements.Add(pendingElement);
            pendingElement.Confirmations[Account.PublicCode] = Confirm(pendingElement.Element);
            Distribution.NotifyAboutPendingElement(pendingElement);
        }

        private KeyValuePair<bool, string> Confirm(BaseChainElement pendingChainElement)
        {
            var result = new KeyValuePair<bool, string>();
            switch (pendingChainElement)
            {
                case BaseBlock _:
                    //Logger.Instance.LogMessage($"{(pendingConfirmChainElement.Element as Block).Hash} added to confirm {Account.PublicCode}");
                    result = BlockValidator.IsCanBeAddedToChain(pendingChainElement as BaseBlock, BlockChain.Blocks.Last());
                    break;
                case BaseTransaction _:
                    result = TransactionValidator.IsValidTransaction(pendingChainElement as BaseTransaction,
                        BlockChain.TransactionsOnBlocks
                                .Union(_miningTransactions)//Транзакции учавствующие в майнинге
                                                           //.Union(BroadcastTransactions)//транслируемые Транзакции
                            .Union(FoundConfirmedTransactions().Where(t => t.Hash != pendingChainElement?.Hash)));
                    break;
            }
            return result;
        }

        public void NotificationNewBlock(BaseBlock newBlock)
        {
            if (newBlock is null) return;
            if (!BlockChain.TryAddBlock(newBlock)) return;
            var blocksToRemove = _pendingConfirmElements.Where(pe => pe.Element is BaseBlock).Where(pe => (pe.Element as BaseBlock)?.PrevHash == newBlock.PrevHash).ToList();
            if (Miner.IsMining) StopMine();
            foreach (var block in blocksToRemove)
            {
                _pendingConfirmElements.Remove(block);
            }
            var transactionsToRemove = newBlock.Transactions;//newBlock.Transactions.Select(transaction => BroadcastTransactions.FirstOrDefault(t => t.Hash == transaction.Hash)).Where(toRemove => toRemove != null).ToList();
            foreach (var pendingElement in _pendingConfirmElements
                .Where(pe => pe.Element is BaseTransaction)
                .Where(pe => transactionsToRemove.FirstOrDefault(t => t.Hash == pe.Element.Hash) != null))
            {
                RemovePendingElelment(pendingElement);
            }
            Logger.Instance.LogMessage($"{Account.PublicCode} add block {newBlock.Hash}");
            Distribution.NotifyAboutBlock(newBlock);
            foreach (var element in PendingConfirmElements)
            {
                NotificationPendingElement(element);
            }
        }

        public void RemovePendingElelment(PendingConfirmChainElement pendingElement) => _pendingConfirmElements.Remove(pendingElement);

        public BaseBlock FoundConfirmedBlock()
        {
            var confirmedBlocks = BlockValidator.ConfirmedBlocks(_pendingConfirmElements).ToArray();
            if (!confirmedBlocks.Any()) return null;
            var minTamestamp = confirmedBlocks.Min(b => b.DateOfReceiving);
            return confirmedBlocks.FirstOrDefault(b => b.DateOfReceiving == minTamestamp);
        }

        public void FoundAndAddConfirmedBlock() => NotificationNewBlock(FoundConfirmedBlock());

        private IEnumerable<BaseTransaction> FoundConfirmedTransactions() => TransactionValidator.ConfirmedTransactions(_pendingConfirmElements);

        public void GenerateNewTransaction(HashCode recipient, decimal amount)
        {
            var newTransaction = new Transaction(Account.PublicCode, recipient, amount, BlockChain.TransactionsOnBlocks.Count());
            Logger.Instance.LogMessage($"{Account.PublicCode} generate new transaction\r\n{newTransaction}\r\n{newTransaction.Hash}");
            NotificationPendingElement(new PendingConfirmChainElement(newTransaction));
        }

    }
}
