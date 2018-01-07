using LoggerLibrary;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace StepCoin.User
{

    public class NodeObserver
    {
        public List<Node> Nodes { get; set; } = new List<Node>();

        public void Attach(Node node)
        {
            if (Nodes.FirstOrDefault(n => n.Account.PublicAddress == node.Account.PublicAddress) is null)
                Nodes.Add(node);
        }

        public void Detach(Node node) => Nodes.Remove(node);

        public void BlockNotification(List<Block> blocks) =>
            Nodes.ForEach(n => blocks.ForEach(b => n.TryAddBlock(b)));
        public void PendingElementsNotification(List<PendingConfirmChainElement> pendingElements) =>
            Nodes.ForEach(n => pendingElements.ForEach(pe => n.TryAddPendingElement(pe)));
    }

    public class Node
    {
        public Account Account { get; set; }
        public BlockChain BlockChain { get; set; }
        public Miner Miner { get; set; }

        public NodeObserver Observer { get; set; } = new NodeObserver();

        //Блоки ожидающие подтверждения других пользователей
        public ObservableCollection<PendingConfirmChainElement> PendingConfirmElements { get; set; } = new ObservableCollection<PendingConfirmChainElement>();

        public async void StartMineAsync()
        {
            if (Miner.PendingToMineTransactions.Count() > 0)
            {
                Logger.Instance.LogMessage($"{Account.PublicAddress} start mining");
                Block newBlock = await Miner.StartMineBlock();
                Logger.Instance.LogMessage($"{Account.PublicAddress} finished mining {newBlock.Hash}");
                PendingConfirmElements.Add(new PendingConfirmChainElement(newBlock));
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
        
        public void NotifyAboutPendingElements() =>
                Observer.PendingElementsNotification(PendingConfirmElements.ToList());

        public void NotifyAboutBlocks() =>
                Observer.BlockNotification(BlockChain.Chain.ToList());

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

        internal void TryAddPendingElement(PendingConfirmChainElement newElement)
        {
            if (newElement != null)
            {
                var pendingElementOnList = PendingConfirmElements.FirstOrDefault(pe => pe.Element.Hash == newElement.Element.Hash);
                bool notCntains = pendingElementOnList is null;


                if (notCntains)
                    pendingElementOnList = newElement.Clone;

                if (newElement.Element is Block)
                {
                    pendingElementOnList.Confirmations[Account.PublicAddress] = Validator.IsCanBeAddedToChain(pendingElementOnList.Element as Block, BlockChain);
                }
                else if (newElement.Element is Transaction)
                {
                    pendingElementOnList.Confirmations[Account.PublicAddress] = TransactionsValidator.IsValidTransaction(pendingElementOnList.Element as Transaction,
                        BlockChain.TransactionsOnChain
                        .Union(Miner.PendingToMineTransactions)
                        .Union(PendingConfirmElements.Where(pe => pe.Element is Transaction).Select(pe => pe.Element as Transaction)));
                }

                if (notCntains)
                {
                    PendingConfirmElements.Add(pendingElementOnList);
                    NotifyAboutPendingElements();
                }
            }
        }

        internal void TryAddBlock(Block newBlock)
        {
            if (newBlock != null)
            {
                BlockChain.TryAddBlock(newBlock);
                NotifyAboutBlocks();
            }
        }

        public IEnumerable<Block> FoundConfirmedBlocks() => Validator.ConfirmedBlocks(PendingConfirmElements);
        public void FoundAndAddConfirmedBlocks()
        {
            var blocks = FoundConfirmedBlocks();
            foreach (var block in blocks)
            {
                BlockChain.TryAddBlock(block);
                PendingConfirmElements.Remove(PendingConfirmElements.FirstOrDefault(pe => pe.Element == block));
            }
        }

        IEnumerable<Transaction> FoundConfirmedTransactions() => TransactionsValidator.ConfirmedTransactions(PendingConfirmElements);
        public void FoundAndAddConfirmedTransactions()
        {
            foreach (var transaction in FoundConfirmedTransactions().ToList())
            {
                Miner.TryAddNewTransaction(transaction);
                PendingConfirmElements.Remove(PendingConfirmElements.FirstOrDefault(pe => pe.Element == transaction));
            }
        }



        public void GenerateNewTransaction(HashCode recipient, decimal amount)
        {
            Random r = new Random();
            var newTransaction = new Transaction(Account.PublicAddress, recipient, amount, BlockChain.TransactionsOnChain.Count);
            Logger.Instance.LogMessage($"{Account.PublicAddress} generate new transaction\r\n{newTransaction}\r\n{newTransaction.Hash}");

            PendingConfirmElements.Add(new PendingConfirmChainElement(newTransaction));
        }

    }
}
