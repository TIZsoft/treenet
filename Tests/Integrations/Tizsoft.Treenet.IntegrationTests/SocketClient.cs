using System;
using System.Diagnostics;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.IntegrationTests
{
    class SocketClient : IConnectionObserver, IDisposable
    {
        readonly IConnection _connection;
        bool _isDisposed;
        bool _isConnecting;
        
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

            var packetProtocol = new PacketProtocol(config.PacketProtocolSettings);

            _receiveBufferManager.InitBuffer(1, config.BufferSize);
            _sendBufferManager.InitBuffer(1, config.BufferSize);

            _packetSender.Setup(_sendBufferManager, 1);
            _packetSender.PacketProtocol = packetProtocol;

            _connection = new Connection(_receiveBufferManager, _packetContainer, _packetSender, config.MaxMessageSize);
            _connection.Register(_connector);
            _connection.PacketProtocol = packetProtocol;

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
            _isConnecting = true;
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
                var content = packet.Content;
                _packetSender.SendMsg(_connection, content, packet.PacketType);
                var guidBytes = new byte[16];
                Array.Copy(content, 0, guidBytes, 0, guidBytes.Length);
                var guid = new Guid(guidBytes);
                Debug.Print("[SEND] GUID={0}", guid);

                if (processedCount >= maxProcessCount)
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            _isConnecting = false;
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
            _isConnecting = false;
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
            if (isConnected)
            {
                _isConnecting = false;
                if (ConnectionIncoming != null)
                {
                    ConnectionIncoming(connection, true);
                }
            }
            else if (_isConnecting)
            {
                _connector.StartConnect();
            }
        }
    }
}