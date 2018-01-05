using StepCoin.Hash;

namespace StepCoin.BlockChainClasses
{
    public abstract class ChainElememnt
    {
        public int Id { get; protected set; }
        public abstract HashCode Hash { get; }
        public abstract ChainElememnt Clone { get; }
    }
}
