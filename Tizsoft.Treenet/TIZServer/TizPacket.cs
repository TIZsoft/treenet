using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using TIZServer.Interface;
using TIZSoft;

namespace TIZServer
{
	public enum PacketType : int
	{
		Test = -1,
		Undefine = 0,
	}

	public class TizPacket : INullObj
	{
		private MemoryStream _buffer;
		private byte[] _header;
		private int _packetType;
		private byte[] _contents;

		public void SetContent(TizConnection connection, SocketAsyncEventArgs asyncArgs)
		{
			if (_buffer == null)
				_buffer = new MemoryStream();

			_buffer.Write(asyncArgs.Buffer, asyncArgs.Offset, asyncArgs.Count);
			_buffer.Seek(0, SeekOrigin.Begin);

			try
			{
				using (BinaryReader reader = new BinaryReader(_buffer, Encoding.UTF8, true))
				{
					_header = reader.ReadBytes(TizNetwork.PacketHeaderSize);
					_packetType = reader.ReadInt32();
					int length = reader.ReadInt32();
					_contents = reader.ReadBytes(length);
				}
			}
			catch (Exception e)
			{
				Logger.LogException(e);
			}
			finally
			{
				_buffer.SetLength(0);
			}

			Connection = connection;
		}

		public void Clear()
		{
			_buffer.SetLength(0);
			Array.Clear(_header, 0, _header.Length);
			Array.Clear(_contents, 0, _contents.Length);
			Connection = TizConnection.NullConnection;
		}

		public byte[] Header {get { return _header; }}

		public PacketType PacketType
		{
			get
			{
				return Enum.IsDefined(typeof (PacketType), _packetType) ? (PacketType) _packetType : PacketType.Undefine;
			}
		}
		public byte[] Content { get { return _contents; } }
		public TizConnection Connection { get; private set; }

		#region INullObj Members

		public bool IsNull
		{
			get { return false; }
		}

		#endregion
	}
}