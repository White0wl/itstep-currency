using StepCoin.Hash;

namespace StepCoin.BaseClasses
{
    public interface ITransaction : IChainElement
    {
        HashCode Sender { get; }//адрес отправителя
        HashCode Recipient { get; } //адрес получателя
        decimal Amount { get; }
    }
}
