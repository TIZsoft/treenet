using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Tizsoft.Collections;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class AsyncSocketConnector : IConnectionSubject, IConnectionObserver
    {
        readonly List<IConnectionObserver> _connectionObservers = new List<IConnectionObserver>();
        readonly List<Connection> _workingConnections = new List<Connection>();
        SocketAsyncEventArgs _connectArgs;
        FixedSizeObjPool<Connection> _connectionPool;
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

        Connection NewConnection(Socket socket)
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
                        Logger.LogWarning("連線數已達上限!");
                        return;
                    }

                    var newConnection = NewConnection(args.AcceptSocket);
                    _workingConnections.Add(newConnection);
                    Logger.Log(string.Format("IP: <color=cyan>{0}</color> 已連線", newConnection.DestAddress));
                    Logger.Log(string.Format("目前連線數: {0}", _workingConnections.Count));
                    Notify(newConnection, true);
                    break;

                default:
                    Logger.Log(string.Format("因為 {0} ，所以無法連線", args.SocketError));
                    Notify(Connection.NullConnection, false);
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
            var ipAddress = IPAddress.Parse(config.Address);
            _connectArgs.RemoteEndPoint = new IPEndPoint(ipAddress, config.Port);
            _connectArgs.Completed += OnConnectComplete;
        }

        public void Connect()
        {
            if (_connectArgs == null)
                InitConnectArgs(_clientConfig);

            if (!_connectArgs.AcceptSocket.ConnectAsync(_connectArgs))
                ConnectResult(_connectArgs);
        }

        public void Setup(EventArgs configArgs, FixedSizeObjPool<Connection> connectionPool)
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

        public void Notify(Connection connection, bool isConnect)
        {
            RemoveNullConnectionObservers();

            foreach (var connectionObserver in _connectionObservers)
                connectionObserver.GetConnectionEvent(connection, isConnect);
        }

        #endregion

        #region IConnectionObserver Members

        public void GetConnectionEvent(Connection connection, bool isConnect)
        {
            if (!isConnect)
            {
                FreeConnectComponent();
                if (!connection.IsNull)
                {
                    _workingConnections.Remove(connection);
                    _connectionPool.Push(connection);
                }

                Logger.Log(string.Format("IP: <color=cyan>{0}</color> 已斷線", connection.DestAddress));
                Logger.Log(string.Format("目前連線數: {0}", _workingConnections.Count));
                Notify(connection, isConnect);    
            }
        }

        #endregion
    }
}