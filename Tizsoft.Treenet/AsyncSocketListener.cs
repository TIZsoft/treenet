using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Tizsoft.Log;
using Tizsoft.Treenet.Factory;
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
        readonly ConcurrentStack<SocketAsyncEventArgs> _asyncAcceptArgs = new ConcurrentStack<SocketAsyncEventArgs>();
        ServerConfig _config;
        ConnectionFactory _connectionFactory;
        Timer _heartBeatTimer;

        readonly ConcurrentDictionary<IConnectionObserver, object> _observers = new ConcurrentDictionary<IConnectionObserver, object>();
        readonly ConcurrentDictionary<IConnection, object> _workingConnections = new ConcurrentDictionary<IConnection, object>();

        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        void StartAccept()
        {
            SocketAsyncEventArgs asyncAccept;

            if (!_asyncAcceptArgs.TryPop(out asyncAccept))
                return;

            _maxNumberAcceptedClients.WaitOne();

            var willRaiseEvent = _listenSocket.AcceptAsync(asyncAccept);

            if (willRaiseEvent)
            {
                return;
            }

            ProcessAccept(asyncAccept);
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
                if (_workingConnections.Count >= _config.MaxConnections)
                {
                    GLogger.Warn("連線數已達上限!");
                    CloseAcceptSocket(acceptOperation);
                    return;
                }

                var newConnection = CreateNewConnection(acceptOperation.AcceptSocket);

                if (newConnection.IsNull)
                {
                    GLogger.Warn("無法建立連線!");
                    CloseAcceptSocket(acceptOperation);
                    _maxNumberAcceptedClients.Release();
                    _asyncAcceptArgs.Push(acceptOperation);
                    StartAccept();
                    return;
                }

                _workingConnections.TryAdd(newConnection, acceptOperation.UserToken);
                GLogger.Debug("IP: {0} 已連線", newConnection.DestAddress);
                GLogger.Debug("目前連線數: {0}", _workingConnections.Count);
                GLogger.Debug("可連線數: {0}", _config.MaxConnections - _workingConnections.Count);
                Notify(newConnection, true);
            }
            else
            {
                var lastError = acceptOperation.SocketError;
                // Handle bad accept.
                CloseAcceptSocket(acceptOperation);

                // Server close on purpose.
                if (lastError == SocketError.OperationAborted)
                    return;
            }

            acceptOperation.AcceptSocket = null;
            _asyncAcceptArgs.Push(acceptOperation);

            // Accept the next connection request.
            StartAccept();
        }

        IConnection CreateNewConnection(Socket socket)
        {
            var connection = _connectionFactory.NewConnection();
            connection.SetConnection(socket);
            connection.Register(this);
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

        void CloseAcceptSocket(SocketAsyncEventArgs args)
        {
            if (args != null)
            {
                try
                {
                    if (args.AcceptSocket != null && args.AcceptSocket.Connected)
                        args.AcceptSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception exception)
                {
                    GLogger.Error(exception);
                }
                finally
                {
                    if (args.AcceptSocket != null)
                        args.AcceptSocket.Close();

                    args.AcceptSocket = null;
                    _asyncAcceptArgs.Push(args);
                }
            }
        }

        void CloseAllAsyncAccept()
        {
            if (_asyncAcceptArgs.Count == 0)
                return;

            SocketAsyncEventArgs asyncAccept;

            while (_asyncAcceptArgs.TryPop(out asyncAccept))
            {
                try
                {
                    if (asyncAccept == null)
                        continue;

                    asyncAccept.Completed -= OnAsyncAcceptCompleted;

                    if (asyncAccept.AcceptSocket != null && asyncAccept.AcceptSocket.Connected)
                        asyncAccept.AcceptSocket.Shutdown(SocketShutdown.Both);

                    asyncAccept.AcceptSocket = null;
                }
                catch (Exception exception)
                {
                    GLogger.Error(exception);
                }
                finally
                {
                    if (asyncAccept != null)
                    {
                        if (asyncAccept.AcceptSocket != null)
                        {
                            asyncAccept.AcceptSocket.Close();
                        }

                        asyncAccept.Dispose();
                    }
                }    
            }

            _asyncAcceptArgs.Clear();
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

        //public void Setup(ServerConfig config, FixedSizeObjPool<IConnection> connectionPool)
        public void Setup(ServerConfig config, ConnectionFactory connectionFactory)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            if (_listenSocket != null)
            {
                FreeAcceptComponents();
            }

            _config = config;
            _connectionFactory = connectionFactory;
            InitAsyncAccept(_config);
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

        void InitAsyncAccept(ServerConfig config)
        {
            for (var i = 0; i < config.MaxAcceptions; ++i)
            {
                var asyncAccept = new SocketAsyncEventArgs();
                asyncAccept.Completed += OnAsyncAcceptCompleted;
                _asyncAcceptArgs.Push(asyncAccept);
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
            _workingConnections.Keys.CopyTo(connections, 0);

            foreach (var workingConnection in connections.Where(workingConnection => workingConnection != null))
            {
                workingConnection.Dispose();
            }
        }

        void FreeAcceptComponents()
        {
            CloseListenSocket();
            CloseAllAsyncAccept();
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

        public void Notify(IConnection connection, bool isConnected)
        {
            if (connection == null)
                return;

            foreach (var observer in _observers.Where(p => p.Key != null))
            {
                observer.Key.GetConnectionEvent(connection, isConnected);
            }
        }

        public int Count { get { return _workingConnections.Count; } }

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
            }

            if (_maxNumberAcceptedClients != null)
                _maxNumberAcceptedClients.Release();

            GLogger.Debug("IP: {0} 已斷線", connection.DestAddress);
            GLogger.Debug("目前連線數: {0}", Count);
            GLogger.Debug("可連線數: {0}", _config.MaxConnections - Count);
            Notify(connection, false);
        }

        #endregion
    }
}