﻿using System;
using System.Collections.Generic;
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
        FixedSizeObjPool<Connection> _connectionPool;
        readonly AsyncSocketConnector _connector;
        readonly BufferManager _bufferManager;
        readonly IPacketContainer _packetContainer;
        //readonly ConnectionObserver _connectionObserver;
        readonly PacketHandler _packetHandler;
        readonly List<IConnectionObserver> _connectionObservers = new List<IConnectionObserver>();

        void InitConnectionPool()
        {
            _connection = new Connection(_bufferManager, _packetContainer);
            _connection.Register(_connector);
            _connectionPool = new FixedSizeObjPool<Connection>(1);
            _connectionPool.Push(_connection);
        }

        public ConnectService()
        {
            _connector = new AsyncSocketConnector();
            _packetContainer = new PacketContainer();
            _bufferManager = new BufferManager();
            _packetHandler = new PacketHandler();
        }

        public void Send(byte[] contents)
        {
            _connection.Send(contents);
        }

        public Packet Receive()
        {
            return IsWorking ? _packetContainer.NextPacket() : Packet.NullPacket;
        }

        public void AddParser(PacketType type, IPacketProcessor processor)
        {
            _packetHandler.AddParser(type, processor);
        }

        #region IService Members

        public void Start()
        {
            _connector.Connect();
            IsWorking = true;
        }

        public void Setup(EventArgs configArgs)
        {
            _config = (ClientConfig)configArgs;

            if (_config == null)
                throw new InvalidCastException("configArgs");

            InitConnectionPool();
            _bufferManager.InitBuffer(_config.BufferSize * 2, _config.BufferSize);
            _connector.Setup(_config, _connectionPool);
        }

        public void Update()
        {
        }

        public void Stop()
        {
            _connector.Stop();
            _packetContainer.Clear();
            IsWorking = false;
        }

        public bool IsWorking { get; private set; }

        #endregion

        #region IConnectionSubject Members

        public void Register(IConnectionObserver observer)
        {
            if (!_connectionObservers.Contains(observer))
                _connectionObservers.Add(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            _connectionObservers.Remove(observer);
        }

        public void Notify(Connection connection, bool isConnect)
        {
            foreach (var connectionObserver in _connectionObservers)
            {
                connectionObserver.GetConnectionEvent(connection, isConnect);
            }
        }

        #endregion
    }
}