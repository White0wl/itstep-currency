using LoggerLibrary;
using StepCoin.Hash;
using StepCoin.Validators;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StepCoin.BlockChainClasses
{
    public class BlockChain
    {
        internal ObservableCollection<Block> Chain { get; } = new ObservableCollection<Block>();
        public List<Transaction> TransactionsOnChain => Chain.Aggregate(new List<Transaction>(), AggregateTransactions);

        private List<Transaction> AggregateTransactions(List<Transaction> list, Block block)
        {
            list.AddRange(block.Transactions);
            return list;
        }

        public BlockChain()
        {
            Chain.Add(BlockZero);
        }

        private Block BlockZero => new Block(new HashCode(new string('0', Configurations.ActualDifficulty)), new HashCode(new string('0', Configurations.ActualDifficulty)), Chain.Count);

        /// <summary>
        /// Метод проверяет и добавляет блок в BlockChain
        /// </summary>
        /// <param name="newBlock"></param>
        public bool TryAddBlock(Block newBlock)
        {
            bool result = Validator.IsCanBeAddedToChain(newBlock, this);
            if (result)
            {
                Chain.Add(newBlock);
            }

            return result;
        }


        public Block LastBlock() => Chain.Last();
        public Block FirstBlock() => Chain.First();

    }
}