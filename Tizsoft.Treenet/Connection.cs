using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    // TODO: Reuse packets.
    public class Connection : IConnection
    {
        static readonly IConnection NullConnection = new NullConnection();

        public static IConnection Null { get { return NullConnection; } }

        //bool _isActive;

        readonly MessageFraming _messageFraming;
        readonly SocketAsyncEventArgs _socketOperation;
        readonly PacketSender _packetSender;
        readonly IPacketContainer _packetContainer;
        readonly HashSet<IConnectionObserver> _observers = new HashSet<IConnectionObserver>();

        public bool IsNull { get { return false; } }

        public string DestAddress { get; private set; }

        public Socket ConnectSocket { get; private set; }

        public PacketProtocol PacketProtocol { get; set; }

        public double IdleTime { get; set; }

        void OnAsyncReceiveCompleted(object sender, SocketAsyncEventArgs socketOperation)
        {
            GLogger.Debug("async {0} complete with result {1}", socketOperation.LastOperation, socketOperation.SocketError);

            switch (socketOperation.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(socketOperation);
                    break;
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous receive operation completes.<br />
        /// If the remote host closed the socket, then the socket is closed.
        /// </summary>
        void ProcessReceive(SocketAsyncEventArgs receiveOperation)
        {
            // Check if the remote host closed the socket.
            if (receiveOperation.SocketError == SocketError.Success)
            {
                // Client has been disconnected.
                if (receiveOperation.BytesTransferred == 0)
                {
                    Dispose();
                    return;
                }

                IdleTime = 0;
                var buffer = new byte[receiveOperation.BytesTransferred];
                Array.Copy(receiveOperation.Buffer, receiveOperation.Offset, buffer, 0, receiveOperation.BytesTransferred);
                _messageFraming.DataReceived(buffer);

                StartReceive();
            }
            else
            {
                GLogger.Error(receiveOperation.SocketError);
                Dispose();
            }
        }

        void StartReceive()
        {
            var willRaiseEvent = ConnectSocket.ReceiveAsync(_socketOperation);

            if (willRaiseEvent)
            {
                return;
            }

            ProcessReceive(_socketOperation);
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
            }
            catch (Exception e)
            {
                GLogger.Error(e);
            }
            finally
            {
                ConnectSocket.Close(0);
                ConnectSocket = null;
            }
        }

        public Connection(BufferManager bufferManager, IPacketContainer packetContainer, PacketSender packetSender, int maxMessageSize)
        {
            if (bufferManager == null)
                throw new ArgumentNullException("bufferManager");

            if (packetContainer == null)
                throw new ArgumentNullException("packetContainer");

            if (packetSender == null)
                throw new ArgumentNullException("packetSender");

            DestAddress = string.Empty;
            _socketOperation = new SocketAsyncEventArgs();
            bufferManager.SetBuffer(_socketOperation);
            _packetContainer = packetContainer;
            _packetSender = packetSender;
            _messageFraming = new MessageFraming(maxMessageSize);
            _messageFraming.MessageArrived += OnMessageArrived;
        }

        void OnMessageArrived(object sender, MessageArrivedEventArgs e)
        {
            if (PacketProtocol == null)
            {
                throw new InvalidOperationException("PacketProtocol is null.");
            }

            // TODO: Reuse packets.
            IPacket packet;
            if (PacketProtocol.TryParsePacket(e.Message, out packet))
            {
                packet.Connection = this;
                _packetContainer.AddPacket(packet);
            }
            else
            {
                // Invalid packet.
                GLogger.Error("Invalid packet.");
                Dispose();
            }
        }

        ~Connection()
        {
            Dispose();
            _messageFraming.MessageArrived -= OnMessageArrived;
            _socketOperation.Dispose();
        }

        public void SetConnection(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }

            IsActive = true;
            ConnectSocket = socket;
            DestAddress = socket.RemoteEndPoint.ToString();
            _socketOperation.Completed += OnAsyncReceiveCompleted;
            StartReceive();
        }

        public void Send(byte[] content, PacketType packetType)
        {
            Send(content, PacketFlags.None, packetType);
        }

        public void Send(byte[] content, PacketFlags packetFlags, PacketType packetType)
        {
            if (IsActive)
            {
                IdleTime = 0;
                _packetSender.SendMsg(this, content, packetType);
            }
        }

        public bool IsActive { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (IsActive)
            {
                IsActive = false;
                IdleTime = 0;
                _socketOperation.Completed -= OnAsyncReceiveCompleted;
                _messageFraming.Clear();
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