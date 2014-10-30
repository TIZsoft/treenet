using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.PacketParser
{
    public class ParseDefaultEchoPacket : IPacketProcessor
    {
        #region IPacketProcessor Members

        public void Process(IPacket packet)
        {
            GLogger.Debug((object) "parse by default: echo back!");
            packet.Connection.Send(packet.Content, packet.PacketType);
        }

        #endregion
    }
}