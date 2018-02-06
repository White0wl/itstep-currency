using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using System.Runtime.Serialization;

namespace StepCoin.BaseClasses
{
    [DataContract]
    [KnownType(typeof(Transaction))]
    public abstract class BaseTransaction : BaseChainElement
    {
        [DataMember]
        public HashCode Sender { get; protected set; }//адрес отправителя
        [DataMember]
        public HashCode Recipient { get; protected set; } //адрес получателя
        [DataMember]
        public decimal Amount { get; protected set; }
    }
}
