using NUnit.Framework;
using System;
using System.Net.Sockets;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.Tests
{
    [TestFixture]
    public class TestAsyncSocketListener
    {
        static readonly ServerConfig Case1 = new ServerConfig
        {
            Address = "127.0.0.1",
            Port = 50035,
            BufferSize = 512,
            Backlog = 10,
            MaxConnections = 10,
        };

        AsyncSocketListener _socketListener;
        FixedSizeObjPool<IConnection> _connectionPool;
        BufferManager _receiveBufferManager;
        BufferManager _sendBufferManager;
        IPacketContainer _packetContainer;
        PacketHandler _packetHandler;
        PacketSender _packetSender;
        
        [TearDown]
        public void Teardown()
        {
            if (_socketListener != null)
            {
                _socketListener.Stop();
            }

            _socketListener = null;

            if (_connectionPool != null)
            {
                while (_connectionPool.Count > 0)
                {
                    var connection = _connectionPool.Pop();

                    if (connection != null)
                    {
                        connection.Dispose();
                    }
                }
            }

            _receiveBufferManager = null;
            _sendBufferManager = null;
        }

        [Test]
        public void TestSetup()
        {
            var serverConfig = new ServerConfig
            {
                Address = "localhost",
                Port = 50053,
                BufferSize = 512,
                Backlog = 10,
                MaxConnections = 10,
                TimeOut = 5,
                TransferType = SocketType.Stream,
                UseProtocol = ProtocolType.Tcp
            };
            //var connectionPool = new FixedSizeObjPool<IConnection>(serverConfig.MaxConnections);
            
            Assert.Throws<ArgumentNullException>(() =>
            {
                var socketListener = new AsyncSocketListener();
                socketListener.Setup(null, null);
                socketListener.Stop();
            });

            //Assert.Throws<ArgumentNullException>(() =>
            //{
            //    var socketListener = new AsyncSocketListener();
            //    socketListener.Setup(null, connectionPool);
            //    socketListener.Stop();
            //});

            Assert.Throws<ArgumentNullException>(() =>
            {
                var socketListener = new AsyncSocketListener();
                socketListener.Setup(serverConfig, null);
                socketListener.Stop();
            });

            //Assert.Catch<Exception>(() =>
            //{
            //    var invalidConfig = new ServerConfig
            //    {
            //        Address = string.Empty,
            //        Port = -1,
            //        Backlog = -1,
            //        BufferSize = -1,
            //        MaxConnections = 0
            //    };

            //    var socketListener = new AsyncSocketListener();
            //    socketListener.Setup(invalidConfig, connectionPool);
            //    socketListener.Stop();
            //});

            //Assert.DoesNotThrow(() =>
            //{
            //    var socketListener = new AsyncSocketListener();
            //    socketListener.Setup(serverConfig, connectionPool);
            //    socketListener.Stop();
            //});

            //Assert.DoesNotThrow(() =>
            //{
            //    var socketListener = new AsyncSocketListener();
            //    socketListener.Setup(serverConfig, connectionPool);
            //    socketListener.Setup(serverConfig, connectionPool);
            //    socketListener.Stop();
            //});

            //Assert.DoesNotThrow(() =>
            //{
            //    var socketListener = new AsyncSocketListener();

            //    socketListener.Setup(serverConfig, connectionPool);
            //    socketListener.Stop();

            //    socketListener.Setup(serverConfig, connectionPool);
            //    socketListener.Stop();
            //});
        }

        // TODO: Test other operations.
    }
}
