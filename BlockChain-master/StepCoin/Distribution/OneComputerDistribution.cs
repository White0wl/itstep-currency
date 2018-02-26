using System.Collections.Generic;
using System.Linq;
using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using StepCoin.User;

namespace StepCoin.Distribution
{
    //Observer
    public class OneComputerDistribution : IDistribution
    {
        public event TakeBlock BlockNotification;
        public event TakePendingElement PendingElementNotification;

        public event RequestBlocks RequestBlocks;
        public event RequestPendingElements RequestPendingElements;

        private readonly List<Node> _subscrubers = new List<Node>();

        public OneComputerDistribution(HashCode client) => Client = client;

        public HashCode Client { get; }

        public void Subscribe(Node subscriber)
        {
            if (subscriber is null) return;
            if (_subscrubers.FirstOrDefault(s => s.Account.PublicCode == subscriber.Account.PublicCode) != null) return;
            _subscrubers.Add(subscriber);
            subscriber.Distribution.BlockNotification += Distribution_BlockNotification;
            subscriber.Distribution.PendingElementNotification += Distribution_PendingElementNotification;
        }

        private void Distribution_PendingElementNotification(PendingConfirmChainElement element) => PendingElementNotification?.Invoke(element);
        private void Distribution_BlockNotification(BaseBlock block) => BlockNotification?.Invoke(block);


        public void Describe(Node subscriber)
        {
            if (subscriber is null) return;
            if (!_subscrubers.Contains(subscriber)) return;
            subscriber.Distribution.BlockNotification -= Distribution_BlockNotification;
            subscriber.Distribution.PendingElementNotification -= Distribution_PendingElementNotification;
            _subscrubers.Remove(_subscrubers.FirstOrDefault(s => s.Account.PublicCode == subscriber.Account.PublicCode));
        }


        public void NotifyAboutPendingElement(PendingConfirmChainElement element) =>
            _subscrubers.ForEach(s => s.NotificationPendingElement(element));

        public void NotifyAboutBlock(BaseBlock newBlock) =>
            _subscrubers.ForEach(s => s.NotificationNewBlock(newBlock));
    }
}
