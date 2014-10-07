using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class Connection : IDisposable, INullObj, IConnectionSubject
    {
        Socket _socket;
        readonly SocketAsyncEventArgs _receiveAsyncArgs;
        readonly PacketSender _packetSender;
        readonly IPacketContainer _packetContainer;
        readonly List<IConnectionObserver> _observers;
        bool _isActive = false;

        void ValidatePacket(SocketAsyncEventArgs args)
        {
            var offset = args.Offset;

            if (Network.HasValidHeader(args.Buffer, args.Offset, args.BytesTransferred))
            {
                offset += Network.CheckFlagSize;
                var compressionFlag = BitConverter.ToBoolean(args.Buffer, offset);
                offset += sizeof (bool);
                var packetType = Enum.IsDefined(typeof(PacketType), args.Buffer[offset]) ? (PacketType)args.Buffer[offset] : PacketType.Echo;
                offset += sizeof (byte);
                var contentSize = BitConverter.ToInt32(args.Buffer, offset);
                offset += sizeof (int);
                var contentBuffer = new byte[contentSize];
                Buffer.BlockCopy(args.Buffer, offset, contentBuffer, 0, contentSize);
                _packetContainer.AddPacket(this, contentBuffer, packetType);
            }
        }

        void OnAsyncReceiveComplete(object sender, SocketAsyncEventArgs args)
        {
            Logger.Log(string.Format("async {0} complete with result {1}", args.LastOperation, args.SocketError));

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
                    ValidatePacket(args);
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
            if (!_socket.ReceiveAsync(_receiveAsyncArgs))
                ReceiveResult(_receiveAsyncArgs);
        }

        void CloseAsyncSocket()
        {
            if (_socket == null)
            {
                return;
            }

            try
            {
                if (_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
            finally
            {
                _socket = null;
            }
        }

        void InitSocketAsyncEventArgs(ref SocketAsyncEventArgs asyncArgs, BufferManager bufferManager)
        {
            asyncArgs = new SocketAsyncEventArgs();

            if (bufferManager != null)
                bufferManager.SetBuffer(asyncArgs);
        }

        public Connection(BufferManager bufferManager, IPacketContainer packetContainer, PacketSender packetSender)
            : this()
        {
            InitSocketAsyncEventArgs(ref _receiveAsyncArgs, bufferManager);
            _packetContainer = packetContainer;
            _packetSender = packetSender;
        }

        protected Connection()
        {
            DestAddress = string.Empty;
            _observers = new List<IConnectionObserver>();
        }

        ~Connection()
        {
            Dispose();
        }

        public virtual void SetConnection(Socket socket)
        {
            _isActive = true;
            _socket = socket;
            DestAddress = _socket.RemoteEndPoint.ToString();
            _receiveAsyncArgs.Completed += OnAsyncReceiveComplete;
            StartReceive();
        }

        public virtual void Send(byte[] content, PacketType packetType)
        {
            _packetSender.SendMsg(this, content, packetType);
        }

        public string DestAddress { get; private set; }

        public static Connection NullConnection { get { return Treenet.NullConnection.Instance; } }

        public Socket Connector {get { return _socket; }}

        #region IDisposable Members

        public virtual void Dispose()
        {
            if (_isActive)
            {
                _receiveAsyncArgs.Completed -= OnAsyncReceiveComplete;
                _isActive = false;
                CloseAsyncSocket();
                Notify(this, false);
            }
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

        public virtual void Notify(Connection connection, bool isConnect)
        {
            RemoveNullObservers();

            foreach (var observer in _observers)
                observer.GetConnectionEvent(connection, isConnect);
        }

        #endregion
    }
}