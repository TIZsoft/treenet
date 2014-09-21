using System.IO;
using TIZServer.Interface;
using TIZSoft;

namespace TIZServer
{
	public class ParseDefaultEchoPacket : IPacketParser
	{
		#region IPacketParser Members

		public void Parse(TizPacket packet)
		{
			Logger.Log("parse by default");

			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(TizNetwork.NetPacketHeader);
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