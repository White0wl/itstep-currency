using System.Collections.Generic;

namespace StepCoin.BaseClasses
{
    public interface IBlockChain
    {
        IEnumerable<BaseBlock> Blocks { get; }
        IEnumerable<BaseTransaction> TransactionsOnBlocks { get; }
    }
}
