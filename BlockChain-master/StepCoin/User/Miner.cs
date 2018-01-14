using StepCoin.Hash;
using System.Collections.Generic;
using System.Linq;
using StepCoin.BlockChainClasses;
using StepCoin.Validators;
using System.Threading.Tasks;

namespace StepCoin.User
{
    public class Miner
    {
        internal HashCode PublicAddress { get; set; }
        private BlockChain _blockChain;
        public IEnumerable<Transaction> PendingToMineTransactions { get => _pendingToMineTransactions.Select(t => t.GetClone() as Transaction); }
        private List<Transaction> _pendingToMineTransactions = new List<Transaction>();

        private bool isMining;

        public bool TryAddNewTransaction(Transaction transaction)
        {
            bool result = TransactionsValidator.IsValidTransaction(transaction, _blockChain.TransactionsOnChain.Union(PendingToMineTransactions)) && Configurations.CountTransactionsToBlock > _pendingToMineTransactions.Count;
            if (result)
                _pendingToMineTransactions.Add(transaction);
            return result;
        }

        public Miner(BlockChain blockChain, HashCode addressMiner)
        {
            _blockChain = blockChain;
            PublicAddress = addressMiner;
        }

        public async Task<Block> StartMineBlock()
        {
            return await Task.Run(() =>
            {
                isMining = true;
                Block blockToMine = new Block(_blockChain.LastBlock().Hash, _blockChain.Chain.Count);
                blockToMine.Transactions = PendingToMineTransactions
                    .Union(new Transaction[] { new Transaction(PublicAddress, PublicAddress, Configurations.ActualDifficulty * 5, _blockChain.TransactionsOnChain.Count) });//+ Вознаграждение за майнинг в размере: актуальной сложности * 5
                _pendingToMineTransactions.Clear();
                while (blockToMine.CalculateNewHash(Configurations.ActualDifficulty, PublicAddress).Code.Substring(0, Configurations.ActualDifficulty) != new string('0', Configurations.ActualDifficulty))
                {
                }
                isMining = false;
                return blockToMine;
            });
        }


        //public string CalculateThisHash(Block newBlock)
        //{
        //    //thisBlockData - строка, в которую добвятся хэши всех транзакций из списка
        //    StringBuilder thisBlockData = new StringBuilder(
        //        newBlock.Id +
        //        CopyOfBlockChain.Chain.Last().Hash.ToString() +
        //        newBlock.Nonce);

        //    foreach (Transaction instance in MinigBlock.Transactions)
        //    {
        //        thisBlockData.Append(instance.CalculateTransactionHash());
        //    }
        //    string toEncrypt = thisBlockData.ToString();

        //    return new MD5Algorithm().GetHashString(toEncrypt);
        //}

    }
}