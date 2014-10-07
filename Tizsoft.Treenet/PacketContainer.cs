using System.Collections.Generic;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketContainer : IPacketContainer
    {
        readonly Queue<Packet> _queueingPackets;
        readonly Queue<Packet> _unusedPackets;

        Packet GetUnusedPacket()
        {
            return _unusedPackets.Count != 0 ? _unusedPackets.Dequeue() : new Packet();
        }

        public PacketContainer()
        {
            _queueingPackets = new Queue<Packet>();
            _unusedPackets = new Queue<Packet>();
        }

        #region IPacketContainer Members

        public void AddPacket(Connection connection, byte[] contents, PacketType packetType)
        {
            var packet = GetUnusedPacket();
            packet.SetContent(connection, contents, packetType);
            _queueingPackets.Enqueue(packet);
        }

        public void RecyclePacket(Packet packet)
        {
            packet.Clear();

            if (!packet.IsNull)
                _unusedPackets.Enqueue(packet);
        }

        public void Clear()
        {
            while (_queueingPackets.Count > 0)
                RecyclePacket(_queueingPackets.Dequeue());
        }

        public Packet NextPacket()
        {
            return _queueingPackets.Count > 0 ? _queueingPackets.Dequeue() : Packet.NullPacket;
        }

        #endregion
    }
}