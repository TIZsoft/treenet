using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tizsoft.Collections;

namespace Tizsoft.Tests.Collections
{
    [TestFixture]
    public class TestPool
    {
        Pool<object> _pool;

        [SetUp]
        public void Setup()
        {
            _pool = new Pool<object>(CreateObject);
        }

        [TearDown]
        public void Teardown()
        {
            _pool = null;
            GC.Collect();
        }

        static object CreateObject()
        {
            return new object();
        }

        [Test]
        [Category("Exception")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorWithNullArgs()
        {
            _pool = new Pool<object>(null);
            Assert.Fail("Should throw an ArgumentNullException.");
        }

        [TestCase(10000000)]
        [TestCase(10000)]
        [TestCase(10)]
        [TestCase(1)]
        public void TestAcquireRecycle(int acquireAmount)
        {
            for (var i = 0; i != acquireAmount; ++i)
            {
                using (_pool.Acquire())
                {
                    // Do nothing.
                }
            }

            Assert.AreEqual(1, _pool.Count);
        }

        [TestCase(10000000)]
        [TestCase(10000)]
        [TestCase(10)]
        [TestCase(1)]
        public void TestAcquireAllocation(int acquireAmount)
        {
            var objects = new IPoolObject<object>[acquireAmount];
            
            for (var i = 0; i != acquireAmount; ++i)
            {
                objects[i] = _pool.Acquire();
            }

            foreach (var obj in objects)
            {
                obj.Dispose();
            }
            
            Assert.AreEqual(acquireAmount, _pool.Count);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        [TestCase(1000000, 8)]
        [TestCase(1000000, 4)]
        [TestCase(1000000, 1)]
        [TestCase(10000, 8)]
        [TestCase(10000, 4)]
        [TestCase(10000, 1)]
        [TestCase(10, 8)]
        [TestCase(10, 4)]
        [TestCase(10, 1)]
        [TestCase(1, 8)]
        [TestCase(1, 4)]
        [TestCase(1, 1)]
        public void TestAcquireRecycleThreading(int acquireAmount, int taskCount)
        {
            var tasks = new Task[taskCount];

            for (var ti = 0; ti != taskCount; ++ti)
            {
                var task = Task.Run(() =>
                {
                    for (var ai = 0; ai != acquireAmount; ++ai)
                    {
                        using (_pool.Acquire())
                        {
                            // Do nothing.
                        }
                    }
                });
                tasks[ti] = task;
            }

            Task.WaitAll(tasks);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        [TestCase(1000000, 8)]
        [TestCase(1000000, 4)]
        [TestCase(1000000, 1)]
        [TestCase(10000, 8)]
        [TestCase(10000, 4)]
        [TestCase(10000, 1)]
        [TestCase(10, 8)]
        [TestCase(10, 4)]
        [TestCase(10, 1)]
        [TestCase(1, 8)]
        [TestCase(1, 4)]
        [TestCase(1, 1)]
        public void TestAcquireAllocationThreading(int acquireAmount, int taskCount)
        {
            // Recycle objects will modifiy the pool collection.
            // If the implementation of pool is not thread-safe, then the following tests may fail.

            var totalAmount = acquireAmount * taskCount;
            var countingBoard = new ConcurrentDictionary<object, int>();
            var acquiredObjects = new ConcurrentBag<IDisposable>();
            var tasks = new Task[taskCount];
            
            for (var ti = 0; ti != taskCount; ++ti)
            {
                var task = Task.Run(() =>
                {
                    for (var ai = 0; ai != acquireAmount; ++ai)
                    {
                        var poolObject = _pool.Acquire();
                        var o = poolObject.Value;
                        int count;

                        if (countingBoard.TryGetValue(o, out count))
                        {
                            ++countingBoard[o];
                        }
                        else
                        {
                            countingBoard[o] = 1;
                        }

                        acquiredObjects.Add(poolObject);
                    }
                });

                tasks[ti] = task;
            }

            Task.WaitAll(tasks);

            Assert.AreEqual(totalAmount, acquiredObjects.Count);

            foreach (var counter in countingBoard)
            {
                if (counter.Value != 1)
                {
                    Assert.Fail("An object was recycled by two or more times.");
                }
            }

            foreach (var disposable in acquiredObjects)
            {
                disposable.Dispose();
            }

            Assert.AreEqual(totalAmount, _pool.Count);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        static void CheckPoolHasDuplicatedObjects(Pool<object> pool)
        {
            var checker = new HashSet<object>();
            var poolCount = pool.Count;

            for (var i = 0; i != poolCount; ++i)
            {
                var obj = pool.Acquire();

                if (checker.Contains(obj.Value))
                {
                    Assert.Fail("An object was recycled by two or more times.");
                }
                else
                {
                    checker.Add(obj.Value);
                }
            }
        }
    }
}
