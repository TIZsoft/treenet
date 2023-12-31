﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tizsoft.Treenet.Factory;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    /// <summary>
    /// support "only 1" connection
    /// </summary>
    public class ConnectService : IService, IConnectionSubject, IConnectionObserver
    {
        IConnection _connection;
        ClientConfig _config;
        ConnectionFactory _connectionFactory;
        readonly AsyncSocketConnector _connector = new AsyncSocketConnector();
        readonly BufferManager _receiveBufferManager = new BufferManager();
        readonly BufferManager _sendBufferManager = new BufferManager();
        readonly IPacketContainer _packetContainer = new PacketContainer();
        readonly PacketHandler _packetHandler = new PacketHandler();
        readonly PacketSender _packetSender = new PacketSender();

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
            _connectionFactory = new ConnectionFactory(_receiveBufferManager, _packetContainer, _packetSender, packetProtocol, _config.MaxMessageSize);
            _packetSender.Setup(_sendBufferManager, 1);
            _packetSender.PacketProtocol = packetProtocol;
            _connector.Setup(_config, _connectionFactory);
            _connector.Register(this);
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
            if (_connection != null)
            {
                _connection.Dispose();
            }

            _connector.Stop();
            _connector.Unregister(this);
            _packetContainer.Clear();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

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

        public int Count { get { return _connector.Count; } }

        #endregion

        public void GetConnectionEvent(IConnection connection, bool isConnected)
        {
            if (isConnected)
            {
                _connection = connection;
            }
        }
    }
}