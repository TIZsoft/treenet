using System.IO;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    public class ParseDefaultEchoPacket : IPacketParser
    {
        #region IPacketParser Members

        public void Parse(Packet packet)
        {
            Logger.Log("parse by default");

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Network.NetPacketHeader);
                    writer.Write((int)PacketType.Test);
                    writer.Write(packet.Content.Length);
                    writer.Write(packet.Content);
                    packet.Connection.Send(stream.ToArray());
                }
            }
        }

        #endregion
    }
}