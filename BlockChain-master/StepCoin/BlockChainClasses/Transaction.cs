using System;
using System.Text;
using StepCoin.Hash;
using StepCoin.BaseClasses;

namespace StepCoin.BlockChainClasses
{
    public class Transaction : ITransaction
    {
        public int Id { get; set; }
        public HashCode Sender { get; }//адрес отправителя
        public HashCode Recipient { get; } //адрес получателя

        public decimal Amount { get; } //сумма
        public DateTime Timestamp { get; private set; } //время создания
        public HashCode Hash { get; set; }

        public IChainElement Clone()
        {
            return new Transaction(Sender.Clone(), Recipient.Clone(), Amount, Id)
            {
                Timestamp = Timestamp,
                Hash = CalculateHash()
            };
        }

        public HashCode CalculateHash() => new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.AlgorithmTransactionHash, Encoding.Unicode.GetBytes($"{Id}{Sender}{Recipient}{Amount}{Timestamp}")));

        public Transaction(HashCode sender, HashCode recipient, decimal amount, int id)
        {
            Id = id;
            Sender = sender.Clone();
            Recipient = recipient.Clone();
            Amount = amount;
            Timestamp = DateTime.Now;
            Hash = CalculateHash();
        }

        public override string ToString() => $"[{Timestamp}]{Sender}=>{Recipient}({Amount.ToString()})";
    }
}
