using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.IntegrationTests
{
    // TODO: Correct this test.
    [TestFixture]
    public class TestSocketServer
    {
        public class CaseSource
        {
            public ServerConfig ServerConfig { get; set; }

            public ClientConfig ClientConfig { get; set; }

            public int ClientCount { get; set; }

            public int ContentSize { get; set; }

            public int SendTimes { get; set; }

            public CaseSource()
            {
                
            }

            public CaseSource(CaseSource other)
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


        #region Case sources.

        public IEnumerable<CaseSource> Cases()
        {
            const string address = "127.0.0.1";
            const int port = 50035;
            const int defaultBufferSize = 1024;
            const int defaultBacklog = 1000;
            const int defaultMaxConnectionCount = 1000;
            const int defaultMaxMessageSize = 520 * 1024 * 1024;
            const double timeout = 45.0;
            const SocketType socketType = SocketType.Stream;
            const ProtocolType protocolType = ProtocolType.Tcp;

            var packetProtocolSettings = new PacketProtocolSettings
            {
                Signature = new byte[] { 12, 34, 56, 78, 90 },
                MaxContentSize = 512 * 1024 * 1024,
            };

            var prototype = new CaseSource
            {
                ServerConfig = new ServerConfig
                {
                    Address = address,
                    Port = port,
                    BufferSize = defaultBufferSize,
                    Backlog = defaultBacklog,
                    MaxConnections = defaultMaxConnectionCount,
                    MaxMessageSize = defaultMaxMessageSize,
                    TimeOut = timeout,
                    TransferType = socketType,
                    UseProtocol = protocolType,
                    PacketProtocolSettings = packetProtocolSettings,
                },
                ClientConfig = new ClientConfig
                {
                    Address = address,
                    Port = port,
                    BufferSize = defaultBufferSize,
                    MaxMessageSize = defaultMaxMessageSize,
                    TransferType = socketType,
                    UseProtocol = protocolType,
                    PacketProtocolSettings = packetProtocolSettings,
                },
                ContentSize = 400,
            };

            // Begin client count = 1
            yield return CreateSendReceiveTestSource(prototype, 1, 1);
            yield return CreateSendReceiveTestSource(prototype, 1, 10);
            yield return CreateSendReceiveTestSource(prototype, 1, 100);

            // Begin client count = 16
            yield return CreateSendReceiveTestSource(prototype, 16, 1);
            yield return CreateSendReceiveTestSource(prototype, 16, 10);
            yield return CreateSendReceiveTestSource(prototype, 16, 100);

            // Begin client count = 128
            yield return CreateSendReceiveTestSource(prototype, 128, 1);
            yield return CreateSendReceiveTestSource(prototype, 128, 10);
            yield return CreateSendReceiveTestSource(prototype, 128, 100);

            // Begin client count = 512
            yield return CreateSendReceiveTestSource(prototype, 512, 1);
            yield return CreateSendReceiveTestSource(prototype, 512, 10);
            yield return CreateSendReceiveTestSource(prototype, 512, 100);

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

        static CaseSource CreateSendReceiveTestSource(CaseSource prototype, int clientCount, int sendTimes)
        {
            const int contentSize = 512;
            var caseSource = new CaseSource(prototype)
            {
                ClientCount = clientCount,
                ContentSize = contentSize,
                SendTimes = sendTimes
            };
            return caseSource;
        }

        static CaseSource CreateBandwidthTestSource(CaseSource prototype, int contentSize, int sendTimes)
        {
            var caseSource = new CaseSource(prototype)
            {
                ClientCount = 1,
                ContentSize = contentSize,
                SendTimes = sendTimes
            };
            return caseSource;
        }

        #endregion


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
        }

        // Timeout is related with environment.
        [TestCaseSource("Cases")]
        [Timeout(90000)]
        public void TestSendReceive(CaseSource caseSource)
        {
            Debug.Print("[INFO] ClientCount={0}, ConentSize={1}, SendTimes={2}", caseSource.ClientCount, caseSource.ContentSize, caseSource.SendTimes);

            var guidHash = new HashSet<Guid>();
            var receivedByteCount = 0;
            var connectedCount = 0;
            var incomingMessageListener = new SocketServerIncomingMessageListener((connection, isConnected) =>
            {
                if (isConnected)
                {
                    ++connectedCount;
                    Debug.Print("[CONNECT] {0} is connected. Conncetion count={1}.", connection.DestAddress, connectedCount);
                }
                else
                {
                    --connectedCount;
                    Debug.Print("[CONNECT] {0} has been disconnected. Conncetion count={1}.", connection.DestAddress, connectedCount);
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

                    Debug.Print("[RCVD] GUID={0}, received byte count={1}",
                        guid,
                        receivedByteCount);
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
            var maxPacketProcessCount = (200 / sentClients.Count / caseSource.SendTimes) + 1;

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
    }
}
