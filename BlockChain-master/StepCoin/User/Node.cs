using LoggerLibrary;
using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StepCoin.User
{

    public class NodeObserver
    {
        internal List<Node> Nodes { get; set; } = new List<Node>();//Лист уведомляемых Node

        public void Subscribe(Node node)//добавление Node в подписчики
        {
            if (Nodes.FirstOrDefault(n => n.Account.PublicAddress == node.Account.PublicAddress) is null)//поиск в списке этого же node если не найден, добавление
                Nodes.Add(node);
        }

        public void Describe(Node node) => Nodes.Remove(node);//Удаление из подписчиков
    }

    public class Node
    {
        public IAccount Account { get; }
        public Miner Miner { get; }

        public BlockChain BlockChain { get; }
        public NodeObserver Observer { get; } = new NodeObserver();
        /// <summary>
        /// Подтвержденные транзакции, транслируемые другим узлам
        /// </summary>
        public List<ITransaction> BroadcastTransactions { get; } = new List<ITransaction>();
        /// <summary>
        /// Транзакции участвующие в получении нового блока
        /// </summary>
        private List<ITransaction> miningTransactions = new List<ITransaction>();
        /// <summary>
        /// Блоки и транзакции ожидающие подтверждения других пользователей
        /// </summary>
        public IEnumerable<PendingConfirmChainElement> PendingConfirmElements => _pendingConfirmElements.Select(pe => pe.Clone());
        private List<PendingConfirmChainElement> _pendingConfirmElements = new List<PendingConfirmChainElement>();
        public bool IsCanMine => (BroadcastTransactions.Count() >= BlockChainConfigurations.CountTransactionsToBlock && !Miner.IsMining);
        CancellationTokenSource cancelTokenSource;

        public IBlock StartMine()
        {
            if (IsCanMine)
            {
                miningTransactions = GetTransactionsToMine();
                cancelTokenSource = new CancellationTokenSource();
                Logger.Instance.LogMessage($"{Account.PublicAddress} start mining");
                IBlock newBlock = Miner.MineBlock(miningTransactions, BlockChain.Blocks.Last(), cancelTokenSource.Token);
                Logger.Instance.LogMessage($"{Account.PublicAddress} finished mining {newBlock.Hash}");
                AddOrChangePendingElement(new PendingConfirmChainElement(newBlock));
                return newBlock;
            }
            return null;
        }

        private List<ITransaction> GetTransactionsToMine()
        {
            List<ITransaction> toMine = new List<ITransaction>();
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
            if (cancelTokenSource is null) return;
            if (cancelTokenSource.IsCancellationRequested) return;
            cancelTokenSource.Cancel();
            foreach (var transaction in miningTransactions)
            {
                BroadcastTransactions.Add(transaction);
            }
        }

        public Node(string name)
        {
            Account = new Account(name);
            AccountList.ListOfAllAccounts.Add(Account);

            BlockChain = new BlockChain();
            Miner = new Miner(Account.PublicAddress);

            //if (baseNode != null)
            //    SubscribeToEvents(baseNode);
        }

        public void NotifyAboutPendingElement(PendingConfirmChainElement element)
        {
            Observer.Nodes.ForEach(n => n.AddOrChangePendingElement(element));
        }

        public void NotifyAboutBlock(IBlock newBlock)
        {
            Observer.Nodes.ForEach(n => n.AddBlock(newBlock));
        }

        internal void AddOrChangePendingElement(PendingConfirmChainElement element)
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
            bool oldValue = IsConfirm(pendingElement.Element);
            bool newValue = oldValue;
            if (pendingElement.Confirmations.ContainsKey(Account.PublicAddress))
                oldValue = pendingElement.Confirmations[Account.PublicAddress];
            pendingElement.Confirmations[Account.PublicAddress] = newValue;
            if (oldValue != newValue)
                NotifyAboutPendingElement(pendingElement);

            if (pendingElement.CountConfirm >= BlockChainConfigurations.BlockCountConfirmations && pendingElement.Element is IBlock)
            {
                AddBlock(pendingElement.Element as IBlock);
            }
        }

        private void AddPendingElement(PendingConfirmChainElement pendingElement)
        {
            if (pendingElement is null) return;
            if (pendingElement.Element is IBlock)
            {
                if ((pendingElement.Element as IBlock).PrevHash != BlockChain.Blocks.Last().Hash) return;
            }
            _pendingConfirmElements.Add(pendingElement);
            pendingElement.Confirmations[Account.PublicAddress] = IsConfirm(pendingElement.Element);
            NotifyAboutPendingElement(pendingElement);
        }

        private bool IsConfirm(IChainElement pendingChainElement)
        {
            bool result = false;
            if (pendingChainElement is IBlock)
            {
                //Logger.Instance.LogMessage($"{(pendingConfirmChainElement.Element as Block).Hash} added to confirm {Account.PublicAddress}");
                result = BlockValidator.IsCanBeAddedToChain(pendingChainElement as IBlock, BlockChain.Blocks.Last());
                ;
            }
            else if (pendingChainElement is ITransaction)
            {
                result = TransactionValidator.IsValidTransaction(pendingChainElement as ITransaction,
                    BlockChain.TransactionsOnBlocks
                    .Union(BroadcastTransactions)
                    .Union(FoundConfirmedTransactions().Where(t => t.Hash != (pendingChainElement as ITransaction).Hash)));
            }
            return result;
        }

        private void AddBlock(IBlock newBlock)
        {
            if (newBlock is null) return;
            if (BlockChain.TryAddBlock(newBlock))
            {
                var blocksToRemove = _pendingConfirmElements.Where(pe => pe.Element is IBlock).Where(pe => (pe.Element as IBlock).PrevHash == newBlock.PrevHash).ToList();
                if (Miner.IsMining) StopMine();
                foreach (var block in blocksToRemove)
                {
                    _pendingConfirmElements.Remove(block);
                }
                var transactionsToRemove = new List<ITransaction>();
                foreach (var transaction in newBlock.Transactions)
                {
                    var transactionToRemove = BroadcastTransactions.FirstOrDefault(t => t.Hash == transaction.Hash);
                    if (transactionToRemove != null)
                        transactionsToRemove.Add(transactionToRemove);
                }
                foreach (var transaction in transactionsToRemove)
                {
                    BroadcastTransactions.Remove(transaction);
                }
                

                Logger.Instance.LogMessage($"{Account.PublicAddress} add block {newBlock.Hash}");
                NotifyAboutBlock(newBlock);
            }
        }

        public IBlock FoundConfirmedBlock()
        {
            var confirmedBlocks = BlockValidator.ConfirmedBlocks(_pendingConfirmElements);
            if (confirmedBlocks.Count() < 1) return null;
            var minTamestamp = confirmedBlocks.Min(b => b.DateOfReceiving);
            return confirmedBlocks.Where(b => b.DateOfReceiving == minTamestamp).FirstOrDefault();
        }

        public void FoundAndAddConfirmedBlock() => AddBlock(FoundConfirmedBlock());

        private IEnumerable<ITransaction> FoundConfirmedTransactions() => TransactionValidator.ConfirmedTransactions(_pendingConfirmElements);

        public void FoundAndAddConfirmedTransactions()
        {
            foreach (var transaction in FoundConfirmedTransactions().ToList())
            {
                if (BroadcastTransactions.Union(miningTransactions).FirstOrDefault(pe => pe.Hash == transaction.Hash) is null)
                    BroadcastTransactions.Add(transaction);
            }
        }



        public void GenerateNewTransaction(HashCode recipient, decimal amount)
        {
            Random r = new Random();
            var newTransaction = new Transaction(Account.PublicAddress, recipient, amount, BlockChain.TransactionsOnBlocks.Count());
            Logger.Instance.LogMessage($"{Account.PublicAddress} generate new transaction\r\n{newTransaction}\r\n{newTransaction.Hash}");

            AddOrChangePendingElement(new PendingConfirmChainElement(newTransaction));
        }

    }
}
