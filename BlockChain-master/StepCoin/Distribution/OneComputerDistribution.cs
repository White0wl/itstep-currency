using System.Collections.Generic;
using System.Linq;
using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.User;

namespace StepCoin.Distribution
{
    //Observer
    public class OneComputerDistribution : IDistribution
    {
        public event GetBlock BlockNotification;
        public event GetPendingElement PendingElementNotification;
        private readonly List<Node> _subscrubers = new List<Node>();

        public void Subscribe(Node subscriber)
        {
            if (subscriber is null) return;
            if (_subscrubers.FirstOrDefault(s => s.Account.PublicAddress == subscriber.Account.PublicAddress) is null) _subscrubers.Add(subscriber);
        }
        public void Describe(Node subscriber)
        {
            if (subscriber is null) return;
            _subscrubers.Remove(_subscrubers.FirstOrDefault(s => s.Account.PublicAddress == subscriber.Account.PublicAddress));
        }

        public void NotifyAboutPendingElement(PendingConfirmChainElement element) => 
            _subscrubers.ForEach(s => s.NotificationPendingElement(element));

        public void NotifyAboutBlock(BaseBlock newBlock) =>
            _subscrubers.ForEach(s => s.NotificationNewBlock(newBlock));
    }
}
