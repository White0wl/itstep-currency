using StepCoin.Hash;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepCoin.BlockChainClasses
{
    public class PendingConfirmChainElement
    {
        public ChainElememnt Element { get; private set; }
        //Содержит адресс подтвердителя и его результат
        public Dictionary<HashCode, bool> Confirmations { get; set; } = new Dictionary<HashCode, bool>();
        public DateTime PendingStartTime { get; private set; }

        public PendingConfirmChainElement(ChainElememnt elememnt) { PendingStartTime = DateTime.Now; Element = elememnt.Clone; }

        public PendingConfirmChainElement Clone => new PendingConfirmChainElement(Element.Clone)
        {
            Confirmations = Confirmations.ToDictionary(k => k.Key.Clone, v => v.Value),
            PendingStartTime = PendingStartTime
        };
    }
}
