using StepCoin.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
using System.Text;
using System.Threading.Tasks;

namespace DistributionLibrary
{
    public class PeerEntry
    {
        public Node Node { get; set; }
        public PeerName PeerName { get; set; }
        public IP2PService ServiceProxy { get; set; }
    }
}
