using System;
using System.Collections.Generic;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketContainer : IPacketContainer
    {
        readonly Queue<IPacket> _queueingPackets = new Queue<IPacket>();
        readonly Queue<IPacket> _unusedPackets = new Queue<IPacket>();

        IPacket GetUnusedPacket()
        {
            return _unusedPackets.Count != 0 ? _unusedPackets.Dequeue() : new Packet();
        }


        #region IPacketContainer Members

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
                RecyclePacket(_queueingPackets.Dequeue());
        }

        public IPacket NextPacket()
        {
            return _queueingPackets.Count > 0 ? _queueingPackets.Dequeue() : Packet.Null;
        }

        #endregion
    }
}