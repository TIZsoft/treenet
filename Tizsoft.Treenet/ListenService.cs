using System;
using System.Threading.Tasks;
using Tizsoft.Treenet.Factory;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ListenService : IService, IConnectionSubject
    {
        ConnectionFactory _connectionFactory;
        readonly BufferManager _receiveBufferManager = new BufferManager();
        readonly BufferManager _sendBufferManager = new BufferManager();
        readonly AsyncSocketListener _socketListener = new AsyncSocketListener();
        readonly IPacketContainer _packetContainer = new PacketContainer();
        readonly PacketHandler _packetHandler = new PacketHandler();
        readonly PacketSender _packetSender = new PacketSender();

        public void AddParser(PacketType type, IPacketProcessor processor)
        {
            _packetHandler.AddParser(type, processor);
        }

        #region IService Members

        public void Start()
        {
            _socketListener.Start();
            IsWorking = true;
        }

        public void Setup(EventArgs configArgs)
        {
            if (configArgs == null)
                throw new ArgumentNullException("configArgs");

            var config = configArgs as ServerConfig;

            if (config == null)
                throw new InvalidCastException("Type of configArgs is not ServerConfig.");

            var packetProtocol = new PacketProtocol(config.PacketProtocolSettings);
            _receiveBufferManager.InitBuffer(config.MaxConnections, config.BufferSize);
            _connectionFactory = new ConnectionFactory(_receiveBufferManager, _packetContainer, _packetSender, packetProtocol, config.MaxMessageSize);
            var sendConnectionCount = Math.Max(1, config.MaxConnections / 10);
            _sendBufferManager.InitBuffer(sendConnectionCount, config.BufferSize);
            _packetSender.Setup(_sendBufferManager, sendConnectionCount);
            _packetSender.PacketProtocol = packetProtocol;
            _socketListener.Setup(config, _connectionFactory);
        }

        public void Update()
        {
            if (IsWorking)
            {
                var packet = _packetContainer.NextPacket();

                if (packet != Packet.Null)
                    _packetHandler.Parse(packet);
            }
        }

        public async Task UpdateAsync()
        {
            await Task.Run(() => Update()).ConfigureAwait(false);
        }

        public void Stop()
        {
            _socketListener.Stop();
            _packetContainer.Clear();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        #endregion

        #region IConnectionSubject Members

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

        public int Count { get { return _socketListener.Count; } }

        #endregion
    }
}