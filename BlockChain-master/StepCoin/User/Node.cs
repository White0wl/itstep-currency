using LoggerLibrary;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.Validators;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace StepCoin.User
{
    public class Node
    {
        public Account Account { get; set; }
        public BlockChain BlockChain { get; set; }
        public Miner Miner { get; set; }

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

        public Node(string name, Node baseNode = null)
        {
            Account = new Account(name);
            AccountList.ListOfAllAccounts.Add(Account);

            BlockChain = new BlockChain();
            Miner = new Miner(BlockChain, Account.PublicAddress);

            if (baseNode != null)
                SubscribeToEvents(baseNode);
        }

        private void SubscribeToEvents(Node baseNode)
        {
            baseNode.PendingConfirmElements.CollectionChanged += PendingConfirmElements_CollectionChanged;
            baseNode.BlockChain.Chain.CollectionChanged += Chain_CollectionChanged;
        }

        private void PendingConfirmElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var newItem = item as PendingConfirmChainElement;
                    if (newItem != null)
                    {
                        var pendingElementOnList = PendingConfirmElements.FirstOrDefault(pe => pe.Element.Hash == newItem.Element.Hash);
                        bool notCntains = pendingElementOnList is null;


                        if (notCntains)
                            pendingElementOnList = newItem.Clone;

                        if (newItem.Element is Block)
                        {
                            pendingElementOnList.Confirmations[Account.PublicAddress] = Validator.IsCanBeAddedToChain(pendingElementOnList.Element as Block, BlockChain);
                        }
                        else if (newItem.Element is Transaction)
                        {
                            pendingElementOnList.Confirmations[Account.PublicAddress] = TransactionsValidator.IsValidTransaction(pendingElementOnList.Element as Transaction,
                                BlockChain.TransactionsOnChain
                                .Union(Miner.PendingToMineTransactions)
                                .Union(PendingConfirmElements.Where(pe => pe.Element is Transaction).Select(pe => pe.Element as Transaction)));
                        }

                        if (notCntains)
                            PendingConfirmElements.Add(pendingElementOnList);
                    }
                    FoundAndAddConfirmedBlocks();
                    FoundAndAddConfirmedTransactions();
                }
                //PendingConfirmBlock block = (sender as ObservableCollection<PendingConfirmBlock>)?.Last();
                //if (block != null)
                //{
                //    block.Confirmations[Account.PublicAddress] = Validator.IsCanBeAddedToChain(block.Block, BlockChain);
                //    PendingBlocks.Add(block.Clone);
                //    AddConfirmedBlocks();
                //}
            }
        }

        private void FoundAndAddConfirmedBlocks()
        {
            foreach (var block in Validator.ConfirmedBlocks(PendingConfirmElements))
            {
                BlockChain.TryAddBlock(block);
                PendingConfirmElements.Remove(PendingConfirmElements.FirstOrDefault(pe => pe.Element == block));
            }
        }

        private void FoundAndAddConfirmedTransactions()
        {
            foreach (var transaction in TransactionsValidator.ConfirmedTransactions(PendingConfirmElements))
            {
                Miner.TryAddNewTransaction(transaction);
                PendingConfirmElements.Remove(PendingConfirmElements.FirstOrDefault(pe => pe.Element == transaction));
            }
        }

        private void Chain_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var newItem = item as Block;
                    if (newItem != null)
                    {
                        BlockChain.TryAddBlock(newItem);
                    }
                }
            }
        }

        public void GenerateNewTransaction(HashCode sender, HashCode recipient, decimal amount)
        {
            Random r = new Random();
            var newTransaction = new Transaction(sender, recipient, amount, BlockChain.TransactionsOnChain.Count);
            Logger.Instance.LogMessage($"{Account.PublicAddress} generate new transaction\r\n{newTransaction}\r\n{newTransaction.Hash}");

            PendingConfirmElements.Add(new PendingConfirmChainElement(newTransaction));

        }
    }
}
