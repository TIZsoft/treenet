using System.Net.Sockets;
using TIZServer.Interface;

namespace TIZServer
{
	public class NullTizPacket : TizPacket, INullObj
	{
		private static NullTizPacket _instance;

		public static NullTizPacket Instance
		{
			get
			{
				if (_instance == null)
					_instance = new NullTizPacket();

				return _instance;
			}
		}

		NullTizPacket()
		{
			Connection = NullTizConnection.Instance;
		}

		public new void SetContent(TizConnection connection, SocketAsyncEventArgs asyncArgs)
		{
		}

		public new void Clear()
		{
		}

		public new byte[] Content { get { return null; } }
		public new TizConnection Connection { get; private set; }

		#region INullObj Members

		public new bool IsNull
		{
			get { return true; }
		}

		#endregion
	}
}
