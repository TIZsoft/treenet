using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Tizsoft.Collections;
using Tizsoft.Log;
using Tizsoft.Security.Cryptography;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketSender
    {
        FixedSizeObjPool<SocketAsyncEventArgs> _asyncSendOpPool;
        List<SocketAsyncEventArgs> _workingAsyncSendOps;
        readonly IPacketContainer _sendPacketContainer = new PacketContainer();
        byte[] _sendBuffer;
        ICryptoProvider _crypto;
        int _bufferSize;

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
                GLogger.Debug("sent <color=cyan>{0}</color> bytes msg to <color=cyan>{1}</color>", sendOperation.BytesTransferred, connection.DestAddress);
            }
            else
            {
                GLogger.Error("send msg to <color=cyan>{0}</color> failed due to <color=cyan>{1}</color>", connection.DestAddress, sendOperation.SocketError);
                connection.Dispose();
            }

            sendOperation.UserToken = null;
            sendOperation.AcceptSocket = null;
            _asyncSendOpPool.Push(sendOperation);
            _workingAsyncSendOps.Remove(sendOperation);
            StartSend();
        }

        // TODO: “TCP does not operate on packets of data. TCP operates on streams of data.”
        // TODO: Message framing.
        void StartSend()
        {
            //check if there is any available async send operation first, then pop from send queue,
            //otherwise packet will lost when pop first and there is no available async send operation.
            if (_asyncSendOpPool.Count == 0)
                return;

            var packet = _sendPacketContainer.NextPacket();

            if (packet.IsNull ||
                packet.Connection.IsNull)
                return;

            int msgSize;

            using (var stream = new MemoryStream(_sendBuffer))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Network.CheckFlags);
                    writer.Write(false);  //compression now set to false
                    writer.Write((byte)packet.PacketType);
                    var content = packet.Content;

                    if (content.Length + stream.Position <= _sendBuffer.Length)
                    {
                        writer.Write(content.Length);
                        writer.Write(content);
                    }

                    msgSize = (int)stream.Position;    
                }
            }

            if (_crypto != null)
                _sendBuffer = _crypto.Encrypt(_sendBuffer, 0, msgSize);

            // BUG: Possible expected an InvalidOperationException when send operation called too many times at same time.
            var asyncSendOperation = _asyncSendOpPool.Pop();
            asyncSendOperation.SetBuffer(asyncSendOperation.Offset, msgSize);
            Buffer.BlockCopy(_sendBuffer, 0, asyncSendOperation.Buffer, asyncSendOperation.Offset, msgSize);
            asyncSendOperation.UserToken = packet.Connection;
            _workingAsyncSendOps.Add(asyncSendOperation);
            
            var connector = packet.Connection.ConnectSocket;

            try
            {
                var willRaiseEvent = connector.SendAsync(asyncSendOperation);

                if (willRaiseEvent)
                {
                    return;
                }

                ProcessSend(asyncSendOperation);
            }
            catch (Exception)
            {
                packet.Connection.Dispose();
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
                        sendOperation.AcceptSocket.Close();
                    }
                }
                catch (Exception e)
                {
                    GLogger.Error(e);
                    throw;
                }
                finally
                {
                    sendOperation.UserToken = null;
                    sendOperation.AcceptSocket = null;
                    _asyncSendOpPool.Push(sendOperation);
                }
            }

            _workingAsyncSendOps.Clear();
        }

        public void Setup(BufferManager bufferManager, int asyncCount, ICryptoProvider crypto)
        {
            _crypto = crypto;
            _bufferSize = bufferManager.BufferSize;
            _sendBuffer = new byte[_bufferSize];
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

            _workingAsyncSendOps = new List<SocketAsyncEventArgs>(asyncCount);
        }

        public void SendMsg(IConnection connection, byte[] msg, PacketType packetType)
        {
            _sendPacketContainer.AddPacket(connection, msg, packetType);
            StartSend();
        }
    }
}