using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class AsyncSocketListener : IConnectionSubject
    {
        Socket _listenSocket;
        Semaphore _maxNumberAcceptedClients;
        SocketAsyncEventArgs _acceptAsyncOp;
        ServerConfig _config;
        readonly List<IConnectionObserver> _observers;

        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        /// <param name="args">
        /// The context object to use when issuing the accept operation on
        /// the server's listening socket.
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
        /// This method is called whenever a receive or send operation is completed on a socket
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
                Notify(args.AcceptSocket, true);
            }
            else
            {
                Notify(args.AcceptSocket, false);

                // Server close on purpose.
                if (args.SocketError == SocketError.OperationAborted)
                    return;
            }

            // Accept the next connection request.
            StartAccept(args);
        }

        void CloseSemaphore()
        {
            try
            {
                _maxNumberAcceptedClients.Dispose();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
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
                Logger.LogException(exception);
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
                Logger.LogException(exception);
            }
            finally
            {
                _listenSocket.Dispose();
                _listenSocket = null;
            }
        }

        public AsyncSocketListener()
        {
            _observers = new List<IConnectionObserver>();
        }

        public void Setup(ServerConfig config)
        {
            _config = config;

            if (_listenSocket != null)
            {
                CloseListenSocket();
                CloseAsyncAcceptOp(_acceptAsyncOp);
                CloseSemaphore();
            }

            _maxNumberAcceptedClients = new Semaphore(config.MaxConnections, config.MaxConnections);
            var endPoint = Network.GetIpEndPoint(config.Address, config.Port);
            _listenSocket = new Socket(endPoint.AddressFamily, config.TransferType, config.UseProtocol);
            _listenSocket.Bind(endPoint);
        }

        public void Start()
        {
            _listenSocket.Listen(_config.MaxConnections);
            StartAccept(null);
            Logger.Log("Server try accept...");
        }

        public void Stop()
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

        public void Notify(Socket socket, bool isConnect)
        {
            if (socket == null)
                return;

            RemoveNullObservers();

            foreach (var observer in _observers)
                observer.GetConnectionEvent(socket, isConnect);
        }

        #endregion
    }
}