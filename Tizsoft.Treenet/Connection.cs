using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class Connection : IDisposable, INullObj, IConnectionSubject
    {
        SocketAsyncEventArgs _receiveAsyncArgs;
        SocketAsyncEventArgs _sendAsyncArgs;
        Socket _socket;
        readonly BufferManager _bufferManager;
        readonly IPacketContainer _packetContainer;
        readonly List<IConnectionObserver> _observers;

        void OnAsyncComplete(object sender, SocketAsyncEventArgs args)
        {
            Logger.Log(string.Format("async {0} complete with result {1}", args.LastOperation, args.SocketError));

            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ReceiveResult(args);
                    break;

                case SocketAsyncOperation.Send:
                    SendResult(args);
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
                    CloseAsyncSocket();
                    return;
                }
                if (args.BytesTransferred > 0)
                {
                    _packetContainer.AddPacket(this, args);
                    StartReceive();
                }
            }
            else
            {
                Dispose();
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.
        /// The method issues another receive on the socket to read any additional data sent from the client.
        /// </summary>
        void SendResult(SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Logger.Log(string.Format("already send <color=cyan>{0}</color> bytes msg to <color=cyan>{1}</color>", args.BytesTransferred, DestAddress));
            }
            else
            {
                Dispose();
            }
        }

        void StartReceive()
        {
            if (!_socket.ReceiveAsync(_receiveAsyncArgs))
                ReceiveResult(_receiveAsyncArgs);
        }

        void StartSend()
        {
            if (!_socket.SendAsync(_sendAsyncArgs))
                SendResult(_sendAsyncArgs);
        }

        void CloseAsyncSocket()
        {
            if (_socket == null)
            {
                return;
            }

            Notify(_socket, false);

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
            finally
            {
                _socket.Dispose();
                _socket = null;
            }
        }

        void InitSocketAsyncEventArgs(ref SocketAsyncEventArgs asyncArgs, BufferManager bufferManager)
        {
            asyncArgs = new SocketAsyncEventArgs();
            asyncArgs.Completed += OnAsyncComplete;

            if (bufferManager != null)
                bufferManager.SetBuffer(asyncArgs);
        }

        void ClearSocketAsyncEventArgs(ref SocketAsyncEventArgs asyncArgs)
        {
            if (_bufferManager != null)
                _bufferManager.FreeBuffer(asyncArgs);

            if (asyncArgs == null)
            {
                return;
            }

            try
            {
                asyncArgs.Dispose();
            }
            finally
            {
                asyncArgs = null;
            }
        }

        public Connection(BufferManager bufferManager, IPacketContainer packetContainer)
            : this()
        {
            _bufferManager = bufferManager;
            _packetContainer = packetContainer;
        }

        protected Connection()
        {
            DestAddress = string.Empty;
            _observers = new List<IConnectionObserver>();
        }

        public virtual void SetConnection(Socket socket)
        {
            _socket = socket;
            DestAddress = _socket.RemoteEndPoint.ToString();
            InitSocketAsyncEventArgs(ref _receiveAsyncArgs, _bufferManager);
            InitSocketAsyncEventArgs(ref _sendAsyncArgs, _bufferManager);
            StartReceive();
        }

        public virtual void Send(byte[] content)
        {
            _sendAsyncArgs.SetBuffer(_sendAsyncArgs.Offset, content.Length);
            Buffer.BlockCopy(content, 0, _sendAsyncArgs.Buffer, _sendAsyncArgs.Offset, content.Length);
            StartSend();
        }

        public string DestAddress { get; private set; }

        public static Connection NullConnection { get { return Treenet.NullConnection.Instance; } }

        #region IDisposable Members

        public virtual void Dispose()
        {
            CloseAsyncSocket();
            ClearSocketAsyncEventArgs(ref _receiveAsyncArgs);
            ClearSocketAsyncEventArgs(ref _sendAsyncArgs);
        }

        #endregion

        #region INullObj Members

        public virtual bool IsNull
        {
            get { return false; }
        }

        #endregion

        #region IConnectionSubject Members

        public virtual void Register(IConnectionObserver observer)
        {
            if (observer == null)
                return;

            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public virtual void Unregister(IConnectionObserver observer)
        {
            _observers.Remove(observer);
        }

        void RemoveNullObservers()
        {
            _observers.RemoveAll(observer => observer == null);
        }

        public virtual void Notify(Socket socket, bool isConnect)
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