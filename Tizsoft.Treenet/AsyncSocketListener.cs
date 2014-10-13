using System;
using System.Collections.Generic;
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
        Socket _listenSocket;
        Semaphore _maxNumberAcceptedClients;
        SocketAsyncEventArgs _acceptAsyncOp;
        ServerConfig _config;
        FixedSizeObjPool<Connection> _connectionPool;
        Timer _heartBeatTimer;
        readonly List<IConnectionObserver> _observers = new List<IConnectionObserver>();
        readonly List<Connection> _workingConnections = new List<Connection>();

        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        /// <param name="args">
        /// The context object to use when issuing the accept operation on
        /// the server's listening connection.
        /// </param>
        void StartAccept(SocketAsyncEventArgs args)
        {
            if (args == null)
            {
                args = new SocketAsyncEventArgs();
                args.Completed += OnAcceptComplete;
                _acceptAsyncOp = args;
            }
            else
            {
                // Socket must be cleared since the context object is being reused.
                args.AcceptSocket = null;
            }

            _maxNumberAcceptedClients.WaitOne();

            if (!_listenSocket.AcceptAsync(args))
                AcceptResult(args);
        }

        /// <summary>
        /// This method is called whenever a receive or send operation is completed on a connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">SocketAsyncEventArg associated with the completed receive operation.</param>
        void OnAcceptComplete(object sender, SocketAsyncEventArgs args)
        {
            if (_acceptAsyncOp.LastOperation != SocketAsyncOperation.Accept)
                return;

            AcceptResult(args);
        }

        void AcceptResult(SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
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
            }
            else
            {
                // Server close on purpose.
                if (args.SocketError == SocketError.OperationAborted)
                    return;
            }

            // Accept the next connection request.
            StartAccept(args);
        }

        Connection NewConnection(Socket socket)
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

        void CloseAsyncAcceptOp(SocketAsyncEventArgs args)
        {
            if (args == null)
            {
                return;
            }

            try
            {
                if (args.AcceptSocket != null)
                    args.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception exception)
            {
                GLogger.Fatal(exception);
            }
            finally
            {
                args.Dispose();
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
                GLogger.Fatal(exception);
            }
            finally
            {
                if (_listenSocket != null)
                    _listenSocket.Dispose();

                _listenSocket = null;
            }
        }

        public void Setup(ServerConfig config, FixedSizeObjPool<Connection> connectionPool)
        {
            _config = config;
            _connectionPool = connectionPool;

            if (_listenSocket != null)
                FreeAcceptComponents();

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
            _listenSocket.Listen(_config.MaxConnections);
            StartAccept(null);
            GLogger.Debug("Server try accept...");
        }

        public void Stop()
        {
            FreeAcceptComponents();
            FreeWorkingConnections();
        }

        void FreeWorkingConnections()
        {
            _workingConnections.RemoveAll(observer => observer == null);

            foreach (var workingConnection in _workingConnections.ToArray())
                workingConnection.Dispose();
        }

        void FreeAcceptComponents()
        {
            CloseListenSocket();
            CloseAsyncAcceptOp(_acceptAsyncOp);
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
            _observers.RemoveAll(observer => observer == null);
        }

        public void Notify(Connection connection, bool isConnect)
        {
            if (connection == null)
                return;

            RemoveNullObservers();

            foreach (var observer in _observers)
                observer.GetConnectionEvent(connection, isConnect);
        }

        #endregion

        #region IConnectionObserver Members

        public void GetConnectionEvent(Connection connection, bool isConnect)
        {
            if (!isConnect)
            {
                if (!connection.IsNull)
                {
                    _workingConnections.Remove(connection);
                    _connectionPool.Push(connection);    
                }
                
                GLogger.Debug(string.Format("IP: <color=cyan>{0}</color> 已斷線", connection.DestAddress));
                GLogger.Debug(string.Format("目前連線數: {0}", _workingConnections.Count));
                Notify(connection, isConnect);
            }
        }

        #endregion
    }
}