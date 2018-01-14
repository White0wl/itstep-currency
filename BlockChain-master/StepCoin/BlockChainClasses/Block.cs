using StepCoin.Hash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StepCoin.BlockChainClasses
{
    public class Block : ChainElememnt
    {
        public HashCode PrevHash { get; private set; }
        public int Difficulty { get; private set; }
        public int Nonce { get; private set; }
        public IEnumerable<Transaction> Transactions { get => _transactions.Select(t => t.GetClone() as Transaction); internal set => _transactions = value.ToList(); }

        public override ChainElememnt GetClone()
        {
            return new Block(PrevHash, Id) { Hash = Hash, Nonce = Nonce, _transactions = Transactions.ToList(), Difficulty = Difficulty, _miner = _miner?.Clone, _timestamp = _timestamp };
        }

        private List<Transaction> _transactions = new List<Transaction>();
        private HashCode _miner;//адрес эккаунта майнера, на который переведется вознаграждение   
        private DateTime _timestamp;//Время получения хэша
        public DateTime Timestamp { get => _timestamp; }
        public Block(HashCode prevHash, int id)
        {
            PrevHash = prevHash.Clone;
            Id = id;
        }

        internal Block(HashCode prevHash, HashCode hash, int id) : this(prevHash, id)
        {
            Hash = hash.Clone;
        }

        public override HashCode CalculateHash() => new HashCode(HashGenerator.GenerateString(SHA256.Create(),
                Encoding.Unicode.GetBytes($"{Id}{PrevHash}{String.Join(string.Empty, Transactions.Select(t => t.Hash))}{Nonce}")));

        internal HashCode CalculateNewHash(int difficulty, HashCode miner)
        {
            Difficulty = difficulty;
            Nonce++;
            _timestamp = DateTime.Now;
            _miner = miner.Clone;
            Hash = CalculateHash();
            return Hash;
        }

        public override string ToString()
        {
            return $"Id : {Id}\r\n" +
                $"Prev Hash : {PrevHash}\r\n" +
                $"This Hash : {Hash}\r\n" +
                $"Date : {_timestamp}\r\n" +
                $"Transactions : \r\n" +
                $"{String.Join("\r\n", Transactions.Select(t => t.Hash))}";
        }
    }


}