using StepCoin;
using StepCoin.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //RandomTest();

           
        }

        private static void RandomTest()
        {
            Random r = new Random();
            List<Node> nodeList = new List<Node>();
            nodeList.Add(new Node("node -1"));
            for (int i = 0; i < r.Next(10, 20); i++)
            {
                nodeList.Add(new Node("node " + i, nodeList[r.Next(nodeList.Count)]));
            }

            while (true)
            {
                nodeList[r.Next(nodeList.Count)].GenerateNewTransaction();

                if (TransactionsValidator.PendingTransactions.Count >= 5)
                {
                    ThreadPool.QueueUserWorkItem(((state) =>
                    {
                        nodeList[r.Next(nodeList.Count)].StartMineAsync();
                    }), null);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
