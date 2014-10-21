using System;
using System.Diagnostics;
using Tizsoft.Collections;
using Tizsoft.Security.Cryptography;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.IntegrationTests
{
    class SocketClient : IConnectionObserver, IDisposable
    {
        static readonly ICryptoProvider CryptoProvider = new XorCryptoProvider(Network.DefaultXorKey);

        readonly IConnection _connection;
        bool _isDisposed;
        
        readonly AsyncSocketConnector _connector = new AsyncSocketConnector();
        readonly BufferManager _receiveBufferManager = new BufferManager();
        readonly BufferManager _sendBufferManager = new BufferManager();
        readonly IPacketContainer _packetContainer = new PacketContainer();
        readonly IPacketContainer _sendPacketContainer = new PacketContainer();
        readonly PacketHandler _packetHandler = new PacketHandler();
        readonly PacketSender _packetSender = new PacketSender();

        public event Action<IConnection, bool> ConnectionIncoming;

        public bool IsConnected { get; private set; }

        public int SentCount { get; private set; }

        public SocketClient(ClientConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _receiveBufferManager.InitBuffer(config.BufferSize, config.BufferSize);
            _sendBufferManager.InitBuffer(config.BufferSize, config.BufferSize);

            _packetSender.Setup(_sendBufferManager, 1, CryptoProvider);
            _packetContainer.Setup(CryptoProvider);
            _sendPacketContainer.Setup(CryptoProvider);

            _connection = new Connection(_receiveBufferManager, _packetContainer, _packetSender);
            _connection.Register(_connector);

            var connectionPool = new FixedSizeObjPool<IConnection>(1);
            connectionPool.Push(_connection);

            _connector.Register(this);
            _connector.Setup(config, connectionPool);
        }

        ~SocketClient()
        {
            Dispose(false);
        }

        public void GetConnectionEvent(IConnection connection, bool isConnected)
        {
            IsConnected = isConnected;
            OnConnectionEvent(connection, isConnected);
        }

        public void Send(byte[] content, PacketType packetType)
        {
            ThrowExceptionIfDisposed();
            _sendPacketContainer.AddPacket(_connection, content, packetType);
        }

        public void Start()
        {
            ThrowExceptionIfDisposed();
            _connector.StartConnect();
        }

        public void Update(int maxProcessCount)
        {
            ThrowExceptionIfDisposed();
            UpdateReceive(maxProcessCount);
            UpdateSend(maxProcessCount);
        }

        void UpdateReceive(int maxProcessCount)
        {
            var processedCount = 0;

            while (true)
            {
                var packet = _packetContainer.NextPacket();

                if (packet.IsNull ||
                    packet.Connection.IsNull)
                {
                    break;
                }

                ++processedCount;
                _packetHandler.Parse(packet);

                if (processedCount >= maxProcessCount)
                {
                    break;
                }
            }
        }

        void UpdateSend(int maxProcessCount)
        {
            if (_connection == null ||
                _connection.IsNull ||
                _connection.ConnectSocket == null ||
                !_connection.ConnectSocket.Connected)
            {
                return;
            }

            var processedCount = 0;

            while (true)
            {
                var packet = _sendPacketContainer.NextPacket();

                if (packet.IsNull ||
                    packet.Connection.IsNull)
                {
                    break;
                }

                ++SentCount;
                ++processedCount;
                _packetSender.SendMsg(_connection, packet.Content, packet.PacketType);
                Debug.Print("[SEND] Message size={0}, sent count={1}", packet.Content.Length, SentCount);

                if (processedCount >= maxProcessCount)
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            _connector.Stop();
            _packetContainer.Clear();
        }

        public void AddPacketParser(PacketType packetType, IPacketProcessor packetProcessor)
        {
            ThrowExceptionIfDisposed();
            _packetHandler.AddParser(packetType, packetProcessor);
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

            _connector.Stop();
            _packetContainer.Clear();
        }

        void ThrowExceptionIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("SocketClient");
            }
        }

        void OnConnectionEvent(IConnection connection, bool isConnected)
        {
            if (ConnectionIncoming != null)
            {
                ConnectionIncoming(connection, isConnected);
            }
        }
    }
}