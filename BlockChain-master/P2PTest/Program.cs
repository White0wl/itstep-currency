using StepCoin.Distribution;
using StepCoin.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Node node = new Node("24-03", new PTPDistribution("p2pDemo"));
            node.GenerateNewTransaction(new StepCoin.Hash.HashCode("3123123"), 10);
        }
    }
}
