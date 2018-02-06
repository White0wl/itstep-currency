using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepCoin.Hash;
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
            var accounts = new ObservableCollection<HashCode>{new HashCode("a1"),new HashCode("a2"),new HashCode("a3")};
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
            Assert.IsFalse(BlockValidator.IsCanBeAddedToChain(null, null));
        }
    }
}
