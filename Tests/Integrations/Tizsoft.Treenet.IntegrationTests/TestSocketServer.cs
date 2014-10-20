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
                    Header = other.ServerConfig.Header,
                    TimeOut = other.ServerConfig.TimeOut,
                    TransferType = other.ServerConfig.TransferType,
                    UseProtocol = other.ServerConfig.UseProtocol
                };

                ClientConfig = new ClientConfig
                {
                    Address = other.ClientConfig.Address,
                    Port = other.ClientConfig.Port,
                    BufferSize = other.ClientConfig.BufferSize,
                    TransferType = other.ClientConfig.TransferType,
                    UseProtocol = other.ClientConfig.UseProtocol
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

        public static IEnumerable<CaseSource> Cases()
        {
            const string address = "127.0.0.1";
            const int port = 50035;
            const double timeout = 3.0;
            const SocketType socketType = SocketType.Stream;
            const ProtocolType protocolType = ProtocolType.Tcp;

            var caseSource = new CaseSource
            {
                ServerConfig = new ServerConfig
                {
                    Address = address,
                    Port = port,
                    BufferSize = 512,
                    Backlog = 1000,
                    MaxConnections = 1000,
                    Header = new byte[0],
                    TimeOut = timeout,
                    TransferType = socketType,
                    UseProtocol = protocolType,
                },
                ClientConfig = new ClientConfig
                {
                    Address = address,
                    Port = port,
                    BufferSize = 512,
                    TransferType = socketType,
                    UseProtocol = protocolType,
                },
                ContentSize = 400,
            };

            // Begin client count = 1
            yield return new CaseSource(caseSource)
            {
                ClientCount = 1,
                ContentSize = 400,
                SendTimes = 1
            };

            yield return new CaseSource(caseSource)
            {
                ClientCount = 1,
                ContentSize = 400,
                SendTimes = 10
            };

            yield return new CaseSource(caseSource)
            {
                ClientCount = 1,
                ContentSize = 400,
                SendTimes = 100
            };

            // Begin client count = 10
            yield return new CaseSource(caseSource)
            {
                ClientCount = 10,
                ContentSize = 400,
                SendTimes = 1
            };

            yield return new CaseSource(caseSource)
            {
                ClientCount = 10,
                ContentSize = 400,
                SendTimes = 10
            };

            yield return new CaseSource(caseSource)
            {
                ClientCount = 10,
                ContentSize = 400,
                SendTimes = 100
            };

            // Begin client count = 100
            yield return new CaseSource(caseSource)
            {
                ClientCount = 100,
                ContentSize = 400,
                SendTimes = 1
            };

            yield return new CaseSource(caseSource)
            {
                ClientCount = 100,
                ContentSize = 400,
                SendTimes = 10
            };

            yield return new CaseSource(caseSource)
            {
                ClientCount = 100,
                ContentSize = 400,
                SendTimes = 100
            };

            // Begin client count = 1000
            yield return new CaseSource(caseSource)
            {
                ClientCount = 1000,
                ContentSize = 400,
                SendTimes = 1
            };

            yield return new CaseSource(caseSource)
            {
                ClientCount = 1000,
                ContentSize = 400,
                SendTimes = 10
            };

            yield return new CaseSource(caseSource)
            {
                ClientCount = 1000,
                ContentSize = 400,
                SendTimes = 100
            };
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

            var receivedByteCount = 0;
            var incomingMessageCount = 0;
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
                receivedByteCount += packet.Content.Length;
                incomingMessageCount++;
                Debug.Print("[RCVD] Message serial={0}, Message size={1}, received byte count={2}", incomingMessageCount, packet.Content.Length, receivedByteCount);
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
            var sendedClients = new HashSet<SocketClient>();
            var sendedCount = 0;

            while (sendedCount < maxConnection)
            {
                foreach (var client in _clients)
                {
                    if (client.IsConnected)
                    {
                        if (sendedClients.Add(client))
                        {
                            for (var i = 0; i != caseSource.SendTimes; ++i)
                            {
                                client.Send(sendContent, PacketType.Stream);
                            }
                            ++sendedCount;
                        }
                    }
                }
            }

            const int serverUpdateTime = 65;
            var expectedBytesCount = caseSource.ContentSize * sendedClients.Count * caseSource.SendTimes;
            var maxPacketProcessCount = (200 / sendedClients.Count / caseSource.SendTimes) + 1;

            Debug.Print("[INFO] Sended client count={0}, Expected byte count={1}", sendedClients.Count, expectedBytesCount);

            while (true)
            {
                int remaingSleepTime;
                _server.Update(serverUpdateTime, out remaingSleepTime);

                Thread.Sleep(1 + serverUpdateTime - remaingSleepTime);

                foreach (var client in sendedClients)
                {
                    client.Update(maxPacketProcessCount);
                }

                Thread.Sleep(5);

                if (receivedByteCount >= expectedBytesCount)
                {
                    break;
                }

                Assert.Greater(connectedCount, 0);
            }
        }
    }
}
