using StepCoin.BaseClasses;
using StepCoin.Hash;
using StepCoin.Validators;
using System.Collections.Generic;
using System.Linq;

namespace StepCoin.BlockChainClasses
{
    public class BlockChain : IBlockChain
    {
        public IEnumerable<IBlock> Blocks { get => _chain.Select(b => b.Clone() as IBlock); }
        private List<IBlock> _chain = new List<IBlock>();
        public IEnumerable<ITransaction> TransactionsOnBlocks => _chain.Aggregate(new List<ITransaction>(), AggregateTransactions);//Получение всех транзакций из блоков

        private List<ITransaction> AggregateTransactions(List<ITransaction> list, IBlock block)//Метод для накопления транзакций их блоков в один лист
        {
            list.AddRange(block.Transactions);
            return list;
        }

        public BlockChain()
        {
            _chain.Add(BlockZero);
        }

        private IBlock BlockZero => new Block(new HashCode(new string('0', BlockChainConfigurations.ActualDifficulty)), _chain.Count) { Hash = new HashCode(new string('0', BlockChainConfigurations.ActualDifficulty)) };

        /// <summary>
        /// Метод проверяет и добавляет блок в BlockChain новый Block
        /// </summary>
        /// <param name="newBlock"></param>
        public bool TryAddBlock(IBlock newBlock)
        {
            bool result = Validator.IsCanBeAddedToChain(newBlock, _chain.Last());
            if (result)
            {
                _chain.Add(newBlock);
            }

            return result;
        }
    }
}