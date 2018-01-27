using System.Collections.Generic;

namespace StepCoin.BaseClasses
{
    public interface IBlockChain
    {
        IEnumerable<IBlock> Blocks { get; }
        IEnumerable<ITransaction> TransactionsOnBlocks { get; }
    }
}
