﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Collections;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class AsyncSocketConnector : IConnectionSubject, IConnectionObserver
    {
        readonly HashSet<IConnectionObserver> _connectionObservers = new HashSet<IConnectionObserver>();
        readonly List<IConnection> _workingConnections = new List<IConnection>();
        SocketAsyncEventArgs _connectOperation;
        FixedSizeObjPool<IConnection> _connectionPool;
        ClientConfig _clientConfig;

        void OnConnectCompleted(object sender, SocketAsyncEventArgs socketOperation)
        {
            switch (socketOperation.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(socketOperation);
                    break;
            }
        }

        IConnection CreateNewConnection(Socket socket)
        {
            var connection = _connectionPool.Pop();
            connection.SetConnection(socket);
            return connection;
        }

        void ProcessConnect(SocketAsyncEventArgs connectOperation)
        {
            switch (connectOperation.SocketError)
            {
                case SocketError.Success:
                    if (_connectionPool.Count <= 0)
                    {
                        GLogger.Warn("連線數已達上限!");
                        return;
                    }

                    var newConnection = CreateNewConnection(connectOperation.AcceptSocket);
                    _workingConnections.Add(newConnection);
                    GLogger.DebugFormat("IP: <color=cyan>{0}</color> 已連線", newConnection.DestAddress);
                    GLogger.DebugFormat("目前連線數: {0}", _workingConnections.Count);
                    Notify(newConnection, true);
                    break;

                default:
                    GLogger.DebugFormat("因為 {0} ，所以無法連線", connectOperation.SocketError);
                    Notify(Connection.Null, false);
                    break;
            }
        }

        void InitConnectOperation(ClientConfig config)
        {
            if (_connectOperation != null)
                _connectOperation.Dispose();

            _connectOperation = new SocketAsyncEventArgs
            {
                AcceptSocket = new Socket(AddressFamily.InterNetwork, config.TransferType, config.UseProtocol)
            };

            var endPoint = Network.GetIpEndPoint(config.Address, config.Port);
            _connectOperation.RemoteEndPoint = endPoint;
            _connectOperation.Completed += OnConnectCompleted;
        }

        public void StartConnect()
        {
            if (_connectOperation == null)
                InitConnectOperation(_clientConfig);

            var willRaiseEvent = _connectOperation.AcceptSocket.ConnectAsync(_connectOperation);

            if (willRaiseEvent)
            {
                return;
            }

            ProcessConnect(_connectOperation);
        }

        public void Setup(EventArgs configArgs, FixedSizeObjPool<IConnection> connectionPool)
        {
            _clientConfig = (ClientConfig) configArgs;

            if (_clientConfig == null)
                throw new InvalidCastException("config");

            _connectionPool = connectionPool;
            InitConnectOperation(_clientConfig);
        }

        public void Stop()
        {
            FreeConnectComponent();
            FreeWorkingConnections();
        }

        void FreeWorkingConnections()
        {
            foreach (var workingConnection in _workingConnections.ToArray())
                workingConnection.Dispose();
        }

        void FreeConnectComponent()
        {
            if (_connectOperation != null)
                _connectOperation.Dispose();
                
            _connectOperation = null;
        }

        #region IConnectionSubject Members

        public void Register(IConnectionObserver observer)
        {
            if (observer != null && !_connectionObservers.Contains(observer))
                _connectionObservers.Add(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            _connectionObservers.Remove(observer);
        }

        void RemoveNullConnectionObservers()
        {
            _connectionObservers.RemoveWhere(observer => observer == null);
        }

        public void Notify(IConnection connection, bool isConnected)
        {
            RemoveNullConnectionObservers();

            foreach (var connectionObserver in _connectionObservers)
                connectionObserver.GetConnectionEvent(connection, isConnected);
        }

        #endregion

        #region IConnectionObserver Members

        public void GetConnectionEvent(IConnection connection, bool isConnected)
        {
            if (isConnected)
                return;

            FreeConnectComponent();
            if (!connection.IsNull)
            {
                _workingConnections.Remove(connection);
                _connectionPool.Push(connection);
            }

            GLogger.DebugFormat("IP: <color=cyan>{0}</color> 已斷線", connection.DestAddress);
            GLogger.DebugFormat("目前連線數: {0}", _workingConnections.Count);
            Notify(connection, false);
        }

        #endregion
    }
}