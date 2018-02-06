using StepCoin.Hash;
using System.Runtime.Serialization;

namespace StepCoin.BaseClasses
{
    [DataContract]
    [KnownType(typeof(BaseBlock))]
    [KnownType(typeof(BaseTransaction))]
    public abstract class BaseChainElement
    {
        [DataMember]
        public int Id { get; protected set; }
        [DataMember]
        public HashCode Hash { get; internal set; }
        public abstract BaseChainElement Clone();
        public abstract HashCode CalculateHash();
    }
}
