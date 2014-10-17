﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
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
        readonly HashSet<IConnectionObserver> _observers = new HashSet<IConnectionObserver>();
        readonly HashSet<IConnection> _workingConnections = new HashSet<IConnection>();

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
            Debug.Assert(_asyncAcceptOperation.LastOperation != SocketAsyncOperation.Accept);
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
                _workingConnections.Add(newConnection);
                GLogger.Debug(string.Format("IP: <color=cyan>{0}</color> 已連線", newConnection.DestAddress));
                GLogger.Debug(string.Format("目前連線數: {0}", _workingConnections.Count));
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
                _maxNumberAcceptedClients.Dispose();
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
                if (_asyncAcceptOperation.AcceptSocket != null)
                {
                    _asyncAcceptOperation.AcceptSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception exception)
            {
                // TODO: The situation is that the socket is already closed/disposed. Log level can be ERROR.
                GLogger.Fatal(exception);
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

            try
            {
                _listenSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception exception)
            {
                // TODO: The situation is that the socket is already closed/disposed. Log level can be ERROR.
                GLogger.Fatal(exception);
            }
            finally
            {
                _listenSocket.Close();
                _listenSocket = null;
            }
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
            
            InitialHeartBeatTime();
        }

        void InitialHeartBeatTime()
        {
            if (_heartBeatTimer != null)
                _heartBeatTimer.Dispose();

            _heartBeatTimer = new Timer(_config.TimeOut);
            _heartBeatTimer.AutoReset = true;
            _heartBeatTimer.Elapsed += HeartBeatTimerOnElapsed;
            _heartBeatTimer.Start();
        }

        void HeartBeatTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_workingConnections.Count == 0)
                return;

            _heartBeatTimer.Stop();

            foreach (var workingConnection in _workingConnections)
            {
                workingConnection.Send(null, PacketType.Stream);
            }
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
            _workingConnections.CopyTo(connections);

            foreach (var workingConnection in connections)
            {
                if (workingConnection != null)
                {
                    workingConnection.Dispose();
                }
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

            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            _observers.Remove(observer);
        }

        void RemoveNullObservers()
        {
            _observers.RemoveWhere(observer => observer == null);
        }

        public void Notify(IConnection connection, bool isConnected)
        {
            if (connection == null)
                return;

            RemoveNullObservers();

            foreach (var observer in _observers)
                observer.GetConnectionEvent(connection, isConnected);
        }

        #endregion


        #region IConnectionObserver Members

        public void GetConnectionEvent(IConnection connection, bool isConnected)
        {
            if (isConnected)
                return;

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