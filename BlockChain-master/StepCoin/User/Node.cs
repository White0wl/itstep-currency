using LoggerLibrary;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public Account Account { get; set; }
        public BlockChain BlockChain { get; set; }
        public Miner Miner { get; set; }

        public NodeObserver Observer { get; set; } = new NodeObserver();

        //Блоки и транзакции ожидающие подтверждения других пользователей
        public IEnumerable<PendingConfirmChainElement> PendingConfirmElements { get { return _pendingConfirmElements.Select(pe => pe.GetClone()); } }
        private List<PendingConfirmChainElement> _pendingConfirmElements = new List<PendingConfirmChainElement>();

        public async void StartMineAsync()
        {
            if (Miner.PendingToMineTransactions.Count() > 0)
            {
                Logger.Instance.LogMessage($"{Account.PublicAddress} start mining");
                Block newBlock = await Miner.StartMineBlock();
                Logger.Instance.LogMessage($"{Account.PublicAddress} finished mining {newBlock.Hash}");
                AddOrChangePendingElement(new PendingConfirmChainElement(newBlock));
            }
        }

        public Node(string name)
        {
            Account = new Account(name);
            AccountList.ListOfAllAccounts.Add(Account);

            BlockChain = new BlockChain();
            Miner = new Miner(BlockChain, Account.PublicAddress);

            //if (baseNode != null)
            //    SubscribeToEvents(baseNode);
        }

        public void NotifyAboutPendingElement(PendingConfirmChainElement element)
        {
            Observer.Nodes.ForEach(n => n.AddOrChangePendingElement(element));
        }

        public void NotifyAboutBlock(Block newBlock)
        {
            Observer.Nodes.ForEach(n => n.AddBlock(newBlock));
        }

        #region OldVersionNotification
        //private void SubscribeToEvents(Node baseNode)
        //{
        //    baseNode.PendingConfirmElements.CollectionChanged += PendingConfirmElements_CollectionChanged;
        //    baseNode.BlockChain.Chain.CollectionChanged += Chain_CollectionChanged;
        //}

        //private void PendingConfirmElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Add)
        //    {
        //        foreach (var item in e.NewItems)
        //        {
        //            var newItem = item as PendingConfirmChainElement;
        //            if (newItem != null)
        //            {
        //                var pendingElementOnList = PendingConfirmElements.FirstOrDefault(pe => pe.Element.Hash == newItem.Element.Hash);
        //                bool notCntains = pendingElementOnList is null;


        //                if (notCntains)
        //                    pendingElementOnList = newItem.Clone;

        //                if (newItem.Element is Block)
        //                {
        //                    pendingElementOnList.Confirmations[Account.PublicAddress] = Validator.IsCanBeAddedToChain(pendingElementOnList.Element as Block, BlockChain);
        //                }
        //                else if (newItem.Element is Transaction)
        //                {
        //                    pendingElementOnList.Confirmations[Account.PublicAddress] = TransactionsValidator.IsValidTransaction(pendingElementOnList.Element as Transaction,
        //                        BlockChain.TransactionsOnChain
        //                        .Union(Miner.PendingToMineTransactions)
        //                        .Union(PendingConfirmElements.Where(pe => pe.Element is Transaction).Select(pe => pe.Element as Transaction)));
        //                }

        //                if (notCntains)
        //                    PendingConfirmElements.Add(pendingElementOnList);
        //            }
        //            FoundAndAddConfirmedBlocks();
        //            FoundAndAddConfirmedTransactions();
        //        }
        //    }
        //}


        //private void Chain_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Add)
        //    {
        //        foreach (var item in e.NewItems)
        //        {
        //            var newItem = item as Block;
        //            if (newItem != null)
        //            {
        //                BlockChain.TryAddBlock(newItem);
        //            }
        //        }
        //    }
        //}
        #endregion

        internal void AddOrChangePendingElement(PendingConfirmChainElement element)
        {
            if (element != null)
            {
                var pendingElementOnList = _pendingConfirmElements.FirstOrDefault(pe => pe.Element.Hash == element.Element.Hash);

                if (pendingElementOnList is null)
                {
                    AddPendingElement(element.GetClone());
                }
                else
                {
                    foreach (var confirmation in element.Confirmations.Where(c=>c.Key!=Account.PublicAddress))
                    {
                        pendingElementOnList.Confirmations[confirmation.Key] = confirmation.Value;
                    }
                    ConfirmOrDenyElement(pendingElementOnList);
                }
            }
        }




        //Проверка елемента который уже есть в списке PendingConfirmElements и уведомление подписчиков об изменении результата подтверждения
        //Если текущий Node уже подтверждал/опровергал ожидающий елемент, подписчики будут уведомлены лишь в случае изменения решения
        private void ConfirmOrDenyElement(PendingConfirmChainElement pendingElement)
        {
            if (pendingElement is null) return;

            bool isNeedNotify = true;
            if (pendingElement.Confirmations.ContainsKey(Account.PublicAddress))
            {
                bool oldValue = pendingElement.Confirmations[Account.PublicAddress];
                pendingElement.Confirmations[Account.PublicAddress] = IsConfirm(pendingElement);
                isNeedNotify = oldValue != pendingElement.Confirmations[Account.PublicAddress];
            }
            else
            {
                pendingElement.Confirmations[Account.PublicAddress] = IsConfirm(pendingElement);
            }

            if (isNeedNotify)
                NotifyAboutPendingElement(pendingElement);

        }

        private void AddPendingElement(PendingConfirmChainElement pendingElement)
        {
            if (pendingElement is null) return;
            _pendingConfirmElements.Add(pendingElement);
            pendingElement.Confirmations[Account.PublicAddress] = IsConfirm(pendingElement);
            NotifyAboutPendingElement(pendingElement);
        }

        private bool IsConfirm(PendingConfirmChainElement pendingConfirmChainElement)
        {
            bool result = false;
            if (pendingConfirmChainElement.Element is Block)
            {
                result = Validator.IsCanBeAddedToChain(pendingConfirmChainElement.Element as Block, BlockChain);
            }
            else if (pendingConfirmChainElement.Element is Transaction)
            {
                result = TransactionsValidator.IsValidTransaction(pendingConfirmChainElement.Element as Transaction,
                    BlockChain.TransactionsOnChain
                    .Union(Miner.PendingToMineTransactions)
                    .Union(_pendingConfirmElements.Where(pe => pe.Element is Transaction && pe.Element.Hash != pendingConfirmChainElement.Element.Hash).Select(pe => pe.Element as Transaction)));
            }
            return result;
        }

        private void AddBlock(Block newBlock)
        {
            if (newBlock != null)
            {
                if (BlockChain.TryAddBlock(newBlock))
                    NotifyAboutBlock(newBlock);
            }
        }

        public IEnumerable<Block> FoundConfirmedBlocks() => Validator.ConfirmedBlocks(_pendingConfirmElements);
        public void FoundAndAddConfirmedBlocks()
        {
            var blocks = FoundConfirmedBlocks();
            foreach (var block in blocks)
            {
                AddBlock(block);
                _pendingConfirmElements.Remove(_pendingConfirmElements.FirstOrDefault(pe => pe.Element == block));
            }
        }

        IEnumerable<Transaction> FoundConfirmedTransactions() => TransactionsValidator.ConfirmedTransactions(_pendingConfirmElements);
        public void FoundAndAddConfirmedTransactions()
        {
            foreach (var transaction in FoundConfirmedTransactions().ToList())
            {
                Miner.TryAddNewTransaction(transaction);
                _pendingConfirmElements.Remove(_pendingConfirmElements.FirstOrDefault(pe => pe.Element == transaction));
            }
        }



        public void GenerateNewTransaction(HashCode recipient, decimal amount)
        {
            Random r = new Random();
            var newTransaction = new Transaction(Account.PublicAddress, recipient, amount, BlockChain.TransactionsOnChain.Count);
            Logger.Instance.LogMessage($"{Account.PublicAddress} generate new transaction\r\n{newTransaction}\r\n{newTransaction.Hash}");

            AddOrChangePendingElement(new PendingConfirmChainElement(newTransaction));
        }

    }
}
