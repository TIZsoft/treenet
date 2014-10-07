using System.IO;
using Tizsoft.Log;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.PacketParser
{
    public class ParseDefaultEchoPacket : IPacketProcessor
    {
        #region IPacketProcessor Members

        public void Process(Packet packet)
        {
            Logger.Log("parse by default: echo back!");
            packet.Connection.Send(packet.Content, packet.PacketType);
        }

        #endregion
    }
}