using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.User;

namespace StepCoin.Distribution
{
    public delegate void GetBlock(BaseBlock block);
    public delegate void GetPendingElement(PendingConfirmChainElement element);
    public interface IDistribution
    {
        HashCode ClientCode { get; set; }
        //Уведомить подписчиков об элементе
        void NotifyAboutPendingElement(PendingConfirmChainElement element);
        //Уведомить подписчиков о добавленном блоке
        void NotifyAboutBlock(BaseBlock newBlock);

        //Получение блока
        event GetBlock BlockNotification;
        //Получение ожидающего подтверждения элементе
        event GetPendingElement PendingElementNotification;
    }
}