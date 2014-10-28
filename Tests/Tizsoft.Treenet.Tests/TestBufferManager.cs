using System;
using System.Net.Sockets;
using NUnit.Framework;

namespace Tizsoft.Treenet.Tests
{
    // TODO: Reduce test took time.
    [TestFixture]
    public class TestBufferManager
    {
        const int KiloBytes = 1024;
        const int MegaBytes = 1024 * KiloBytes;

        [TearDown]
        public void Teardown()
        {
            GC.Collect();
        }

        [TestCase(-1, -1)]
        [TestCase(0, 0)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        public void TestConstructorOutOfRangeArgs(int bufferCount, int bufferSize)
        {
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                var bufferManager = new BufferManager();
                bufferManager.InitBuffer(bufferCount, bufferSize);
            });
        }

        [TestCase(int.MaxValue, 2)]
        [TestCase(2, int.MaxValue)]
        [TestCase(int.MaxValue, int.MaxValue)]
        public void TestConstructorOverflowArgs(int bufferCount, int bufferSize)
        {
            Assert.Catch<OverflowException>(() =>
            {
                var bufferManager = new BufferManager();
                bufferManager.InitBuffer(bufferCount, bufferSize);
            });
        }

        [TestCase(500 * MegaBytes, 1)]
        [TestCase(1, 1500 * MegaBytes)]
        [TestCase(256, MegaBytes)]
        [TestCase(64, KiloBytes)]
        [TestCase(1, 1)]
        public void TestAllocate(int bufferCount, int bufferSize)
        {
            Assert.DoesNotThrow(() =>
            {
                var bufferManager = new BufferManager();
                bufferManager.InitBuffer(bufferCount, bufferSize);
            });
        }

        [TestCase(MegaBytes, 1)]
        [TestCase(1, 1500 * MegaBytes)]
        [TestCase(256, MegaBytes)]
        [TestCase(64, KiloBytes)]
        public void TestSetBuffer(int bufferCount, int bufferSize)
        {
            var bufferManager = new BufferManager();
            var socketOperations = new SocketAsyncEventArgs[bufferCount];

            bufferManager.InitBuffer(bufferCount, bufferSize);

            for (var i = 0; i != bufferCount; ++i)
            {
                var socketOperation = new SocketAsyncEventArgs();
                var isSetBufferSuccess = bufferManager.SetBuffer(socketOperation);
                Assert.IsTrue(isSetBufferSuccess);
                socketOperations[i] = socketOperation;
            }

            for (var i = 0; i != socketOperations.Length; ++i)
            {
                var socketOperation = socketOperations[i];
                Assert.AreEqual(i * bufferSize, socketOperation.Offset);
                Assert.AreEqual(bufferSize, socketOperation.Count);
            }
        }
    }
}
