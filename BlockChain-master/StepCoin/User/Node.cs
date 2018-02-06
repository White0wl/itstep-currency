using LoggerLibrary;
using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Distribution;
using StepCoin.Hash;
using StepCoin.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StepCoin.User
{
    public class Node
    {
        public IAccount Account { get; }
        public Miner Miner { get; }
        public IDistribution Distribution
        {
            get => _distribution; set
            {
                if (_distribution != null)
                {
                    _distribution.BlockNotification -= NotificationNewBlock;
                    _distribution.PendingElementNotification -= NotificationPendingElement;
                }

                _distribution = value;

                if (_distribution is null) return;
                _distribution.BlockNotification += NotificationNewBlock;
                _distribution.PendingElementNotification += NotificationPendingElement;
            }
        }
        public BlockChain BlockChain { get; }
        /// <summary>
        /// Подтвержденные транзакции, транслируемые другим узлам
        /// </summary>
        public List<BaseTransaction> BroadcastTransactions { get; } = new List<BaseTransaction>();
        /// <summary>
        /// Транзакции участвующие в получении нового блока
        /// </summary>
        private List<BaseTransaction> _miningTransactions = new List<BaseTransaction>();
        /// <summary>
        /// Блоки и транзакции ожидающие подтверждения других пользователей
        /// </summary>
        public IEnumerable<PendingConfirmChainElement> PendingConfirmElements => _pendingConfirmElements.Select(pe => pe.Clone());
        private readonly List<PendingConfirmChainElement> _pendingConfirmElements = new List<PendingConfirmChainElement>();
        public bool IsCanMine => (BroadcastTransactions.Count() >= BlockChainConfigurations.CountTransactionsToBlock && !Miner.IsMining);
        private CancellationTokenSource _cancelTokenSource;
        private IDistribution _distribution;

        public BaseBlock StartMine()
        {
            if (!IsCanMine) return null;

            _miningTransactions = GetTransactionsToMine();
            _cancelTokenSource = new CancellationTokenSource();
            Logger.Instance.LogMessage($"{Account.PublicAddress} start mining");
            var newBlock = Miner.MineBlock(_miningTransactions, BlockChain.Blocks.Last(), _cancelTokenSource.Token);
            Logger.Instance.LogMessage($"{Account.PublicAddress} {(_cancelTokenSource.IsCancellationRequested ? "canceled" : "finished")} mining {newBlock?.Hash}");
            if (newBlock is null) return null;
            NotificationPendingElement(new PendingConfirmChainElement(newBlock));
            return newBlock;
        }

        private List<BaseTransaction> GetTransactionsToMine()
        {
            var toMine = new List<BaseTransaction>();
            foreach (var transaction in BroadcastTransactions)
            {
                toMine.Add(transaction);
                if (toMine.Count == BlockChainConfigurations.CountTransactionsToBlock) break;
            }
            foreach (var transaction in toMine)
            {
                BroadcastTransactions.Remove(transaction);
            }
            return toMine;
        }

        public void StopMine()
        {
            if (_cancelTokenSource is null) return;
            if (_cancelTokenSource.IsCancellationRequested) return;
            _cancelTokenSource.Cancel();
            foreach (var transaction in _miningTransactions)
            {
                BroadcastTransactions.Add(transaction);
            }
            _miningTransactions.Clear();
        }

        public Node(string name, string password, IDistribution distribution = null)
        {
            Account = new Account(name, password);
            AccountList.Accounts.Add(Account.PublicAddress);
            Distribution = distribution ?? new OneComputerDistribution();
            Distribution.ClientCode = Account.PublicAddress.Clone();
            BlockChain = new BlockChain();
            Miner = new Miner(Account.PublicAddress);
            //if (baseNode != null)
            //    SubscribeToEvents(baseNode);
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
                foreach (var confirmation in element.Confirmations.Where(c => c.Key != Account.PublicAddress))
                {
                    pendingElementOnList.Confirmations[confirmation.Key] = confirmation.Value;
                }
                ConfirmOrDenyElement(pendingElementOnList);
            }
        }




        //Проверка елемента который уже есть в списке PendingConfirmElements и уведомление подписчиков об изменении результата подтверждения
        //Если текущий Node уже подтверждал/опровергал ожидающий елемент, подписчики будут уведомлены лишь в случае изменения решения
        private void ConfirmOrDenyElement(PendingConfirmChainElement pendingElement)
        {
            if (pendingElement is null) return;
            var oldValue = IsConfirm(pendingElement.Element);
            var newValue = oldValue;
            if (pendingElement.Confirmations.ContainsKey(Account.PublicAddress))
                oldValue = pendingElement.Confirmations[Account.PublicAddress];
            pendingElement.Confirmations[Account.PublicAddress] = newValue;
            if (oldValue != newValue)
                Distribution.NotifyAboutPendingElement(pendingElement);

            if (pendingElement.CountConfirm >= BlockChainConfigurations.BlockCountConfirmations && pendingElement.Element is BaseBlock)
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
            pendingElement.Confirmations[Account.PublicAddress] = IsConfirm(pendingElement.Element);
            Distribution.NotifyAboutPendingElement(pendingElement);
        }

        private bool IsConfirm(BaseChainElement pendingChainElement)
        {
            var result = false;
            switch (pendingChainElement)
            {
                case BaseBlock _:
                    //Logger.Instance.LogMessage($"{(pendingConfirmChainElement.Element as Block).Hash} added to confirm {Account.PublicAddress}");
                    result = BlockValidator.IsCanBeAddedToChain(pendingChainElement as BaseBlock, BlockChain.Blocks.Last());
                    break;
                case BaseTransaction _:
                    result = TransactionValidator.IsValidTransaction(pendingChainElement as BaseTransaction,
                        BlockChain.TransactionsOnBlocks
                                .Union(_miningTransactions)
                            .Union(BroadcastTransactions)
                            .Union(FoundConfirmedTransactions().Where(t => t.Hash != (pendingChainElement as BaseTransaction)?.Hash)));
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
            var transactionsToRemove = newBlock.Transactions.Select(transaction => BroadcastTransactions.FirstOrDefault(t => t.Hash == transaction.Hash)).Where(toRemove => toRemove != null).ToList();
            foreach (var transaction in transactionsToRemove)
            {
                BroadcastTransactions.Remove(transaction);
            }
            foreach (var element in PendingConfirmElements)
            {
                NotificationPendingElement(element);
            }


            Logger.Instance.LogMessage($"{Account.PublicAddress} add block {newBlock.Hash}");
            Distribution.NotifyAboutBlock(newBlock);
        }

        public BaseBlock FoundConfirmedBlock()
        {
            var confirmedBlocks = BlockValidator.ConfirmedBlocks(_pendingConfirmElements).ToArray();
            if (!confirmedBlocks.Any()) return null;
            var minTamestamp = confirmedBlocks.Min(b => b.DateOfReceiving);
            return confirmedBlocks.FirstOrDefault(b => b.DateOfReceiving == minTamestamp);
        }

        public void FoundAndAddConfirmedBlock() => NotificationNewBlock(FoundConfirmedBlock());

        private IEnumerable<BaseTransaction> FoundConfirmedTransactions() => TransactionValidator.ConfirmedTransactions(_pendingConfirmElements);

        public void FoundAndAddConfirmedTransactions()
        {
            foreach (var transaction in FoundConfirmedTransactions().ToList())
            {
                if (BroadcastTransactions.Union(_miningTransactions).FirstOrDefault(pe => pe.Hash == transaction.Hash) is null)
                    BroadcastTransactions.Add(transaction);
            }
        }

        public void GenerateNewTransaction(HashCode recipient, decimal amount)
        {
            var newTransaction = new Transaction(Account.PublicAddress, recipient, amount, BlockChain.TransactionsOnBlocks.Count());
            Logger.Instance.LogMessage($"{Account.PublicAddress} generate new transaction\r\n{newTransaction}\r\n{newTransaction.Hash}");
            NotificationPendingElement(new PendingConfirmChainElement(newTransaction));
        }

    }
}
