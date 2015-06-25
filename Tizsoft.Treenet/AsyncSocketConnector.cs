using System;
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
            var newConnection = Connection.Null;

            switch (connectOperation.SocketError)
            {
                case SocketError.Success:
                    if (_connectionPool.Count <= 0)
                    {
                        GLogger.Warn("連線數已達上限!");
                        return;
                    }

                    newConnection = CreateNewConnection(connectOperation.AcceptSocket);
                    _workingConnections.Add(newConnection);
                    GLogger.Debug("IP: {0} 已連線", newConnection.DestAddress);
                    GLogger.Debug("目前連線數: {0}", _workingConnections.Count);
                    break;

                default:
                    GLogger.Debug("因為 {0} ，所以無法連線", connectOperation.SocketError);
                    break;
            }

            var isConnected = connectOperation.SocketError == SocketError.Success;
            connectOperation.Dispose();
            Notify(newConnection, isConnected);
        }

        SocketAsyncEventArgs CreateConnectOperation(ClientConfig config)
        {
            var args = new SocketAsyncEventArgs
            {
                AcceptSocket = new Socket(AddressFamily.InterNetwork, config.TransferType, config.UseProtocol),
                RemoteEndPoint = Network.GetIpEndPoint(config.Address, config.Port),
            };
            args.Completed += OnConnectCompleted;
            return args;
        }

        public void StartConnect()
        {
            _connectOperation = CreateConnectOperation(_clientConfig);
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
            //InitConnectOperation(_clientConfig);
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

            GLogger.Debug("IP: {0} 已斷線", connection.DestAddress);
            GLogger.Debug("目前連線數: {0}", _workingConnections.Count);
            Notify(connection, false);
        }

        #endregion
    }
}