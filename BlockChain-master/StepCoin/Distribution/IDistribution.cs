using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.User;

namespace StepCoin.Distribution
{
    public delegate void TakeBlock(BaseBlock block);
    public delegate void TakePendingElement(PendingConfirmChainElement element);
    public delegate void RequestBlocks();
    public delegate void RequestPendingElements();
    public interface IDistribution
    {
        HashCode Client { get; }
        //Уведомить подписчиков об элементе
        void NotifyAboutPendingElement(PendingConfirmChainElement element);
        //Уведомить подписчиков о добавленном блоке
        void NotifyAboutBlock(BaseBlock block);

        //Получение блока
        event TakeBlock BlockNotification;
        //Получение ожидающего подтверждения элемента
        event TakePendingElement PendingElementNotification;
        //Запрос у подписчика(ов) о надобности получения блоков
        event RequestBlocks RequestBlocks;
        //Запрос у подписчика(ов) о надобности получения ожидающих подтверждения элементов
        event RequestPendingElements RequestPendingElements;
    }
}