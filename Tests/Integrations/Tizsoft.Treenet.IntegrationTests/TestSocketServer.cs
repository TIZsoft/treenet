using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.IntegrationTests
{
    [TestFixture]
    public class TestSocketServer
    {
        const string Address = "127.0.0.1";
        const int Port = 50035;
        const int DefaultBufferSize = 1024;
        const int DefaultMaxConnectionCount = 1000;
        const int DefaultMaxMessageSize = 520 * 1024 * 1024;

        static readonly PacketProtocolSettings DefaultPacketProtocolSettings = new PacketProtocolSettings
        {
            Signature = new byte[] { 12, 34, 56, 78, 90 },
            MaxContentSize = 512 * 1024 * 1024,
        };

        static readonly ServerConfig DefaultServerConfig = new ServerConfig
        {
            Address = Address,
            Port = Port,
            Backlog = 100,
            BufferSize = DefaultBufferSize,
            MaxConnections = DefaultMaxConnectionCount,
            MaxMessageSize = DefaultMaxMessageSize,
            PacketProtocolSettings = DefaultPacketProtocolSettings,
        };

        static readonly ClientConfig DefaultClientConfig = new ClientConfig
        {
            Address = Address,
            Port = Port,
            BufferSize = DefaultBufferSize,
            MaxMessageSize = DefaultMaxMessageSize,
            PacketProtocolSettings = DefaultPacketProtocolSettings,
        };

        class PacketProcessor : IPacketProcessor
        {
            readonly Action<IPacket> _processAction;

            public PacketProcessor(Action<IPacket> processAction)
            {
                _processAction = processAction;
            }

            public void Process(IPacket packet)
            {
                _processAction(packet);
            }
        }

        class SocketServerIncomingMessageListener : IConnectionObserver
        {
            readonly Action<IConnection, bool> _connectionEvent;

            public SocketServerIncomingMessageListener(Action<IConnection, bool> connectionEvent)
            {
                _connectionEvent = connectionEvent;
            }

            public void GetConnectionEvent(IConnection connection, bool isConnected)
            {
                _connectionEvent(connection, isConnected);
            }
        }
        
        SocketServer _server;
        List<SocketClient> _clients;
        
        [TearDown]
        public void Teardown()
        {
            if (_clients != null &&
                _clients.Count > 0)
            {
                foreach (var client in _clients)
                {
                    if (client != null)
                    {
                        client.Stop();
                    }
                }

                _clients.Clear();
                _clients = null;
            }

            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }

            if (_clients != null &&
                _clients.Count != 0)
            {
                var spin = new SpinWait();
                foreach (var c in _clients)
                {
                    if (c.IsConnected)
                    {
                        spin.SpinOnce();
                    }
                }

                _clients.Clear();
                _clients = null;
            }
        }


        #region Test send, receive.

        public class SendReceiveCaseSource
        {
            public ServerConfig ServerConfig { get; set; }

            public ClientConfig ClientConfig { get; set; }

            public int ClientCount { get; set; }

            public int ContentSize { get; set; }

            public int SendTimes { get; set; }

            public SendReceiveCaseSource()
            {

            }

            public SendReceiveCaseSource(SendReceiveCaseSource other)
            {
                ServerConfig = new ServerConfig
                {
                    Address = other.ServerConfig.Address,
                    Port = other.ServerConfig.Port,
                    BufferSize = other.ServerConfig.BufferSize,
                    Backlog = other.ServerConfig.Backlog,
                    MaxConnections = other.ServerConfig.MaxConnections,
                    MaxMessageSize = other.ServerConfig.MaxMessageSize,
                    TimeOut = other.ServerConfig.TimeOut,
                    TransferType = other.ServerConfig.TransferType,
                    UseProtocol = other.ServerConfig.UseProtocol,
                    PacketProtocolSettings = other.ServerConfig.PacketProtocolSettings
                };

                ClientConfig = new ClientConfig
                {
                    Address = other.ClientConfig.Address,
                    Port = other.ClientConfig.Port,
                    BufferSize = other.ClientConfig.BufferSize,
                    MaxMessageSize = other.ClientConfig.MaxMessageSize,
                    TransferType = other.ClientConfig.TransferType,
                    UseProtocol = other.ClientConfig.UseProtocol,
                    PacketProtocolSettings = other.ClientConfig.PacketProtocolSettings
                };

                ClientCount = other.ClientCount;
                ContentSize = other.ContentSize;
                SendTimes = other.SendTimes;
            }
        }

        public IEnumerable<SendReceiveCaseSource> TestSendReceiveCaseSources()
        {
            var prototype = new SendReceiveCaseSource
            {
                ServerConfig = DefaultServerConfig,
                ClientConfig = DefaultClientConfig,
                ContentSize = 400,
            };

            const int singleClientCount = 1;
            const int lowClientCount = 16;
            const int middleClientCount = 128;
            const int highClientCount = 512;

            const int lowContentSize = 512;
            const int middleContentSize = 2048;
            const int highContentSize = 8192;

            const int lowSendCount = 1;
            const int middleSendCount = 10;
            const int highSendCount = 100;

            // Begin client count = 1
            yield return CreateSendReceiveTestSource(prototype, singleClientCount,    lowContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, singleClientCount, middleContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, singleClientCount,   highContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, singleClientCount,    lowContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, singleClientCount, middleContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, singleClientCount,   highContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, singleClientCount,    lowContentSize,   highSendCount);
            yield return CreateSendReceiveTestSource(prototype, singleClientCount, middleContentSize,   highSendCount);
            yield return CreateSendReceiveTestSource(prototype, singleClientCount,   highContentSize,   highSendCount);

            // Begin client count = 16
            yield return CreateSendReceiveTestSource(prototype, lowClientCount,    lowContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, lowClientCount, middleContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, lowClientCount,   highContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, lowClientCount,    lowContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, lowClientCount, middleContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, lowClientCount,   highContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, lowClientCount,    lowContentSize,   highSendCount);
            yield return CreateSendReceiveTestSource(prototype, lowClientCount, middleContentSize,   highSendCount);
            yield return CreateSendReceiveTestSource(prototype, lowClientCount,   highContentSize,   highSendCount);

            // Begin client count = 128
            yield return CreateSendReceiveTestSource(prototype, middleClientCount,    lowContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, middleClientCount, middleContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, middleClientCount,   highContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, middleClientCount,    lowContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, middleClientCount, middleContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, middleClientCount,   highContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, middleClientCount,    lowContentSize,   highSendCount);
            yield return CreateSendReceiveTestSource(prototype, middleClientCount, middleContentSize,   highSendCount);
            yield return CreateSendReceiveTestSource(prototype, middleClientCount,   highContentSize,   highSendCount);

            // Begin client count = 512
            yield return CreateSendReceiveTestSource(prototype, highClientCount,    lowContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, highClientCount, middleContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, highClientCount,   highContentSize,    lowSendCount);
            yield return CreateSendReceiveTestSource(prototype, highClientCount,    lowContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, highClientCount, middleContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, highClientCount,   highContentSize, middleSendCount);
            yield return CreateSendReceiveTestSource(prototype, highClientCount,    lowContentSize,   highSendCount);
            yield return CreateSendReceiveTestSource(prototype, highClientCount, middleContentSize,   highSendCount);
            yield return CreateSendReceiveTestSource(prototype, highClientCount,   highContentSize,   highSendCount);

            // Begin bandwidth test
            const int megabytes = 1024 * 1024;

            // 64MB
            yield return CreateBandwidthTestSource(prototype, 64 * megabytes, 1);

            // 128MB
            yield return CreateBandwidthTestSource(prototype, 128 * megabytes, 1);

            // 256MB
            yield return CreateBandwidthTestSource(prototype, 256 * megabytes, 1);

            // 512MB
            yield return CreateBandwidthTestSource(prototype, 512 * megabytes, 1);
        }

        static SendReceiveCaseSource CreateSendReceiveTestSource(SendReceiveCaseSource prototype, int clientCount, int contentSize, int sendTimes)
        {
            var caseSource = new SendReceiveCaseSource(prototype)
            {
                ClientCount = clientCount,
                ContentSize = contentSize,
                SendTimes = sendTimes
            };
            return caseSource;
        }

        static SendReceiveCaseSource CreateBandwidthTestSource(SendReceiveCaseSource prototype, int contentSize, int sendTimes)
        {
            var caseSource = new SendReceiveCaseSource(prototype)
            {
                ClientCount = 1,
                ContentSize = contentSize,
                SendTimes = sendTimes
            };
            return caseSource;
        }

        // Timeout is related with environment.
        [TestCaseSource("TestSendReceiveCaseSources")]
        [Timeout(90000)]
        public void TestSendReceive(SendReceiveCaseSource caseSource)
        {
            Debug.Print("[INFO] ClientCount={0}, ContentSize={1}, SendTimes={2}", caseSource.ClientCount, caseSource.ContentSize, caseSource.SendTimes);

            var guidHash = new HashSet<Guid>();
            var receivedByteCount = 0;
            var connectedCount = 0;
            var incomingMessageListener = new SocketServerIncomingMessageListener((connection, isConnected) =>
            {
                if (isConnected)
                {
                    ++connectedCount;
                    Debug.Print("[CONNECT] {0} is connected. Connection count={1}.", connection.DestAddress, connectedCount);
                }
                else
                {
                    --connectedCount;
                    Debug.Print("[CONNECT] {0} has been disconnected. Connection count={1}.", connection.DestAddress, connectedCount);
                }
            });

            _server = new SocketServer(caseSource.ServerConfig);
            _server.Register(incomingMessageListener);
            _server.AddParser(PacketType.Stream, new PacketProcessor(packet =>
            {
                var content = packet.Content;
                
                var guidBytes = new byte[16];
                Array.Copy(content, 0, guidBytes, 0, guidBytes.Length);
                var guid = new Guid(guidBytes);

                if (guidHash.Remove(guid))
                {
                    receivedByteCount += content.Length;

                    Debug.Print("[RCVD] GUID={0}, received byte count={1}", guid, receivedByteCount);
                }
                else
                {
                    Assert.Fail("GUID not matched.");
                }
            }));
            _server.Start();

            _clients = new List<SocketClient>(caseSource.ClientCount);

            for (var i = 0; i != caseSource.ClientCount; ++i)
            {
                var client = new SocketClient(caseSource.ClientConfig);
                _clients.Add(client);
            }

            foreach (var client in _clients)
            {
                client.Start();
            }

            var sendContent = new byte[caseSource.ContentSize];
            var maxConnection = Math.Min(caseSource.ClientCount, caseSource.ServerConfig.MaxConnections);
            var sentClients = new HashSet<SocketClient>();
            var sendedCount = 0;
            
            while (sendedCount < maxConnection)
            {
                foreach (var client in _clients)
                {
                    if (client.IsConnected)
                    {
                        if (sentClients.Add(client))
                        {
                            for (var i = 0; i != caseSource.SendTimes; ++i)
                            {
                                var guid = Guid.NewGuid();
                                guidHash.Add(guid);

                                var guidBytes = guid.ToByteArray();
                                Array.Copy(guidBytes, 0, sendContent, 0, guidBytes.Length);
                                client.Send(sendContent, PacketType.Stream);
                            }
                            ++sendedCount;
                        }
                    }
                }
            }

            const int serverUpdateTime = 65;
            var expectedBytesCount = caseSource.ContentSize * sentClients.Count * caseSource.SendTimes;
            var maxPacketProcessCount = (2000/sentClients.Count + 200/caseSource.SendTimes) + 1;

            Debug.Print("[INFO] Sent client count={0}, Expected byte count={1}", sentClients.Count, expectedBytesCount);

            while (true)
            {
                int remaingSleepTime;
                _server.Update(serverUpdateTime, out remaingSleepTime);

                Thread.Sleep(1 + serverUpdateTime - remaingSleepTime);

                foreach (var client in sentClients)
                {
                    client.Update(maxPacketProcessCount);
                }

                Thread.Sleep(5);

                if (receivedByteCount >= expectedBytesCount)
                {
                    break;
                }

                if (caseSource.ClientCount > 1)
                {
                    Assert.Greater(connectedCount, 0);
                }
            }

            if (guidHash.Count != 0)
            {
                Assert.Fail("Packets lost.");
            }
        }

        #endregion


        #region Test connect, disconnect.

        [Test]
        public void TestConnectDisconnect()
        {
            _server = new SocketServer(DefaultServerConfig);
            _server.Start();

            var connectService = new ConnectService();
            connectService.Setup(DefaultClientConfig);

            for (var i = 0; i != 200; ++i)
            {
                connectService.Start();
                connectService.Stop();
            }

            Thread.Sleep(1000);
        }

        #endregion
    }
}
