using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public const int DefaultTimeOut = 5000;

        public const double DefaultTimeOutTick = 1000;

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
                            writer.Write("鈦甲數位科技");
                            _packetHeader = stream.ToArray();
                        }
                    }
                }

                return _packetHeader;
            }
        }

        public static int CheckFlagSize { get { return CheckFlags.Length; } }

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

        public static IPEndPoint GetIpEndPoint(string hostNameOrAddress, int port, bool isIPv6 = false)
        {
            var hostAddresses = Dns.GetHostAddresses(hostNameOrAddress);

            foreach (var ipAddress in hostAddresses.Where(ipAddress => isIPv6 ? ipAddress.AddressFamily == AddressFamily.InterNetworkV6 : ipAddress.AddressFamily == AddressFamily.InterNetwork))
            {
                return new IPEndPoint(ipAddress, port);
            }

            return new IPEndPoint(new IPAddress(new byte[] { 1, 0, 0, 127 }), 5566);
        }

        public static IEnumerable<IPAddress> GetLocalIpAddresses(bool includeIpV6)
        {
            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection)
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            return (from network in networkInterfaces
                    select network.GetIPProperties()
                    into properties
                    from address in properties.UnicastAddresses
                    where includeIpV6 || address.Address.AddressFamily == AddressFamily.InterNetwork
                    where !IPAddress.IsLoopback(address.Address)
                    select address.Address);
        }
    }
}