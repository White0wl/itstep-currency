using StepCoin.Hash;
using System.Collections.Generic;
using System.Linq;
using StepCoin.BlockChainClasses;
using System.Threading.Tasks;
using StepCoin.BaseClasses;
using System.Threading;

namespace StepCoin.User
{
    public class Miner
    {
        public HashCode PublicAddress { get; private set; }
        public bool IsMining { get; set; }

        public Miner(HashCode addressMiner) => PublicAddress = addressMiner;

        public BaseBlock MineBlock(IEnumerable<BaseTransaction> transactionsToMine, BaseBlock lastBlockInChain, CancellationToken token)
        {
            IsMining = true;
            var blockToMine = new Block(lastBlockInChain.Hash, lastBlockInChain.Id + 1);
            var baseTransactions = transactionsToMine as BaseTransaction[] ?? transactionsToMine.ToArray();
            //+ Вознаграждение за майнинг в размере: актуальной сложности * 5
            baseTransactions.Union(new[]
            {
                new Transaction(new HashCode(PublicAddress.Code.Substring(0, PublicAddress.Code.Length / 2)),
                    PublicAddress, BlockChainConfigurations.ActualDifficulty * 5, baseTransactions.Max(t => t.Id) + 1)
            }).ToList().ForEach(t => blockToMine.Transactions.Add(t));
            while (blockToMine.CalculateNewHash(BlockChainConfigurations.ActualDifficulty, PublicAddress).Code.Substring(0, BlockChainConfigurations.ActualDifficulty) != new string('0', BlockChainConfigurations.ActualDifficulty))
            {
                if (token.IsCancellationRequested)
                {
                    blockToMine = null;
                    break;
                }
            }
            IsMining = false;
            return blockToMine;
        }
        public async Task<BaseBlock> MineBlockAsync(IEnumerable<BaseTransaction> transactionsToMine, BaseBlock lastBlockInChain, CancellationTokenSource cancellationToken) =>
            await Task.Run(() => MineBlock(transactionsToMine, lastBlockInChain, cancellationToken.Token), cancellationToken.Token);
    }
}