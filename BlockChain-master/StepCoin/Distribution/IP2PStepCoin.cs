using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
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
    }
}
