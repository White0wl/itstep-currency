using LoggerLibrary;
using StepCoin.Hash;
using StepCoin.Validators;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StepCoin.BlockChainClasses
{
    public class BlockChain
    {
        public IEnumerable<Block> Blocks { get => Chain.Select(b => b.GetClone() as Block); }
        internal List<Block> Chain { get; } = new List<Block>();
        public List<Transaction> TransactionsOnChain => Chain.Aggregate(new List<Transaction>(), AggregateTransactions);//Получение всех транзакций из блоков

        private List<Transaction> AggregateTransactions(List<Transaction> list, Block block)//Метод для накопления транзакций их блоков в один лист
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
        /// Метод проверяет и добавляет блок в BlockChain новый Block
        /// </summary>
        /// <param name="newBlock"></param>
        public bool TryAddBlock(Block newBlock)
        {
            bool result = Validator.IsCanBeAddedToChain(newBlock, Chain.Last());
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