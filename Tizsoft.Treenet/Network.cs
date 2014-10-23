using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Tizsoft.Treenet
{
    public class Network
    {
        static byte[] _packetHeader;

        public const int DefaultClientMaxConn = 1;

        public const int DefaultServerMaxConn = 200;

        public const int DefaultServerListenNum = 200;

        public const int DefaultNetBufferSize = 1024;

        public const int DefaultPortNumber = 5566;

        public const double DefaultTimeOut = 5000f;

        public const double DefaultTimeOutTick = 1000f;

        public const string DefaultXorKey = "Tizsoft";

        public const int DefaultDatabaseKeepAlive = 1000;

        public static byte[] CheckFlags
        {
            get
            {
                if (_packetHeader == null)
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var writer = new BinaryWriter(stream))
                        {
                            //writer.Write(0x54);
                            writer.Write("鈦");
                            //writer.Write(0x33);
                            writer.Write("甲");
                            //writer.Write(0x23);
                            writer.Write("數");
                            //writer.Write(0x37);
                            writer.Write("位");
                            //writer.Write(0x02);
                            writer.Write("科");
                            //writer.Write(0x8770);
                            writer.Write("技");
                            //writer.Write(0x7592);
                            _packetHeader = stream.ToArray();
                        }
                    }
                }

                return _packetHeader;
            }
        }

        public static int CheckFlagSize { get { return CheckFlags.Length; } }

        /// <summary>
        /// PacketMinSize = CheckFlagSize + compression flag(bool) + packet type(byte) + content size(int)
        /// </summary>
        public static int PacketMinSize = CheckFlagSize + sizeof(bool) + sizeof(byte) + sizeof(int);

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
            var hostAddresses = Dns.GetHostAddresses(addressStr);

            foreach (var ipAddress in hostAddresses)
            {
                if (isIPv6 ? ipAddress.AddressFamily == AddressFamily.InterNetworkV6 : ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    return new IPEndPoint(ipAddress, port);
            }

            return new IPEndPoint(new IPAddress(new byte[] { 1, 0, 0, 127 }), 5566);
        }

        public static List<IPAddress> GetLocalIpAddresses(bool includeIpV6)
        {
            var addresses = new List<IPAddress>();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection)
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addresses
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    if (!includeIpV6 && address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1)
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    addresses.Add(address.Address);
                }
            }

            return addresses;
        }

        public static bool HasValidHeader(byte[] msg, int msgOffset, int msgCount)
        {
            return HasValidHeader(CheckFlags, msg, msgOffset, msgCount);
        }

        public static bool HasValidHeader(byte[] header, byte[] msg, int msgOffset, int msgCount)
        {
            var checkFlags = header == null ? CheckFlags : header;

            if (msgCount >= checkFlags.Length)
            {
                for (var i = 0; i < checkFlags.Length; i++)
                {
                    if (msg[i + msgOffset] != checkFlags[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        public static bool IsEmptyHeader(byte[] msg)
        {
            return (msg.Length > CheckFlagSize - 1 && msg[0] == 0 && msg[1] == 0 && msg[2] == 0 && msg[3] == 0);
        }
    }
}