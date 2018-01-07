using StepCoin;
using StepCoin.BlockChainClasses;
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
            RandomTest();
        }

        private static void RandomTest()
        {
            Random r = new Random(8080);
            List<Node> nodeList = new List<Node>();
            nodeList.Add(new Node("node 0"));
            for (int i = 0; i < r.Next(10, 20); i++)
            {
                nodeList.Add(new Node("node " + (i + 1)));
            }

            //Подписка на изменения BlockChain и PendingConfirmElements
            foreach (var node in nodeList.Where(n => n.Observer.Nodes.Count <= 0))
            {
                for (int i = 0; i < r.Next(nodeList.Count); i++)
                {
                    node.Observer.Attach(nodeList[r.Next(nodeList.Count)]);
                }

                //Console.WriteLine($"Attached to {node.Account.PublicAddress}:");
                //foreach (var item in node.Observer.Nodes)
                //{
                //    Console.WriteLine(item.Account.PublicAddress);
                //}
            }

            //Генерация транзакций
            int idx = 0;
            int count = 10;//кол. генерируемых транзакций
            while (idx++ < count)//Генерация count транзакций
            {
                Console.Write($"{idx} ");
                nodeList[r.Next(nodeList.Count)].GenerateNewTransaction(nodeList[r.Next(nodeList.Count)].Account.PublicAddress, r.Next(10, 50));
                //Thread.Sleep(100);
            }
            foreach (var item in nodeList)
            {
                item.NotifyAboutPendingElements();
            }
            Thread.Sleep(Configurations.TransactionConfirmationTime);//Ожидание распотранения транзакций

            for (int i = 0; i < nodeList.Count; i++)
            {
                foreach (var node in nodeList)
                {
                    //node.FoundAndAddConfirmedTransactions();
                    Console.WriteLine($"{node.Account.PublicAddress} pending transactions ({node.PendingConfirmElements.Count}):");
                    ShowElements(node.PendingConfirmElements.Where(pe => pe.Element is Transaction));
                }
            }
        }

        private static void ShowElements(IEnumerable<PendingConfirmChainElement> enumerable)
        {
            foreach (var item in enumerable)
            {
                Console.WriteLine($"{item.Element} Confirmations: {item.Confirmations.Where(c=>c.Value).Count()} Denials: {item.Confirmations.Where(c => !c.Value).Count()}");
            }
        }
    }
}
