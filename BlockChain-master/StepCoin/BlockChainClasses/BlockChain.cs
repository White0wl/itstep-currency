using StepCoin.BaseClasses;
using StepCoin.Hash;
using StepCoin.Validators;
using System.Collections.Generic;
using System.Linq;

namespace StepCoin.BlockChainClasses
{
    public class BlockChain : IBlockChain
    {
        public IEnumerable<BaseBlock> Blocks { get => _chain.Select(b => b.Clone() as BaseBlock); }
        private List<BaseBlock> _chain = new List<BaseBlock>();
        public IEnumerable<BaseTransaction> TransactionsOnBlocks => _chain.Aggregate(new List<BaseTransaction>(), AggregateTransactions);//Получение всех транзакций из блоков

        private List<BaseTransaction> AggregateTransactions(List<BaseTransaction> list, BaseBlock block)//Метод для накопления транзакций их блоков в один лист
        {
            list.AddRange(block.Transactions);
            return list;
        }

        public BlockChain()
        {
            _chain.Add(BlockZero);
        }

        private BaseBlock BlockZero => new Block(new HashCode(new string('0', BlockChainConfigurations.ActualDifficulty)), _chain.Count) { Hash = new HashCode(new string('0', BlockChainConfigurations.ActualDifficulty)) };

        /// <summary>
        /// Метод проверяет и добавляет блок в BlockChain новый Block
        /// </summary>
        /// <param name="newBlock"></param>
        public bool TryAddBlock(BaseBlock newBlock)
        {
            bool result = BlockValidator.IsCanBeAddedToChain(newBlock, _chain.Last());
            if (result)
            {
                _chain.Add(newBlock);
            }

            return result;
        }
    }
}