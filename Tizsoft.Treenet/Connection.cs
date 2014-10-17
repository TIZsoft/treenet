using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    // TODO: “TCP does not operate on packets of data. TCP operates on streams of data.”
    // TODO: Message framing.
    // TODO: Current version possible cannot handle multiple connection requests efficiently.
    public class Connection : IConnection
    {
        static readonly IConnection NullConnection = new NullConnection();

        public static IConnection Null { get { return NullConnection; } }

        bool _isActive;

        readonly SocketAsyncEventArgs _receiveAsyncArgs;
        readonly PacketSender _packetSender;
        readonly IPacketContainer _packetContainer;
        readonly List<IConnectionObserver> _observers = new List<IConnectionObserver>();

        public bool IsNull { get { return false; } }

        public string DestAddress { get; private set; }

        public Socket ConnectSocket { get; private set; }

        void OnAsyncReceiveComplete(object sender, SocketAsyncEventArgs args)
        {
            GLogger.Debug(string.Format("async {0} complete with result {1}", args.LastOperation, args.SocketError));

            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ReceiveResult(args);
                    break;
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous receive operation completes.<br />
        /// If the remote host closed the socket, then the socket is closed.
        /// </summary>
        void ReceiveResult(SocketAsyncEventArgs args)
        {
            // Check if the remote host closed the socket.
            if (args.SocketError == SocketError.Success)
            {
                // Client has been disconnected.
                if (args.BytesTransferred == 0)
                {
                    Dispose();
                    return;
                }
                if (args.BytesTransferred > Network.PacketMinSize)
                {
                    var buffer = new byte[args.BytesTransferred];
                    Buffer.BlockCopy(args.Buffer, args.Offset, buffer, 0, args.BytesTransferred);
                    _packetContainer.ValidatePacket(this, buffer);
                }

                StartReceive();
            }
            else
            {
                Dispose();
            }
        }

        void StartReceive()
        {
            if (!ConnectSocket.ReceiveAsync(_receiveAsyncArgs))
                ReceiveResult(_receiveAsyncArgs);
        }

        void CloseConnectSocket()
        {
            if (ConnectSocket == null)
            {
                return;
            }

            try
            {
                ConnectSocket.Shutdown(SocketShutdown.Both);
                ConnectSocket.DisconnectAsync(_receiveAsyncArgs);
                ConnectSocket.Close();
                ConnectSocket.Dispose();
            }
            catch (Exception e)
            {
                GLogger.Error(e);
            }
            finally
            {
                ConnectSocket = null;
            }
        }

        public Connection(BufferManager bufferManager, IPacketContainer packetContainer, PacketSender packetSender)
            : this()
        {
            if (bufferManager == null)
                throw new ArgumentNullException("bufferManager");

            if (packetContainer == null)
                throw new ArgumentNullException("packetContainer");

            if (packetSender == null)
                throw new ArgumentNullException("packetSender");
            
            _receiveAsyncArgs = new SocketAsyncEventArgs();
            bufferManager.SetBuffer(_receiveAsyncArgs);
            _packetContainer = packetContainer;
            _packetSender = packetSender;
        }

        protected Connection()
        {
            DestAddress = string.Empty;
        }

        ~Connection()
        {
            Dispose();
        }

        public void SetConnection(Socket socket)
        {
            _isActive = true;
            ConnectSocket = socket;
            DestAddress = ConnectSocket.RemoteEndPoint.ToString();
            _receiveAsyncArgs.Completed += OnAsyncReceiveComplete;
            StartReceive();
        }

        public void Send(byte[] content, PacketType packetType)
        {
            _packetSender.SendMsg(this, content, packetType);
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (_isActive)
            {
                _isActive = false;
                _receiveAsyncArgs.Completed -= OnAsyncReceiveComplete;
                CloseConnectSocket();
                Notify(this, false);
            }
        }

        #endregion


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

        public void Notify(IConnection connection, bool isConnected)
        {
            foreach (var observer in _observers)
            {
                if (observer != null)
                    observer.GetConnectionEvent(connection, isConnected);
            }
        }

        #endregion
    }
}