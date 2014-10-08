using System;
using NUnit.Framework;

namespace Tizsoft.Tests
{
    [TestFixture]
    public class TestTizId
    {
        TizId _id;

        [TearDown]
        public void Teardown()
        {
            _id = null;
        }

        [TestCase((uint)1)]
        [TestCase((uint)10000)]
        [TestCase((uint)10000000)]
        public void TestTizIdIncrement(uint v)
        {
            _id = new TizIdIncrement();
            for (var i = 0; i < v; i++)
            {
                _id.Next();
                var id = _id.Current();
                Assert.AreEqual(id, i + 1);
            }
        }

        [TestCase((uint)1)]
        [TestCase((uint)10000)]
        [TestCase((uint)10000000)]
        public void TestTizIdDecrease(uint v)
        {
            _id = new TizIdDecrease();
            for (var i = 0; i < v; i++)
            {
                _id.Next();
                var id = _id.Current();
                var answer = TizId.MaxId - i;
                Assert.AreEqual(id, answer);
            }
        }
    }
}
