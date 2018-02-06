using StepCoin.Hash;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using StepCoin.BlockChainClasses;

namespace StepCoin.BaseClasses
{
    [DataContract]
    [KnownType(typeof(Block))]
    public abstract class BaseBlock : BaseChainElement
    {
        [DataMember]
        public HashCode PrevHash { get; protected set; }
        [DataMember]
        public IList<BaseTransaction> Transactions { get; protected set; }
        [DataMember]
        public DateTime DateOfReceiving { get; protected set; }
    }
}
