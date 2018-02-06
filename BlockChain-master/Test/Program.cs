﻿using StepCoin;
using StepCoin.BlockChainClasses;
using StepCoin.Distribution;
using StepCoin.User;
using StepCoin.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RandomTest();
        }

        private static void RandomTest()
        {
            var r = new Random(8080);
            var nodeList = GenerateNodes(5);

            //Генерация транзакций
            var idx = 0;
            const int count = 20; //кол. генерируемых транзакций
            while (idx++ < count)//Генерация count транзакций
            {
                Console.Write($"{idx} ");
                nodeList[r.Next(nodeList.Count)].GenerateNewTransaction(nodeList[r.Next(nodeList.Count)].Account.PublicAddress, r.Next(10, 50));
            }

            Thread.Sleep(BlockChainConfigurations.TransactionConfirmationTime);//Ожидание распотранения транзакций

            //foreach (var node in nodeList)
            //{
            //    //node.FoundAndAddConfirmedTransactions();
            //    Console.WriteLine($"{node.Account.PublicAddress} pending transactions ({node.PendingConfirmElements.Where(pe=>pe.Element is Transaction).Count()}):");
            //    ShowElements(node.PendingConfirmElements.Where(pe => pe.Element is Transaction));
            //}


            foreach (var node in nodeList)
            {
                //node.FoundAndAddConfirmedTransactions();
                Console.WriteLine($"{node.Account.PublicAddress} pending transactions ({node.PendingConfirmElements.Count(pe => pe.Element is Transaction)}):");
                ShowElements(node.PendingConfirmElements.Where(pe => pe.Element is Transaction));
            }
            //Console.ReadLine();

            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach (var item in nodeList)
            {
                item.FoundAndAddConfirmedTransactions();
                Console.WriteLine($"{item.Account.PublicAddress} can mine: {item.IsCanMine}");
                tasks.Add(new Task<bool>(() => { item.StartMine(); return true; }));
            }

            foreach (var task in tasks)
            {
                task.Start();
                Thread.Sleep(100);
            }

            while (tasks.Count(t => t.IsCompleted) != tasks.Count()) { }

            foreach (var node in nodeList)
            {
                //node.FoundAndAddConfirmedTransactions();
                Console.WriteLine($"{node.Account.PublicAddress} pending block ({node.PendingConfirmElements.Count(pe => pe.Element is Block)}):");
                ShowElements(node.PendingConfirmElements.Where(pe => pe.Element is Block));
            }

            foreach (var node in nodeList)
            {
                Console.WriteLine($"Block chain {node.Account.PublicAddress}");
                var transactions = node.BlockChain.TransactionsOnBlocks;

                decimal amountRecieved = TransactionValidator.ReceivedTransactions(node.Account.PublicAddress, transactions).Sum(t => t.Amount);
                decimal amountSent = TransactionValidator.SentTransactions(node.Account.PublicAddress, transactions).Sum(t => t.Amount);
                Console.WriteLine($"Balance : {amountRecieved - amountSent + BlockChainConfigurations.StartBalance}$");
                //foreach (var block in node.BlockChain.Blocks)
                //{
                //    Console.WriteLine(block);
                //}
                Console.WriteLine();
            }
        }

        private static List<Node> GenerateNodes(int count)
        {
            var nodes = new List<Node> {new Node("node 0", "pass0")};
            if (count > 1)
                for (var i = 0; i < count - 1; i++)
                {
                    nodes.Add(new Node($"node {(i + 1)}",$"pass{(i + 1)}"));
                }


            //Подписка на изменения BlockChain и PendingConfirmElements
            foreach (var node in nodes)
            {
                foreach (var item in nodes.Where(n => n != node))
                {
                    (node.Distribution as OneComputerDistribution)?.Subscribe(item);
                    //Console.WriteLine($"{item.Account.PublicAddress} subscribe to {node.Account.PublicAddress}");
                }
            }
            return nodes;
        }

        private static void ShowElements(IEnumerable<PendingConfirmChainElement> enumerable)
        {
            foreach (var item in enumerable)
            {
                Console.WriteLine($"{item.Element.Hash} Confirmations: {item.Confirmations.Count(c => c.Value)} Denials: {item.Confirmations.Count(c => !c.Value)}");
            }
        }
    }
}
