using System;
using System.Net;
using System.Net.Sockets;

namespace Tizsoft.Treenet
{
    public class Network
    {
        public const int DefaultClientMaxConn = 1;

        public const int DefaultServerMaxConn = 200;

        public const int DefaultServerListenNum = 200;

        public const int DefaultNetBufferSize = 1024;

        public const int DefaultPortNumber = 5566;

        public static readonly byte[] NetPacketHeader = { 0x54, 0x33, 0x23, 0x37 };

        public static int PacketHeaderSize { get { return NetPacketHeader.Length; } }

        public static int ProtocolTypeSize { get { return sizeof(int); } }

        public static int PacketLengthTypeSize { get { return sizeof(int); } }

        public static int PacketMinSize = PacketHeaderSize + ProtocolTypeSize + PacketLengthTypeSize;

        /// <summary>
        /// 建立 keepalive 作業所需的輸入資料
        /// </summary>
        /// <param name="onOff">是否啟用1:on ,0:off</param>
        /// <param name="keepAliveTime">client靜止多久後才開始送偵測訊息(millisecond)</param>
        /// <param name="keepAliveInterval">偵測間隔(millisecond)</param>
        /// <returns></returns>
        public static byte[] GetKeepAliveSetting(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            var buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }

        public static IPEndPoint GetIpEndPoint(string addressStr, int port, bool isIPv6 = false)
        {
            var entry = Dns.GetHostEntry(addressStr);

            foreach (var addr in entry.AddressList)
            {
                if (isIPv6 ? addr.AddressFamily == AddressFamily.InterNetworkV6 : addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return new IPEndPoint(addr, port);
                }
            }

            return new IPEndPoint(new IPAddress(new byte[] { 1, 0, 0, 127 }), 5566);
        }

        public static bool HasValidHeader(byte[] msg)
        {
            return (msg.Length > PacketHeaderSize - 1 && msg[0] == NetPacketHeader[0] && msg[1] == NetPacketHeader[1] &&
                    msg[2] == NetPacketHeader[2] && msg[3] == NetPacketHeader[3]);
        }

        public static bool IsEmptyHeader(byte[] msg)
        {
            return (msg.Length > PacketHeaderSize - 1 && msg[0] == 0 && msg[1] == 0 && msg[2] == 0 && msg[3] == 0);
        }
    }
}