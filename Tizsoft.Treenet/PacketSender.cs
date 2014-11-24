using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using Tizsoft.Collections;
using Tizsoft.Helpers;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketSender
    {
        struct SendOperand
        {
            public IConnection Connection { get; set; }

            public byte[] Message { get; set; }
        }

        FixedSizeObjPool<SocketAsyncEventArgs> _asyncSendOpPool;
        readonly ConcurrentQueue<SendOperand> _sendQueue = new ConcurrentQueue<SendOperand>();
        int _segmentSize;

        public PacketProtocol PacketProtocol { get; set; }

        void OnAsyncSendCompleted(object sender, SocketAsyncEventArgs socketOperation)
        {
            Debug.Assert(socketOperation.LastOperation == SocketAsyncOperation.Send);

            switch (socketOperation.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    ProcessSend(socketOperation);
                    break;
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.
        /// The method issues another receive on the socket to read any additional data sent from the client.
        /// </summary>
        void ProcessSend(SocketAsyncEventArgs sendOperation)
        {
            var connection = (Connection)sendOperation.UserToken;

            if (sendOperation.SocketError == SocketError.Success)
            {
                GLogger.Debug(string.Format("sent <color=cyan>{0}</color> bytes msg to <color=cyan>{1}</color>",
                    sendOperation.BytesTransferred, connection.DestAddress));
            }
            else
            {
                GLogger.Error(string.Format("send msg to <color=cyan>{0}</color> failed due to <color=cyan>{1}</color>",
                    connection.DestAddress, sendOperation.SocketError));
                connection.Dispose();
            }

            sendOperation.UserToken = null;
            sendOperation.AcceptSocket = null;
            
            StartSend(sendOperation);
        }

        void StartSend(SocketAsyncEventArgs asyncSendOperation)
        {
            // Check if there is any available async send operation first, then pop from send queue,
            // Otherwise packet will lost when pop first and there is no available async send operation.
            if (asyncSendOperation == null)
            {
                return;
            }

            SendOperand sendOperand;
            if (!_sendQueue.TryDequeue(out sendOperand))
            {
                _asyncSendOpPool.Push(asyncSendOperation);
                return;
            }

            asyncSendOperation.SetBuffer(asyncSendOperation.Offset, sendOperand.Message.Length);
            Array.Copy(sendOperand.Message, 0, asyncSendOperation.Buffer, asyncSendOperation.Offset, sendOperand.Message.Length);
            asyncSendOperation.UserToken = sendOperand.Connection;

            var connectSocket = sendOperand.Connection.ConnectSocket;

            if (connectSocket == null)
            {
                GLogger.Error("You're trying to send a message but socket is null.");
                return;
            }

            try
            {
                var willRaiseEvent = connectSocket.SendAsync(asyncSendOperation);

                if (willRaiseEvent)
                {
                    return;
                }

                ProcessSend(asyncSendOperation);
            }
            catch (Exception e)
            {
                GLogger.Error(e);
                sendOperand.Connection.Dispose();
            }
        }

        public void Setup(BufferManager bufferManager, int asyncCount)
        {
            asyncCount = Math.Max(1, asyncCount);
            _asyncSendOpPool = new FixedSizeObjPool<SocketAsyncEventArgs>(asyncCount);
            _segmentSize = bufferManager.SegmentSize;
            for (var i = 0; i < asyncCount; ++i)
            {
                var asyncSend = new SocketAsyncEventArgs();
                bufferManager.SetBuffer(asyncSend);
                asyncSend.Completed += OnAsyncSendCompleted;
                _asyncSendOpPool.Push(asyncSend);
            }
        }

        public void SendMsg(IConnection connection, byte[] msg, PacketType packetType)
        {
            var packet = new Packet
            {
                Connection = connection,
                PacketType = packetType,
                Content = msg,
            };

            SendMsg(packet);
        }

        public void SendMsg(IPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            byte[] message;
            if (PacketProtocol.TryWrapPacket(packet, out message))
            {
                message = MessageFraming.WrapMessage(message);

                var splitedMessages = Utils.SplitArray(message, _segmentSize);
                foreach (var splitedMessage in splitedMessages)
                {
                    var operand = new SendOperand
                    {
                        Connection = packet.Connection,
                        Message = splitedMessage
                    };
                    _sendQueue.Enqueue(operand);
                }

                SocketAsyncEventArgs sendOperation;
                if (_asyncSendOpPool.TryPop(out sendOperation))
                {
                    StartSend(sendOperation);
                }
            }
            else
            {
                GLogger.Error("Packet wrapping failure.");
            }
        }
    }
}