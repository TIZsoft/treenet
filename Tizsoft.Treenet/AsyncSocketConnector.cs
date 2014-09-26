using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class AsyncSocketConnector : IConnectionSubject
    {
        readonly List<IConnectionObserver> _connectionObservers;
        SocketAsyncEventArgs _connectArgs;

        void OnConnectComplete(object sender, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ConnectResult(args);
                    break;
            }
        }

        void ConnectResult(SocketAsyncEventArgs args)
        {
            switch (args.SocketError)
            {
                case SocketError.Success:
                    Notify(args.AcceptSocket, true);
                    break;

                default:
                    Logger.Log(string.Format("因為 {0} ，所以無法連線", args.SocketError));
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

        public AsyncSocketConnector()
        {
            _connectionObservers = new List<IConnectionObserver>();
        }

        public void Connect(ClientConfig config)
        {
            InitConnectArgs(config);

            if (!_connectArgs.AcceptSocket.ConnectAsync(_connectArgs))
                ConnectResult(_connectArgs);
        }

        public void Stop()
        {
            if (_connectArgs != null)
                _connectArgs.Dispose();
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

        public void Notify(Socket socket, bool isConnect)
        {
            RemoveNullConnectionObservers();

            foreach (var connectionObserver in _connectionObservers)
            {
                connectionObserver.GetConnectionEvent(socket, isConnect);
            }
        }

        #endregion
    }
}