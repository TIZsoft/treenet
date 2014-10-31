using System;
using System.Diagnostics;
using Tizsoft.Collections;
using Tizsoft.Security.Cryptography;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.IntegrationTests
{
    class SocketServer : IConnectionSubject, IDisposable
    {
        static readonly ICryptoProvider CryptoProvider = new XorCryptoProvider(Network.DefaultXorKey);

        readonly BufferManager _receiveBufferManager = new BufferManager();
        readonly BufferManager _sendBufferManager = new BufferManager();
        readonly AsyncSocketListener _socketListener = new AsyncSocketListener();
        readonly IPacketContainer _packetContainer = new PacketContainer();
        readonly PacketHandler _packetHandler = new PacketHandler();
        readonly PacketSender _packetSender = new PacketSender();

        bool _isDisposed;
        readonly Stopwatch _stopwatch = new Stopwatch();

        public SocketServer(ServerConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var packetProtocol = new PacketProtocol(config.PacketProtocolSettings);
            
            _receiveBufferManager.InitBuffer(config.MaxConnections, config.BufferSize);
            _sendBufferManager.InitBuffer(config.MaxConnections, config.BufferSize);

            _packetContainer.Setup(CryptoProvider);
            _packetSender.Setup(_sendBufferManager, config.MaxConnections, CryptoProvider);
            _packetSender.PacketProtocol = packetProtocol;
            
            var connectionPool = new FixedSizeObjPool<IConnection>(config.MaxConnections);

            for (var i = 0; i < config.MaxConnections; ++i)
            {
                var connection = new Connection(_receiveBufferManager, _packetContainer, _packetSender, config.MaxMessageSize);
                connection.PacketProtocol = packetProtocol;
                connection.Register(_socketListener);
                connectionPool.Push(connection);
            }
            
            _socketListener.Setup(config, connectionPool);
        }

        ~SocketServer()
        {
            Dispose(false);
        }

        public void AddParser(PacketType packetType, IPacketProcessor packetProcessor)
        {
            _packetHandler.AddParser(packetType, packetProcessor);
        }

        public void Start()
        {
            _socketListener.Start();
        }

        public void Update(int maxCpuMiliseconds, out int sleepTime)
        {
            _stopwatch.Restart();
            while (_stopwatch.ElapsedMilliseconds < maxCpuMiliseconds)
            {
                var packet = _packetContainer.NextPacket();

                if (packet != Packet.Null)
                {
                    _packetHandler.Parse(packet);
                }
                else
                {
                    break;
                }
            }

            _stopwatch.Stop();
            sleepTime = (int)(maxCpuMiliseconds - _stopwatch.ElapsedMilliseconds);

            if (sleepTime > maxCpuMiliseconds)
            {
                sleepTime = maxCpuMiliseconds;
            }

            if (sleepTime < 1)
            {
                sleepTime = 1;
            }
        }

        public void Stop()
        {
            if (_isDisposed)
            {
                return;
            }

            _socketListener.Stop();
            _packetContainer.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }

        public void Register(IConnectionObserver observer)
        {
            _socketListener.Register(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            _socketListener.Unregister(observer);
        }

        public void Notify(IConnection connection, bool isConnected)
        {
            _socketListener.Notify(connection, isConnected);
        }
    }
}
