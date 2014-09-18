using System;
using System.Net.Sockets;
using TIZServer.Interface;
using TIZSoft;

namespace TIZServer
{
	public class NullTizConnection : TizConnection
	{
		private static NullTizConnection _instance;

		public static NullTizConnection Instance
		{
			get
			{
				if (_instance == null)
					_instance = new NullTizConnection();

				return _instance;
			}
		}

		NullTizConnection()
		{
		}

		public new void SetConnection(Socket socket, bool connected)
		{
			// Purposefully provides no behaviour.
			Logger.LogWarning("SetConnection in NullTizConnection");
		}

		public new bool IsConnected { get; private set; }

		#region IDisposable Members

		public new void Dispose()
		{
			// Purposefully provides no behaviour.
			Logger.LogWarning("Dispose in NullTizConnection");
		}

		#endregion

		#region INullObj Members

		public new bool IsNull
		{
			get { return true; }
		}

		#endregion
	}
}
