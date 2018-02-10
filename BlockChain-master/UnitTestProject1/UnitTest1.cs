using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepCoin;
using StepCoin.Hash;
using StepCoin.User;
using StepCoin.Validators;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var h1 = new HashCode("aaa");
            var h2 = new HashCode("aaa");
            Assert.IsTrue(h1 == h2);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var h1 = new HashCode("aaa");
            var h2 = new HashCode("aaa");
            Assert.IsFalse(h1 != h2);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var h1 = new HashCode("aaa");
            HashCode h2 = null;
            Assert.IsFalse(h1 == h2);
        }

        [TestMethod]
        public void TestMethod4()
        {
            HashCode h1 = null;
            var h2 = new HashCode("aaa");
            Assert.IsFalse(h1 == h2);
        }

        [TestMethod]
        public void TestMethod5()
        {
            var h1 = new HashCode("aaa");
            var h2 = new HashCode("aasa");
            Assert.IsFalse(h1 == h2);
        }




        [TestMethod]
        public void TestMethod6()
        {
            var accounts = new ObservableCollection<HashCode> { new HashCode("a1"), new HashCode("a2"), new HashCode("a3") };
            var hash = new HashCode("b1");
            Assert.IsFalse(accounts.Contains(hash));
        }
        [TestMethod]
        public void TestMethod7()
        {
            var accounts = new ObservableCollection<HashCode> { new HashCode("a1"), new HashCode("a2"), new HashCode("a3") };
            var hash = new HashCode("a1");
            Assert.IsTrue(accounts.Contains(hash));
        }


        [TestMethod]
        public void TestMethod8()
        {
            Assert.IsFalse(BlockValidator.IsCanBeAddedToChain(null, null).Key);
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMethodSavingAccountsWithException()
        {
            var hash = new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.AlgorithmPublicHashAccout, Encoding.Unicode.GetBytes("name")));
            AccountList.AddAccountKey(hash);
            ;
        }

        [TestMethod]
        public void TestMethodSavingAccounts()
        {
            var hash = new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.AlgorithmPublicHashAccout, Encoding.Unicode.GetBytes("name" + AccountList.Accounts.Count())));
            AccountList.AddAccountKey(hash);
            ;
        }
    }
}
