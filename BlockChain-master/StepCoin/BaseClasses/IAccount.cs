using StepCoin.Hash;

namespace StepCoin.BaseClasses
{
    public interface IAccount
    {
        HashCode PublicAddress { get; }//Address
        HashCode Password { get; }//SecretKey
    }
}
