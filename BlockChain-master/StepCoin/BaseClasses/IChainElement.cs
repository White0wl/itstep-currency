using StepCoin.Hash;

namespace StepCoin.BaseClasses
{
    public interface IChainElement
    {
        int Id { get; }
        HashCode Hash { get; }

        IChainElement Clone();
        HashCode CalculateHash();
    }
}
