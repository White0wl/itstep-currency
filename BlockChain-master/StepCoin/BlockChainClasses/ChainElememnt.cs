using StepCoin.Hash;

namespace StepCoin.BlockChainClasses
{
    public abstract class ChainElememnt
    {
        public int Id { get; protected set; }
        public HashCode Hash { get; protected set; }
        public abstract ChainElememnt Clone { get; }


        public abstract HashCode CalculateHash();
    }
}
