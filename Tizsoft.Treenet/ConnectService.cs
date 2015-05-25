using System;
using System.Threading.Tasks;
using Tizsoft.Collections;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    /// <summary>
    /// support "only 1" connection
    /// </summary>
    public class ConnectService : IService, IConnectionSubject
    {
        Connection _connection;
        ClientConfig _config;
        FixedSizeObjPool<IConnection> _connectionPool;
        readonly AsyncSocketConnector _connector = new AsyncSocketConnector();
        readonly BufferManager _receiveBufferManager = new BufferManager();
        readonly BufferManager _sendBufferManager = new BufferManager();
        readonly IPacketContainer _packetContainer = new PacketContainer();
        readonly PacketHandler _packetHandler = new PacketHandler();
        readonly PacketSender _packetSender = new PacketSender();

        void InitConnectionPool(PacketProtocol packetProtocol)
        {
            _connection = new Connection(_receiveBufferManager, _packetContainer, _packetSender, _config.MaxMessageSize);
            _connection.PacketProtocol = packetProtocol;
            _connection.Register(_connector);
            _connectionPool = new FixedSizeObjPool<IConnection>(1);
            _connectionPool.Push(_connection);
        }

        public void Send(byte[] contents, PacketType packetType)
        {
            _connection.Send(contents, packetType);
        }

        public void AddParser(PacketType type, IPacketProcessor processor)
        {
            _packetHandler.AddParser(type, processor);
        }

        #region IService Members

        public void Start()
        {
            _connector.StartConnect();
            IsWorking = true;
        }

        public void Setup(EventArgs configArgs)
        {
            _config = (ClientConfig)configArgs;

            if (_config == null)
                throw new InvalidCastException("configArgs");

            var packetProtocol = new PacketProtocol(_config.PacketProtocolSettings);
            _receiveBufferManager.InitBuffer(1, _config.BufferSize);
            _sendBufferManager.InitBuffer(1, _config.BufferSize);
            InitConnectionPool(packetProtocol);
            _packetSender.Setup(_sendBufferManager, 1);
            _packetSender.PacketProtocol = packetProtocol;
            _connector.Setup(_config, _connectionPool);
        }

        public void Update()
        {
            if (IsWorking)
            {
                var packet = _packetContainer.NextPacket();

                if (!packet.IsNull)
                    _packetHandler.Parse(packet);
            }
        }

        public async Task UpdateAsync()
        {
            await Task.Run(() => Update()).ConfigureAwait(false);
        }

        public void Stop()
        {
            _connection.Dispose();
            _connector.Stop();
            _packetContainer.Clear();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        public int RemainConnection { get { return _connectionPool.Count; } }

        #endregion

        #region IConnectionSubject Members

        public void Register(IConnectionObserver observer)
        {
            _connector.Register(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            _connector.Unregister(observer);
        }

        public void Notify(IConnection connection, bool isConnected)
        {
            _connector.Notify(connection, isConnected);
        }

        #endregion
    }
}