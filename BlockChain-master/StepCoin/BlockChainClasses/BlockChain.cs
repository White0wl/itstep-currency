using StepCoin.BaseClasses;
using StepCoin.Hash;
using StepCoin.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;

namespace StepCoin.BlockChainClasses
{
    public class BlockChain : IBlockChain
    {
        public IEnumerable<BaseBlock> Blocks => _chain.Select(b => b.Clone() as BaseBlock);
        private readonly List<BaseBlock> _chain;



        public static readonly string DefaultPath = "blockChain.xml";
        private static string _path = ConfigurationManager.AppSettings["BlockChainFilePath"];

        public IEnumerable<BaseTransaction> TransactionsOnBlocks => _chain.Aggregate(new List<BaseTransaction>(), AggregateTransactions);//Получение всех транзакций из блоков

        private static List<BaseTransaction> AggregateTransactions(List<BaseTransaction> list, BaseBlock block)//Метод для накопления транзакций их блоков в один лист
        {
            list.AddRange(block.Transactions);
            return list;
        }

        public BlockChain()
        {
            CheckPath();
            try
            { _chain = new List<BaseBlock>(LoadBlockChain()); }
            catch
            {
                _chain = new List<BaseBlock> { BlockZero };
            }
        }

        private BaseBlock BlockZero => new Block(new HashCode(new string('0', BlockChainConfigurations.ActualDifficulty)), _chain.Count) { Hash = new HashCode(new string('0', BlockChainConfigurations.ActualDifficulty)) };

        /// <summary>
        /// Метод проверяет и добавляет блок в BlockChain новый Block
        /// </summary>
        /// <param name="newBlock"></param>
        public bool TryAddBlock(BaseBlock newBlock)
        {
            if (!BlockValidator.IsCanBeAddedToChain(newBlock, _chain.Last()).Key) return false;
            _chain.Add(newBlock);
            SaveBlockChain();
            return true;
        }

        private void SaveBlockChain()
        {
            CheckPath();
            using (var fileStram = new FileStream(_path, FileMode.Create, FileAccess.ReadWrite))
                new DataContractSerializer(typeof(BaseBlock[])).WriteObject(fileStram, Blocks.ToArray());
        }

        private static IEnumerable<BaseBlock> LoadBlockChain()
        {
            CheckPath();
            BaseBlock[] array;
            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read))
                array = new DataContractSerializer(typeof(BaseBlock[])).ReadObject(stream) as BaseBlock[];
            return array;
        }


        private static void CheckPath() => _path = string.IsNullOrWhiteSpace(_path) ? DefaultPath : _path;
        public decimal GetBalance(HashCode userPublicKey) => TransactionValidator.ActualBalance(userPublicKey, TransactionsOnBlocks.ToArray());
    }
}