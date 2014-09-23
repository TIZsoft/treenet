using System;
using System.Collections.Generic;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class PacketHandler
    {
        readonly Dictionary<PacketType, List<IPacketParser>> _parsers;

        IPacketParser CreatePacketParser(PacketType type)
        {
            switch (type)
            {
                default:
                    return new ParseDefaultEchoPacket();
            }
        }

        void InitPacketParser()
        {
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                AddParser(type, CreatePacketParser(type));
            }
        }

        public PacketHandler()
        {
            _parsers = new Dictionary<PacketType, List<IPacketParser>>();
            InitPacketParser();
        }

        public void Parse(Packet packet)
        {
            List<IPacketParser> parsers;

            if (_parsers.TryGetValue(packet.PacketType, out parsers))
            {
                foreach (var parser in parsers)
                {
                    if (parser != null)
                        parser.Parse(packet);
                }
            }
        }

        public void AddParser(PacketType packetType, IPacketParser parser)
        {
            if (parser == null)
                return;

            List<IPacketParser> parsers;

            if (!_parsers.TryGetValue(packetType, out parsers))
            {
                parsers = new List<IPacketParser>();
                _parsers.Add(packetType, parsers);
            }

            if (!parsers.Contains(parser))
                parsers.Add(parser);
        }

        public void RemoveParser(PacketType packetType, IPacketParser parser)
        {
            List<IPacketParser> parsers;

            if (_parsers.TryGetValue(packetType, out parsers))
                parsers.Remove(parser);
        }
    }
}