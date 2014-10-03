using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Tizsoft.Collections;
using Tizsoft.Collections.Concurrent;

namespace Tizsoft.Tests.Collections.Concurrent
{
    [TestFixture]
    public class TestConcurrentPool
    {
        IPool<object> _pool;

        [SetUp]
        public void Setup()
        {
            _pool = new ConcurrentPool<object>(CreateObject);
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
            _pool = new ConcurrentPool<object>(null);
            Assert.Fail("Should throw an System.ArgumentNullException.");
        }

        [TestCase(10000000)]
        [TestCase(10000)]
        [TestCase(10)]
        [TestCase(1)]
        public void TestAcquireRecycle(int acquireAmount)
        {
            TestRecycle(_pool, acquireAmount);

            Assert.AreEqual(1, _pool.Count);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        [TestCase(10000000)]
        [TestCase(10000)]
        [TestCase(10)]
        [TestCase(1)]
        public void TestAcquireAllocation(int acquireAmount)
        {
            var objects = new BlockingCollection<IPoolObject<object>>();

            TestAllocation(_pool, acquireAmount, objects);
            ReleaseAllocations(objects);

            Assert.LessOrEqual(_pool.Count, acquireAmount);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        [TestCase(1250000, 8)]
        [TestCase(2500000, 4)]
        [TestCase(10000000, 1)]
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
                tasks[ti] = Task.Run(() => TestRecycle(_pool, acquireAmount));
            }

            Task.WaitAll(tasks);

            Assert.LessOrEqual(_pool.Count, acquireAmount * taskCount);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        [TestCase(1250000, 8)]
        [TestCase(2500000, 4)]
        [TestCase(10000000, 1)]
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
            var acquiredObjects = new BlockingCollection<IPoolObject<object>>();
            var tasks = new Task[taskCount];

            for (var ti = 0; ti != taskCount; ++ti)
            {
                var task = Task.Run(() => TestAllocation(_pool, acquireAmount, acquiredObjects));

                tasks[ti] = task;
            }

            Task.WaitAll(tasks);
            acquiredObjects.CompleteAdding();

            Assert.AreEqual(totalAmount, acquiredObjects.Count);

            ReleaseAllocations(acquiredObjects);

            Assert.AreEqual(totalAmount, _pool.Count);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        [TestCase(10000000)]
        [TestCase(10000)]
        [TestCase(10)]
        [TestCase(1)]
        public void TestAcquireRecycleParallel(int acquireAmount)
        {
            var parallel = Parallel.For(0, acquireAmount, (state, i) =>
            {
                using (_pool.Acquire())
                {
                    // Do nothing.
                }
            });

            WaitParallelComplete(parallel);

            Assert.LessOrEqual(_pool.Count, acquireAmount);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        [TestCase(10000000)]
        [TestCase(10000)]
        [TestCase(10)]
        [TestCase(1)]
        public void TestAcquireAllocationParallel(int acquireAmount)
        {
            var acquiredObjects = new BlockingCollection<IPoolObject<object>>();
            var parallel = Parallel.For(0, acquireAmount, (state, i) => acquiredObjects.Add(_pool.Acquire()));

            WaitParallelComplete(parallel);
            
            acquiredObjects.CompleteAdding();

            Assert.AreEqual(acquireAmount, acquiredObjects.Count);

            ReleaseAllocations(acquiredObjects);

            Assert.AreEqual(acquireAmount, _pool.Count);
            CheckPoolHasDuplicatedObjects(_pool);
        }

        static void TestAllocation(IPool<object> pool, int acquireAmount, BlockingCollection<IPoolObject<object>> objects)
        {
            for (var i = 0; i != acquireAmount; ++i)
            {
                objects.Add(pool.Acquire());
            }
        }

        static void ReleaseAllocations(IEnumerable<IPoolObject<object>> objects)
        {
            foreach (var obj in objects)
            {
                obj.Dispose();
            }
        }

        static void TestRecycle(IPool<object> pool, int acquireAmount)
        {
            for (var i = 0; i != acquireAmount; ++i)
            {
                using (pool.Acquire())
                {
                    // Do nothing.
                }
            }
        }

        static void CheckPoolHasDuplicatedObjects(IPool<object> pool)
        {
            var checker = new HashSet<object>();
            var poolCount = pool.Count;

            for (var i = 0; i != poolCount; ++i)
            {
                var obj = pool.Acquire();
                var o = obj.Value;

                if (checker.Contains(o))
                {
                    Assert.Fail("An object was recycled by two or more times.");
                }

                checker.Add(o);
            }
        }

        static void WaitParallelComplete(ParallelLoopResult parallel)
        {
            var spin = new SpinWait();

            while (true)
            {
                if (parallel.IsCompleted)
                {
                    break;
                }

                spin.SpinOnce();
            }
        }
    }
}
