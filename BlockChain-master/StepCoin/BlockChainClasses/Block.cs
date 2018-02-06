using StepCoin.BaseClasses;
using StepCoin.Hash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace StepCoin.BlockChainClasses
{
    [DataContract]
    public class Block : BaseBlock
    {

        [DataMember]
        public int Difficulty { get; private set; }
        [DataMember]
        public int Nonce { get; private set; }

        [DataMember]
        private HashCode _miner;//адрес эккаунта майнера, на который переведется вознаграждение   
        [DataMember]
        private DateTime _timestamp;//Время получения хэша        
        
        public Block(HashCode prevHash, int id)
        {
            
            PrevHash = prevHash.Clone();
            Id = id;
            Transactions = new List<BaseTransaction>();
        }

        public override HashCode CalculateHash() => new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.AlgorithmBlockHash,
                Encoding.Unicode.GetBytes($"{Id}{PrevHash}{String.Join(string.Empty, Transactions.Select(t => t.Hash))}{Nonce}")));

        internal HashCode CalculateNewHash(int difficulty, HashCode miner)
        {
            Difficulty = difficulty;
            Nonce++;
            _timestamp = DateTime.Now;
            _miner = miner.Clone();
            Hash = CalculateHash();
            DateOfReceiving = DateTime.Now;
            return Hash;
        }

        public override string ToString()
        {
            return $"Id : {Id}\r\n" +
                $"Prev Hash : {PrevHash}\r\n" +
                $"This Hash : {Hash}\r\n" +
                $"Date : {_timestamp}\r\n" +
                $"Transactions : \r\n" +
                $"{String.Join("\r\n", Transactions.Select(t => t.ToString()))}";
        }

        public override BaseChainElement Clone() => new Block(PrevHash, Id) { Hash = Hash, Nonce = Nonce, Transactions = Transactions.Select(t => t.Clone() as BaseTransaction).ToList(), Difficulty = Difficulty, _miner = _miner?.Clone(), _timestamp = _timestamp };
    }


}