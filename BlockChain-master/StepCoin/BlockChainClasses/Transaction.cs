using System;
using System.Globalization;
using System.Text;
using StepCoin.Hash;
using StepCoin.BaseClasses;
using System.Runtime.Serialization;

namespace StepCoin.BlockChainClasses
{
    [DataContract]
    public class Transaction : BaseTransaction
    {
        [DataMember]
        public DateTime Timestamp { get; private set; } //время создания

        public override BaseChainElement Clone() => new Transaction(Sender.Clone(), Recipient.Clone(), Amount, Id)
        {
            Timestamp = Timestamp,
            Hash = CalculateHash()
        };

        public sealed override HashCode CalculateHash() => new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.AlgorithmTransactionHash, Encoding.Unicode.GetBytes($"{Id}{Sender}{Recipient}{Amount}{Timestamp}")));

        public Transaction(HashCode sender, HashCode recipient, decimal amount, int id)
        {
            Id = id;
            Sender = sender.Clone();
            Recipient = recipient.Clone();
            Amount = amount;
            Timestamp = DateTime.Now;
            Hash = CalculateHash();
        }

        public override string ToString() => $"[{Timestamp}]{Sender}=>{Recipient}({Amount.ToString(CultureInfo.InvariantCulture)})";
    }
}
