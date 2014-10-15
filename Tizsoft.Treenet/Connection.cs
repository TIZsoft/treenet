using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Log;
using Tizsoft.Security.Cryptography;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    // TODO: “TCP does not operate on packets of data. TCP operates on streams of data.”
    // TODO: Message framing.
    // TODO: Current version possible cannot handle multiple connection requests efficiently.
    // TODO: Implement async disconnect operation.
    public class Connection : IDisposable, INullObj, IConnectionSubject
    {
        bool _isActive = false;
        ICryptoProvider _crypto;

        readonly SocketAsyncEventArgs _receiveAsyncArgs;
        readonly PacketSender _packetSender;
        readonly IPacketContainer _packetContainer;
        readonly List<IConnectionObserver> _observers;

        void ValidatePacket(SocketAsyncEventArgs args)
        {
            var offset = args.Offset;

            if (_crypto != null)
                _crypto.Decrypt(args.Buffer, args.Offset, args.BytesTransferred);

            if (Network.HasValidHeader(args.Buffer, args.Offset, args.BytesTransferred))
            {
                offset += Network.CheckFlagSize;
                var compressionFlag = BitConverter.ToBoolean(args.Buffer, offset);
                offset += sizeof(bool);
                var packetType = Enum.IsDefined(typeof(PacketType), args.Buffer[offset]) ? (PacketType)args.Buffer[offset] : PacketType.Echo;
                offset += sizeof(byte);
                var contentSize = BitConverter.ToInt32(args.Buffer, offset);
                offset += sizeof(int);
                var contentBuffer = new byte[contentSize];

                // BUG: Expected an ArgumentException when a remote sent data more than buffer size.
                //       Buffer size is 512 but a remote send 1024 bytes in one send operation.
                Buffer.BlockCopy(args.Buffer, offset, contentBuffer, 0, contentSize);
                _packetContainer.AddPacket(this, contentBuffer, packetType);
            }
        }

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
            if (!ConnectSocket.ReceiveAsync(_receiveAsyncArgs))
                ReceiveResult(_receiveAsyncArgs);
        }

        void CloseAsyncSocket()
        {
            if (ConnectSocket == null)
            {
                return;
            }

            try
            {
                if (ConnectSocket != null)
                {
                    ConnectSocket.Shutdown(SocketShutdown.Both);
                    ConnectSocket.Dispose();
                }
            }
            catch (Exception e)
            {
                GLogger.Fatal(e);
            }
            finally
            {
                ConnectSocket = null;
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
            ConnectSocket = socket;
            DestAddress = ConnectSocket.RemoteEndPoint.ToString();
            _receiveAsyncArgs.Completed += OnAsyncReceiveComplete;
            StartReceive();
        }

        public virtual void Send(byte[] content, PacketType packetType)
        {
            _packetSender.SendMsg(this, content, packetType);
        }

        public string DestAddress { get; private set; }

        public static Connection NullConnection { get { return Treenet.NullConnection.Instance; } }

        public Socket ConnectSocket { get; private set; }

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
            // TODO: Possible occurs a performance issue when the server is accepted many connections.
            RemoveNullObservers();

            foreach (var observer in _observers)
                observer.GetConnectionEvent(connection, isConnect);
        }

        #endregion
    }
}