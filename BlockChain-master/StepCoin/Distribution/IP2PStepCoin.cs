using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using System.ServiceModel;

namespace StepCoin.Distribution
{
    /// <summary>
    /// Интерфейс для получения данных по дуплексной связи
    /// </summary>
    [ServiceContract]
    public interface IP2PStepCoin
    {
        [OperationContract(IsOneWay = true)]
        void ObtainBlock(BaseBlock block);
        [OperationContract(IsOneWay = true)]
        void ObtainPendingElement(PendingConfirmChainElement element);
        [OperationContract(IsOneWay = true)]
        void ObtainAccount(BaseAccount account);

        [OperationContract(IsOneWay = true)]
        void RequestAccountsFromProxy();
        [OperationContract(IsOneWay = true)]
        void RequestBlocksFromProxy();
        [OperationContract(IsOneWay = true)]
        void RequestPendingElementsFromProxy();
    }
}
