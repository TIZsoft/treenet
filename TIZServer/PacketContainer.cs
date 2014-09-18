using System.Collections.Generic;
using System.Net.Sockets;
using TIZServer;
using TIZServer.Interface;

public class PacketContainer : IPacketContainer
{
	public PacketContainer()
	{
		_waitToParsePackets = new Queue<TizPacket>();
		_unusedPackets = new Queue<TizPacket>();
	}

	Queue<TizPacket> _waitToParsePackets;
	Queue<TizPacket> _unusedPackets;

	TizPacket GetUnusedPacket()
	{
		TizPacket unusedPacket;

		if (_unusedPackets.Count != 0)
			unusedPacket = _unusedPackets.Dequeue();
		else
			unusedPacket = new TizPacket();
		
		return unusedPacket;
	}

	#region IPacketContainer Members

	public void AddPacket(TizConnection connection, SocketAsyncEventArgs asyncArgs)
	{
		TizPacket packet = GetUnusedPacket();
		packet.SetContent(connection, asyncArgs);
		_waitToParsePackets.Enqueue(packet);
	}

	public void RecyclePacket(TizPacket packet)
	{
		if (packet != null)
		{
			packet.Clear();
			_unusedPackets.Enqueue(packet);	
		}
	}

	public TizPacket NextPacket()
	{
		return _waitToParsePackets.Count > 0 ? _waitToParsePackets.Dequeue() : NullPacket;
	}

	#endregion

	public static TizPacket NullPacket { get { return NullTizPacket.Instance; } }
}