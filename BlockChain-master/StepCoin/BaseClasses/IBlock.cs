using StepCoin.Hash;
using System.Collections.Generic;
using System;

namespace StepCoin.BaseClasses
{
    public interface IBlock : IChainElement
    {
        HashCode PrevHash { get; }
        IList<ITransaction> Transactions { get;}
        DateTime DateOfReceiving { get;}
    }
}
