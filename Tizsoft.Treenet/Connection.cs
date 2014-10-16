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
    public class Connection : IConnection
    {
        static readonly IConnection NullConnection = new NullConnection();

        public static IConnection Null { get { return NullConnection; } }

        bool _isActive;
        ICryptoProvider _crypto;

        readonly SocketAsyncEventArgs _receiveAsyncArgs;
        readonly PacketSender _packetSender;
        readonly IPacketContainer _packetContainer;
        readonly List<IConnectionObserver> _observers = new List<IConnectionObserver>();

        public bool IsNull { get { return false; } }

        public string DestAddress { get; private set; }

        public Socket ConnectSocket { get; private set; }

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
                //       Buffer size is 512 but a remote sent 1024 bytes in one send operation.
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

        void CloseConnectSocket()
        {
            if (ConnectSocket == null)
            {
                return;
            }

            try
            {
                ConnectSocket.Shutdown(SocketShutdown.Both);
                ConnectSocket.Close();
            }
            catch (Exception e)
            {
                // TODO: Connect socket has already closed/disposed. Log level can be ERROR or do not log it.
                GLogger.Fatal(e);
            }
            finally
            {
                ConnectSocket.Dispose();
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

        void RemoveNullObservers()
        {
            _observers.RemoveAll(observer => observer == null);
        }

        public void Notify(IConnection connection, bool isConnected)
        {
            // TODO: Possible occurs a performance issue when the server is accepted many connections.
            RemoveNullObservers();

            foreach (var observer in _observers)
                observer.GetConnectionEvent(connection, isConnected);
        }

        #endregion
    }
}