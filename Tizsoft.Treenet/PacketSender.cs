using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Tizsoft.Collections;
using Tizsoft.Helpers;
using Tizsoft.Log;
using Tizsoft.Security.Cryptography;
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

        volatile object _syncRoot = new object();
        FixedSizeObjPool<SocketAsyncEventArgs> _asyncSendOpPool;
        HashSet<SocketAsyncEventArgs> _workingAsyncSendOps;
        readonly ConcurrentQueue<SendOperand> _sendQueue = new ConcurrentQueue<SendOperand>();
        byte[] _sendBuffer;

        readonly IPacketContainer _sendPacketContainer = new PacketContainer();

        public PacketProtocol PacketProtocol { get; set; }

        void OnAsyncSendCompleted(object sender, SocketAsyncEventArgs socketOperation)
        {
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
                GLogger.Debug("sent <color=cyan>{0}</color> bytes msg to <color=cyan>{1}</color>",
                    sendOperation.BytesTransferred, connection.DestAddress);
            }
            else
            {
                GLogger.Error("send msg to <color=cyan>{0}</color> failed due to <color=cyan>{1}</color>",
                    connection.DestAddress, sendOperation.SocketError);
                connection.Dispose();
            }

            sendOperation.UserToken = null;
            sendOperation.AcceptSocket = null;

            lock (_asyncSendOpPool)
                _asyncSendOpPool.Push(sendOperation);

            lock (_workingAsyncSendOps)
                _workingAsyncSendOps.Remove(sendOperation);

            StartSend();
        }

        // TODO: “TCP does not operate on packets of data. TCP operates on streams of data.”
        // TODO: Message framing.
        void StartSend()
        {
            SendOperand sendOperand;
            if (!_sendQueue.TryDequeue(out sendOperand))
            {
                return;
            }

            // TODO: To be reviwed.
            var spin = new SpinWait();
            while (true)
            {
                //check if there is any available async send operation first, then pop from send queue,
                //otherwise packet will lost when pop first and there is no available async send operation.
                SocketAsyncEventArgs asyncSendOperation;

                lock (_asyncSendOpPool)
                {
                    if (!_asyncSendOpPool.TryPop(out asyncSendOperation))
                    {
                        spin.SpinOnce();
                        continue;
                    }
                }

                Debug.Assert(asyncSendOperation != null);

                asyncSendOperation.SetBuffer(asyncSendOperation.Offset, sendOperand.Message.Length);
                Array.Copy(sendOperand.Message, 0, asyncSendOperation.Buffer, asyncSendOperation.Offset, sendOperand.Message.Length);
                asyncSendOperation.UserToken = sendOperand.Connection;

                lock (_workingAsyncSendOps)
                {
                    _workingAsyncSendOps.Add(asyncSendOperation);
                }

                var connectSocket = sendOperand.Connection.ConnectSocket;

                if (connectSocket == null)
                {
                    GLogger.Error("You're trying to send a message but socket is null.");
                    break;
                }

                try
                {
                    var willRaiseEvent = connectSocket.SendAsync(asyncSendOperation);

                    if (willRaiseEvent)
                    {
                        break;
                    }

                    ProcessSend(asyncSendOperation);
                }
                catch (Exception e)
                {
                    GLogger.Error(e);
                    sendOperand.Connection.Dispose();
                }

                break;
            }
        }

        void FreeWorkingAsyncSendOps()
        {
            if (_workingAsyncSendOps == null)
                return;

            foreach (var sendOperation in _workingAsyncSendOps)
            {
                if (sendOperation == null)
                    continue;

                try
                {
                    if (sendOperation.AcceptSocket != null)
                    {
                        sendOperation.AcceptSocket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (Exception e)
                {
                    GLogger.Error(e);
                    throw;
                }
                finally
                {
                    if (sendOperation.AcceptSocket != null)
                    {
                        sendOperation.AcceptSocket.Close();
                        sendOperation.AcceptSocket = null;
                    }

                    sendOperation.UserToken = null;
                    _asyncSendOpPool.Push(sendOperation);
                }
            }

            _workingAsyncSendOps.Clear();
        }

        public void Setup(BufferManager bufferManager, int asyncCount, ICryptoProvider crypto)
        {
            _sendBuffer = new byte[bufferManager.BufferSize];
            _sendPacketContainer.Clear();
            FreeWorkingAsyncSendOps();

            asyncCount = Math.Max(1, asyncCount);
            _asyncSendOpPool = new FixedSizeObjPool<SocketAsyncEventArgs>(asyncCount);

            for (var i = 0; i < asyncCount; ++i)
            {
                var asyncSend = new SocketAsyncEventArgs();
                bufferManager.SetBuffer(asyncSend);
                asyncSend.Completed += OnAsyncSendCompleted;
                _asyncSendOpPool.Push(asyncSend);
            }

            _workingAsyncSendOps = new HashSet<SocketAsyncEventArgs>();
        }

        // TODO: Implements SendPacket method.
        public void SendMsg(IConnection connection, byte[] msg, PacketType packetType)
        {
            var packet = new Packet
            {
                Connection = connection,
                PacketType = packetType,
                Content = msg,
            };

            byte[] message;
            if (PacketProtocol.TryWrapPacket(packet, out message))
            {
                message = MessageFraming.WrapMessage(message);

                //_sendPacketContainer.AddPacket(connection, msg, packetType);
                var splitedMessages = Utils.SplitArray(message, _sendBuffer.Length);
                foreach (var splitedMessage in splitedMessages)
                {
                    var operand = new SendOperand
                    {
                        Connection = connection,
                        Message = splitedMessage
                    };
                    _sendQueue.Enqueue(operand);
                }
                StartSend();
            }
            else
            {
                GLogger.Error("Packet wrapping failure.");
            }
        }
    }
}