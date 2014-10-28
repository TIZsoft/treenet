using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Tizsoft.Treenet.Tests
{
    [TestFixture]
    public class TestMessageFraming
    {
        [TearDown]
        public void TearDown()
        {
            GC.Collect();
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void TestInvalidConstructorArgs(int maxMessageSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MessageFraming(maxMessageSize));
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(1024)]
        [TestCase(4 * 1024 * 1024)]
        [TestCase(1536 * 1024 * 1024)]
        public void TestValidConstructorArgs(int maxMessageSize)
        {
            Assert.DoesNotThrow(() => new MessageFraming(maxMessageSize));
        }

        [Test]
        public void TestInvalidWrapMessage()
        {
            Assert.Catch<ArgumentException>(() => MessageFraming.WrapMessage(null));
        }

        public IEnumerable<byte[]> TestValidWrapMessageCaseSources()
        {
            yield return GenerateRandomBytes(0);
            yield return GenerateRandomBytes(10);
            yield return GenerateRandomBytes(1024);
            yield return GenerateRandomBytes(4 * 1024 * 1024);
            yield return GenerateRandomBytes(512 * 1024 * 1024);
        }

        static byte[] GenerateRandomBytes(int count)
        {
            var rand = new Random();
            var bytes = new byte[count];
            rand.NextBytes(bytes);
            return bytes;
        }

        [TestCaseSource("TestValidWrapMessageCaseSources")]
        public void TestValidWrapMessage(byte[] message)
        {
            Assert.DoesNotThrow(() =>
            {
                var messageLength = message.Length;
                var wrappedMessage = MessageFraming.WrapMessage(message);
                
                Assert.AreEqual(messageLength + sizeof(int), wrappedMessage.Length);

                // Test wrapped message.
                var length = BitConverter.ToInt32(wrappedMessage, 0);
                Assert.AreEqual(messageLength, length);
                var testFailed = false;
                for (var i = 0; i != length; ++i)
                {
                    if (message[i] != wrappedMessage[i + sizeof(int)])
                    {
                        testFailed = true;
                        break;
                    }
                }
                Assert.IsFalse(testFailed);
            });
        }

        class MockReceiver
        {
            readonly int _bufferSize;
            
            public event Action<byte[]> Completed;

            public MockReceiver(int bufferSize)
            {
                _bufferSize = bufferSize;
            }

            public void StartFixedReceive(string txFile)
            {
                using (var fileStream = File.OpenRead(txFile))
                using (var reader = new BinaryReader(fileStream))
                {
                    var sw = Stopwatch.StartNew();
                    var spin = new SpinWait();
                    var count = fileStream.Length / _bufferSize;
                    var remaining = (int)(fileStream.Length - count * _bufferSize);
                    byte[] block;

                    for (var i = 0; i != count; ++i)
                    {
                        block = reader.ReadBytes(_bufferSize);
                        OnCompleted(block);

                        if (sw.ElapsedMilliseconds >= 125)
                        {
                            spin.SpinOnce();
                            sw.Restart();
                        }
                    }

                    if (remaining > 0)
                    {
                        block = reader.ReadBytes(remaining);
                        OnCompleted(block);
                    }
                }
            }

            public void StartReceive(string txFile)
            {
                using (var fileStream = File.OpenRead(txFile))
                using (var reader = new BinaryReader(fileStream))
                {
                    var sw = Stopwatch.StartNew();
                    var spin = new SpinWait();
                    var random = new Random();

                    while (fileStream.Length > fileStream.Position)
                    {
                        var readCount = random.Next(0, _bufferSize);
                        var block = reader.ReadBytes(readCount);
                        OnCompleted(block);

                        if (sw.ElapsedMilliseconds >= 125)
                        {
                            spin.SpinOnce();
                            sw.Restart();
                        }
                    }
                }
            }

            void OnCompleted(byte[] reveivedData)
            {
                if (Completed != null)
                {
                    Completed(reveivedData);
                }
            }
        }

        public class DataReceivedCaseSource
        {
            public int ReceiverBufferSize { get; set; }

            public int MfMaxMessageSize { get; set; }

            public int MinMessageCount { get; set; }

            public int MaxMessageCount { get; set; }

            public int MinMessageSize { get; set; }

            public int MaxMessageSize { get; set; }

            public List<byte[]> GenerateRandomWrappedMessages()
            {
                return TestMessageFraming.GenerateRandomWrappedMessages(MinMessageCount, MaxMessageCount, MinMessageSize, MaxMessageSize);
            }
        }

        // TODO: Remove duplicated code.
        public IEnumerable<DataReceivedCaseSource> TestTestDataReceiveCaseSources()
        {
            const int maxMessageSize = 4 * 1024 * 1024;
            const int receiveBufferSize = 512;

            const int lowMessageCount       = 1;
            const int middleMessageCount    = 1024;
            const int highMessageCount      = 1048576;

            const int messageSizeCase1 = 508;
            const int messageSizeCase2 = 248;
            const int messageSizeCase3 = 700;
            const int messageSizeCase4 = 511;

            const int minMessageSizeFinalCase = 0;
            const int maxMessageSizeFinalCase = 4 * 1024 * 1024;


            #region Case 1: Normal, fit message size.

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = lowMessageCount,
                MaxMessageCount = lowMessageCount,
                MinMessageSize = messageSizeCase1,
                MaxMessageSize = messageSizeCase1,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = middleMessageCount,
                MaxMessageCount = middleMessageCount,
                MinMessageSize = messageSizeCase1,
                MaxMessageSize = messageSizeCase1,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = highMessageCount,
                MaxMessageCount = highMessageCount,
                MinMessageSize = messageSizeCase1,
                MaxMessageSize = messageSizeCase1,
            };
            
            #endregion


            #region Case 2: Merged messages.

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = lowMessageCount,
                MaxMessageCount = lowMessageCount,
                MinMessageSize = messageSizeCase2,
                MaxMessageSize = messageSizeCase2,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = middleMessageCount,
                MaxMessageCount = middleMessageCount,
                MinMessageSize = messageSizeCase2,
                MaxMessageSize = messageSizeCase2,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = highMessageCount,
                MaxMessageCount = highMessageCount,
                MinMessageSize = messageSizeCase2,
                MaxMessageSize = messageSizeCase2,
            };

            #endregion


            #region Case 3: Splited body.

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = lowMessageCount,
                MaxMessageCount = lowMessageCount,
                MinMessageSize = messageSizeCase3,
                MaxMessageSize = messageSizeCase3,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = middleMessageCount,
                MaxMessageCount = middleMessageCount,
                MinMessageSize = messageSizeCase3,
                MaxMessageSize = messageSizeCase3,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = highMessageCount,
                MaxMessageCount = highMessageCount,
                MinMessageSize = messageSizeCase3,
                MaxMessageSize = messageSizeCase3,
            };

            #endregion


            #region Case 4: Splited length-prefix.

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = lowMessageCount,
                MaxMessageCount = lowMessageCount,
                MinMessageSize = messageSizeCase4,
                MaxMessageSize = messageSizeCase4,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = middleMessageCount,
                MaxMessageCount = middleMessageCount,
                MinMessageSize = messageSizeCase4,
                MaxMessageSize = messageSizeCase4,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = highMessageCount,
                MaxMessageCount = highMessageCount,
                MinMessageSize = messageSizeCase4,
                MaxMessageSize = messageSizeCase4,
            };

            #endregion


            #region Final Case: Simulates the realistic environment.

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = lowMessageCount,
                MaxMessageCount = lowMessageCount,
                MinMessageSize = minMessageSizeFinalCase,
                MaxMessageSize = maxMessageSizeFinalCase,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = middleMessageCount,
                MaxMessageCount = middleMessageCount,
                MinMessageSize = minMessageSizeFinalCase,
                MaxMessageSize = maxMessageSizeFinalCase,
            };

            yield return new DataReceivedCaseSource
            {
                ReceiverBufferSize = receiveBufferSize,
                MfMaxMessageSize = maxMessageSize,
                MinMessageCount = highMessageCount,
                MaxMessageCount = highMessageCount,
                MinMessageSize = minMessageSizeFinalCase,
                MaxMessageSize = maxMessageSizeFinalCase,
            };

            #endregion
        }

        public static List<byte[]> GenerateRandomWrappedMessages(int minMessageCount, int maxMessageCount, int minMessageSize, int maxMessageSize)
        {
            var random = new Random();
            var messageCount = random.Next(minMessageCount, maxMessageCount);
            var wrappedMessages = new List<byte[]>(messageCount);
            var totalBytes = 0;

            for (var i = 0; i != messageCount; ++i)
            {
                var messageLength = random.Next(minMessageSize, maxMessageSize);
                totalBytes += messageLength;

                var message = new byte[messageLength];
                random.NextBytes(message);

                var wrappedMessage = MessageFraming.WrapMessage(message);
                wrappedMessages.Add(wrappedMessage);

                // 1 GB
                if (totalBytes >= int.MaxValue / 2)
                {
                    break;
                }
            }

            return wrappedMessages;
        }
        
        // Precondition: WrapMessage must be correct.
        // Timeout is dependent on device.
        [TestCaseSource("TestTestDataReceiveCaseSources")]
        [Timeout(120000)]
        public void TestDataReceive(DataReceivedCaseSource caseSource)
        {
            Debug.WriteLine("BufferSize={0}, MfMaxMessageSize={1}, MessageCount=[{2}, {3}], MessageSize=[{4}, {5}]",
                caseSource.ReceiverBufferSize,
                caseSource.MfMaxMessageSize,
                caseSource.MinMessageCount,
                caseSource.MaxMessageCount,
                caseSource.MinMessageSize,
                caseSource.MaxMessageSize
            );

            const string txFileName = "tx.bin";

            var messageIndex = 0;
            var wrappedMessages = caseSource.GenerateRandomWrappedMessages();
            var messageCount = wrappedMessages.Count;
            var messageFraming = new MessageFraming(caseSource.MaxMessageSize);

            var txPos = 0L;
            messageFraming.MessageArrived += (sender, args) =>
            {
                // Compare TX RX
                using (var txFile = new FileStream(txFileName, FileMode.Open, FileAccess.Read))
                using (var txReader = new BinaryReader(txFile))
                {
                    txPos = txReader.BaseStream.Seek(txPos, SeekOrigin.Begin);
                    var messageLength = txReader.ReadInt32();
                    txPos += sizeof(int);

                    Assert.AreEqual(messageLength, args.Message.Length);

                    txPos = txReader.BaseStream.Seek(txPos, SeekOrigin.Begin);
                    var txMessage = txReader.ReadBytes(messageLength);
                    txPos += messageLength;

                    for (var i = 0; i != messageLength; ++i)
                    {
                        if (txMessage[i] != args.Message[i])
                        {
                            Assert.Fail();
                        }
                    }
                }

                ++messageIndex;
            };

            if (File.Exists(txFileName))
            {
                File.Delete(txFileName);
            }

            using (var fileStream = File.Create(txFileName))
            {
                foreach (var wrappedMessage in wrappedMessages)
                {
                    fileStream.Write(wrappedMessage, 0, wrappedMessage.Length);
                }
            }

            var mockReceiver = new MockReceiver(caseSource.ReceiverBufferSize);
            mockReceiver.Completed += messageFraming.DataReceived;
            mockReceiver.StartFixedReceive(txFileName);
            
            var spin = new SpinWait();
            while (true)
            {
                if (messageIndex >= messageCount)
                {
                    break;
                }

                spin.SpinOnce();
            }
        }
    }
}
