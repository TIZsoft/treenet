﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Tizsoft.Collections;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;
using Timer = System.Timers.Timer;

namespace Tizsoft.Treenet
{
    public class AsyncSocketListener : IConnectionSubject, IConnectionObserver
    {
        // The socket used to lsiten incoming connection requests.
        Socket _listenSocket;

        // Not for controlling threading.
        Semaphore _maxNumberAcceptedClients;

        // Used to do asynchronous accept operation.
        SocketAsyncEventArgs _asyncAcceptOperation;

        ServerConfig _config;
        FixedSizeObjPool<IConnection> _connectionPool;
        Timer _heartBeatTimer;

        readonly ConcurrentDictionary<IConnectionObserver, object> _observers = new ConcurrentDictionary<IConnectionObserver, object>();
        readonly ConcurrentDictionary<IConnection, object> _workingConnections = new ConcurrentDictionary<IConnection, object>();

        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        void StartAccept()
        {
            if (_asyncAcceptOperation == null)
            {
                _asyncAcceptOperation = new SocketAsyncEventArgs();
                _asyncAcceptOperation.Completed += OnAsyncAcceptCompleted;
            }
            else
            {
                // Socket must be cleared since the context object is being reused.
                _asyncAcceptOperation.AcceptSocket = null;
            }

            _maxNumberAcceptedClients.WaitOne();

            var willRaiseEvent = _listenSocket.AcceptAsync(_asyncAcceptOperation);

            if (willRaiseEvent)
            {
                return;
            }

            ProcessAccept(_asyncAcceptOperation);
        }

        /// <summary>
        /// This method is called whenever a receive or send operation is completed on a connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="socketOperation">SocketAsyncEventArg associated with the completed receive operation.</param>
        void OnAsyncAcceptCompleted(object sender, SocketAsyncEventArgs socketOperation)
        {
            Debug.Assert(socketOperation.LastOperation == SocketAsyncOperation.Accept);
            ProcessAccept(socketOperation);
        }

        void ProcessAccept(SocketAsyncEventArgs acceptOperation)
        {
            if (acceptOperation.SocketError == SocketError.Success)
            {
                if (_connectionPool.Count <= 0)
                {
                    GLogger.Warn("連線數已達上限!");
                    return;
                }

                var newConnection = CreateNewConnection(acceptOperation.AcceptSocket);
                _workingConnections.TryAdd(newConnection, acceptOperation.UserToken);
                GLogger.Debug("IP: {0} 已連線", newConnection.DestAddress);
                GLogger.Debug("目前連線數: {0}", _workingConnections.Count);
                GLogger.Debug("可連線數: {0}", _connectionPool.Count);
                Notify(newConnection, true);
            }
            else
            {
                // Handle bad accept.
                if (acceptOperation.AcceptSocket != null)
                {
                    acceptOperation.AcceptSocket.Close();
                }

                // Server close on purpose.
                if (acceptOperation.SocketError == SocketError.OperationAborted)
                    return;
            }

            // Accept the next connection request.
            StartAccept();
        }

        IConnection CreateNewConnection(Socket socket)
        {
            var connection = _connectionPool.Pop();
            connection.SetConnection(socket);
            return connection;
        }

        void CloseSemaphore()
        {
            try
            {
                if (_maxNumberAcceptedClients != null)
                    _maxNumberAcceptedClients.Close();
            }
            catch (Exception exception)
            {
                GLogger.Fatal(exception);
            }
            finally
            {
                _maxNumberAcceptedClients = null;
            }
        }

        void CloseAsyncAcceptOperation()
        {
            if (_asyncAcceptOperation == null)
            {
                return;
            }

            try
            {
                if (_asyncAcceptOperation.AcceptSocket != null &&
                    _asyncAcceptOperation.AcceptSocket.Connected)
                {
                    _asyncAcceptOperation.AcceptSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception exception)
            {
                GLogger.Error(exception);
            }
            finally
            {
                if (_asyncAcceptOperation.AcceptSocket != null)
                {
                    _asyncAcceptOperation.AcceptSocket.Close();
                }

                _asyncAcceptOperation.Dispose();
                _asyncAcceptOperation = null;
            }
        }

        void CloseListenSocket()
        {
            if (_listenSocket == null)
            {
                return;
            }

            //Since the listening socket is never actually connected (it only accepts connected sockets), there is no Disconnect operation.
            //Rather, closing a listening socket simply informs the OS that the socket is no longer listening and frees those resources immediately.
            _listenSocket.Close();
            _listenSocket = null;
        }

        public void Setup(ServerConfig config, FixedSizeObjPool<IConnection> connectionPool)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (connectionPool == null)
            {
                throw new ArgumentNullException("connectionPool");
            }

            if (_listenSocket != null)
            {
                FreeAcceptComponents();
            }

            _config = config;
            _connectionPool = connectionPool;
            _maxNumberAcceptedClients = new Semaphore(config.MaxConnections, config.MaxConnections);

            var endPoint = Network.GetIpEndPoint(config.Address, config.Port);
            _listenSocket = new Socket(endPoint.AddressFamily, config.TransferType, config.UseProtocol);
            _listenSocket.Bind(endPoint);
            
            InitialHeartBeatTime(config.TimeOut);
        }

        void InitialHeartBeatTime(int timeOut)
        {
            if (_heartBeatTimer != null)
                _heartBeatTimer.Close();

            if (timeOut == 0)
                return;

            _heartBeatTimer = new Timer(timeOut) {AutoReset = true};
            _heartBeatTimer.Elapsed += (sender, args) =>
            {
                if (_workingConnections.Count == 0)
                    return;

                foreach (var workingConnection in _workingConnections.Keys.Where(workingConnection => workingConnection.IsActive))
                {
                    workingConnection.IdleTime += Network.DefaultTimeOutTick;

                    if (workingConnection.IdleTime > _config.TimeOut)
                        workingConnection.Send(null, PacketType.Stream);
                }
            };
            _heartBeatTimer.Start();
        }

        public void Start()
        {
            _listenSocket.Listen(_config.Backlog);
            StartAccept();
            GLogger.Debug("Server try accept...");
        }

        public void Stop()
        {
            FreeAcceptComponents();
            FreeWorkingConnections();
        }

        void FreeWorkingConnections()
        {
            var connections = new IConnection[_workingConnections.Count];
            _workingConnections.Keys.CopyTo(connections, 0);

            foreach (var workingConnection in connections.Where(workingConnection => workingConnection != null))
            {
                workingConnection.Dispose();
            }
        }

        void FreeAcceptComponents()
        {
            CloseListenSocket();
            CloseAsyncAcceptOperation();
            CloseSemaphore();
        }

        #region IConnectionSubject Members

        public void Register(IConnectionObserver observer)
        {
            if (observer == null)
                return;

            _observers.TryAdd(observer, observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            object storedValue;
            _observers.TryRemove(observer, out storedValue);
        }

        //void RemoveNullObservers()
        //{
        //    _observers.ke(observer => observer == null);
        //}

        public void Notify(IConnection connection, bool isConnected)
        {
            if (connection == null)
                return;

            foreach (var observer in _observers.Where(p => p.Key != null))
            {
                observer.Key.GetConnectionEvent(connection, isConnected);
            }
        }

        #endregion

        #region IConnectionObserver Members

        public void GetConnectionEvent(IConnection connection, bool isConnected)
        {
            if (isConnected)
                return;

            if (!connection.IsNull)
            {
                object storedValue;
                _workingConnections.TryRemove(connection, out storedValue);
                _connectionPool.Push(connection);    
            }

            if (_maxNumberAcceptedClients != null)
                _maxNumberAcceptedClients.Release();

            GLogger.Debug("IP: {0} 已斷線", connection.DestAddress);
            GLogger.Debug("目前連線數: {0}", _workingConnections.Count);
            GLogger.Debug("可連線數: {0}", _connectionPool.Count);
            Notify(connection, false);
        }

        #endregion
    }
}