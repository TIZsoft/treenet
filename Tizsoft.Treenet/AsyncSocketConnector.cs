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
        readonly List<IConnectionObserver> _connectionObservers = new List<IConnectionObserver>();
        readonly List<IConnection> _workingConnections = new List<IConnection>();
        SocketAsyncEventArgs _connectArgs;
        FixedSizeObjPool<IConnection> _connectionPool;
        ClientConfig _clientConfig;

        void OnConnectComplete(object sender, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ConnectResult(args);
                    break;
            }
        }

        IConnection NewConnection(Socket socket)
        {
            var connection = _connectionPool.Pop();
            connection.SetConnection(socket);
            return connection;
        }

        void ConnectResult(SocketAsyncEventArgs args)
        {
            switch (args.SocketError)
            {
                case SocketError.Success:
                    if (_connectionPool.Count <= 0)
                    {
                        GLogger.Warn("連線數已達上限!");
                        return;
                    }

                    var newConnection = NewConnection(args.AcceptSocket);
                    _workingConnections.Add(newConnection);
                    GLogger.Debug(string.Format("IP: <color=cyan>{0}</color> 已連線", newConnection.DestAddress));
                    GLogger.Debug(string.Format("目前連線數: {0}", _workingConnections.Count));
                    Notify(newConnection, true);
                    break;

                default:
                    GLogger.Debug(string.Format("因為 {0} ，所以無法連線", args.SocketError));
                    Notify(Connection.Null, false);
                    break;
            }
        }

        void InitConnectArgs(ClientConfig config)
        {
            if (_connectArgs != null)
                _connectArgs.Dispose();

            _connectArgs = new SocketAsyncEventArgs
            {
                AcceptSocket = new Socket(AddressFamily.InterNetwork, config.TransferType, config.UseProtocol)
            };

            var endPoint = Network.GetIpEndPoint(config.Address, config.Port);
            _connectArgs.RemoteEndPoint = endPoint;
            _connectArgs.Completed += OnConnectComplete;
        }

        public void Connect()
        {
            if (_connectArgs == null)
                InitConnectArgs(_clientConfig);

            if (!_connectArgs.AcceptSocket.ConnectAsync(_connectArgs))
                ConnectResult(_connectArgs);
        }

        public void Setup(EventArgs configArgs, FixedSizeObjPool<IConnection> connectionPool)
        {
            _clientConfig = (ClientConfig) configArgs;

            if (_clientConfig == null)
                throw new InvalidCastException("config");

            _connectionPool = connectionPool;
            InitConnectArgs(_clientConfig);
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
            if (_connectArgs != null)
                _connectArgs.Dispose();
            _connectArgs = null;
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
            _connectionObservers.RemoveAll(observer => observer == null);
        }

        public void Notify(IConnection connection, bool isConnected)
        {
            RemoveNullConnectionObservers();

            foreach (var connectionObserver in _connectionObservers)
                connectionObserver.GetConnectionEvent(connection, isConnected);
        }

        #endregion

        #region IConnectionObserver Members

        public void GetConnectionEvent(IConnection connection, bool isConnect)
        {
            if (isConnect)
                return;

            FreeConnectComponent();
            if (!connection.IsNull)
            {
                _workingConnections.Remove(connection);
                _connectionPool.Push(connection);
            }

            GLogger.Debug(string.Format("IP: <color=cyan>{0}</color> 已斷線", connection.DestAddress));
            GLogger.Debug(string.Format("目前連線數: {0}", _workingConnections.Count));
            Notify(connection, false);
        }

        #endregion
    }
}