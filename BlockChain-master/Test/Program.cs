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
            List<Node> nodeList = GenerateNodes(11);

            //Подписка на изменения BlockChain и PendingConfirmElements
            foreach (var node in nodeList)
            {
                foreach (var item in nodeList.Where(n => n != node))
                {
                    node.Observer.Subscribe(item);
                    //Console.WriteLine($"{item.Account.PublicAddress} subscribe to {node.Account.PublicAddress}");
                }
            }

            //Генерация транзакций
            int idx = 0;
            int count = 10;//кол. генерируемых транзакций
            while (idx++ < count)//Генерация count транзакций
            {
                Console.Write($"{idx} ");
                nodeList[r.Next(nodeList.Count)].GenerateNewTransaction(nodeList[r.Next(nodeList.Count)].Account.PublicAddress, r.Next(10, 50));
            }

            Thread.Sleep(Configurations.TransactionConfirmationTime);//Ожидание распотранения транзакций

            foreach (var node in nodeList)
            {
                //node.FoundAndAddConfirmedTransactions();
                Console.WriteLine($"{node.Account.PublicAddress} pending transactions ({node.PendingConfirmElements.Count()}):");
                ShowElements(node.PendingConfirmElements.Where(pe => pe.Element is Transaction));
            }
        }

        private static List<Node> GenerateNodes(int count)
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node("node 0"));
            if (count > 1)
                for (int i = 0; i < count - 1; i++)
                {
                    nodes.Add(new Node("node " + (i + 1)));
                }
            return nodes;
        }

        private static void ShowElements(IEnumerable<PendingConfirmChainElement> enumerable)
        {
            foreach (var item in enumerable)
            {
                Console.WriteLine($"{item.Element.Hash} Confirmations: {item.Confirmations.Where(c => c.Value).Count()} Denials: {item.Confirmations.Where(c => !c.Value).Count()}");
            }
        }
    }
}
