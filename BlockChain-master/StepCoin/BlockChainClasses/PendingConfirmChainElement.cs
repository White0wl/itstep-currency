using StepCoin.BaseClasses;
using StepCoin.Hash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace StepCoin.BlockChainClasses
{
    [DataContract]
    public class PendingConfirmChainElement
    {
        [DataMember]
        public BaseChainElement Element { get; private set; }
        //Содержит адресс подтвердителя и его результат
        [DataMember]
        public Dictionary<HashCode, KeyValuePair<bool, string>> Confirmations { get; private set; } = new Dictionary<HashCode, KeyValuePair<bool, string>>();
        [DataMember]
        public DateTime PendingStartTime { get; private set; }

        public PendingConfirmChainElement(BaseChainElement elememnt) { PendingStartTime = DateTime.Now; Element = elememnt.Clone(); }

        public PendingConfirmChainElement Clone() => new PendingConfirmChainElement(Element.Clone())
        {
            Confirmations = Confirmations.ToDictionary(k => k.Key.Clone(), v => v.Value),
            PendingStartTime = PendingStartTime
        };

        public int CountConfirm => Confirmations.Count(c => c.Value.Key);
    }
}
