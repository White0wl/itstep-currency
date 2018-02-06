using StepCoin.Distribution;
using StepCoin.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var distribution = new P2PDistribution("P2PDemo");
            var node = new Node("24-03", "pass", distribution);
            distribution.RegisterPeer();
            node.GenerateNewTransaction(new StepCoin.Hash.HashCode("3123123"), 10);
            Console.ReadKey();
        }
    }
}
