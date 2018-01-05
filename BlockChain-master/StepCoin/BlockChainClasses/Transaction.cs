using System;
using System.Text;
using System.Security.Cryptography;
using StepCoin.Hash;

namespace StepCoin.BlockChainClasses
{
    public class Transaction : ChainElememnt
    {

        //Не подходит, если требуется клонирование объекта
        //static int count = 0;//статический счетчик, на котором основывается присвоение Id
        public HashCode Sender { get; }//адрес отправителя
        public HashCode Recipient { get; } //адрес получателя
        public decimal Amount { get; } //сумма
        public DateTime Timestamp { get; private set; } //время создания
        public override HashCode Hash => new HashCode(HashGenerator.GenerateString(MD5.Create(), Encoding.Unicode.GetBytes($"{Sender}{Recipient}{Amount}{Timestamp}")));
        public override ChainElememnt Clone => new Transaction(Sender.Clone, Recipient.Clone, Amount, Id) { Timestamp = Timestamp };

        public Transaction(HashCode sender, HashCode resepient, decimal amount, int id)
        {
            Id = id;
            Sender = sender.Clone;
            Recipient = resepient.Clone;
            Amount = amount;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp}]{Sender}=>{Recipient}({Amount.ToString()})";
        }
    }
}
