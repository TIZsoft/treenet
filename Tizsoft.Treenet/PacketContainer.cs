using System;
using System.Collections.Concurrent;
using Tizsoft.Security.Cryptography;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketContainer : IPacketContainer
    {
        readonly ConcurrentQueue<IPacket> _queueingPackets = new ConcurrentQueue<IPacket>();
        readonly ConcurrentQueue<IPacket> _unusedPackets = new ConcurrentQueue<IPacket>();
        ICryptoProvider _crypto;

        IPacket GetUnusedPacket()
        {
            IPacket packet;
            return _unusedPackets.TryDequeue(out packet) ? packet : new Packet();
        }

        #region IPacketContainer Members

        public void Setup(ICryptoProvider crypto)
        {
            _crypto = crypto;
        }

        public void AddPacket(IConnection connection, byte[] contents, PacketType packetType)
        {
            var packet = GetUnusedPacket();
            packet.SetContent(connection, contents, packetType);
            _queueingPackets.Enqueue(packet);
        }

        public void RecyclePacket(IPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            packet.Clear();

            if (packet != Packet.Null)
                _unusedPackets.Enqueue(packet);
        }

        public void Clear()
        {
            while (_queueingPackets.Count > 0)
            {
                IPacket packet;
                if (_queueingPackets.TryDequeue(out packet))
                {
                    RecyclePacket(packet);
                }
            }
        }

        public void ValidatePacket(IConnection connection, byte[] buffer)
        {
            var bufferPos = 0;

            if (_crypto != null)
                buffer = _crypto.Decrypt(buffer);

            if (Network.HasValidHeader(buffer, 0, buffer.Length))
            {
                bufferPos += Network.CheckFlagSize;
                var compressionFlag = BitConverter.ToBoolean(buffer, bufferPos);
                bufferPos += sizeof(bool);
                var packetType = Enum.IsDefined(typeof(PacketType), buffer[bufferPos]) ? (PacketType)buffer[bufferPos] : PacketType.Echo;
                bufferPos += sizeof(byte);
                var contentSize = BitConverter.ToInt32(buffer, bufferPos);
                bufferPos += sizeof(int);

                if (contentSize + bufferPos <= buffer.Length)
                {
                    var contentBuffer = new byte[contentSize];
                    Buffer.BlockCopy(buffer, bufferPos, contentBuffer, 0, contentSize);
                    AddPacket(connection, contentBuffer, packetType);
                }
            }
        }

        public IPacket NextPacket()
        {
            IPacket packet;
            return _queueingPackets.TryDequeue(out packet) ? packet : Packet.Null;
        }

        #endregion
    }
}