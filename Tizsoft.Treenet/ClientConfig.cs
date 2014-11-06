using System;
using System.Net.Sockets;

namespace Tizsoft.Treenet
{
    // TODO: Remove duplicated.
    public class ClientConfig : EventArgs
    {
        public const int DefaultBufferSize = 8 * 1024;

        public const int DefaultMaxMessageSize = 8 * 1024;

        /// <summary>
        ///     Gets or sets the remote address.
        /// </summary>
        /// <example>
        /// var config = new ClientConfig();
        /// config.Address = "192.168.1.1";
        /// config.Address = "127.0.0.1";
        /// config.Address = "localhost";
        /// </example>
        public string Address { get; set; }

        public int Port { get; set; }

        /// <summary>
        ///     Gets or sets the maximum message size.
        /// </summary>
        /// <remarks>
        ///     We strongly recommend the buffer size should be 8KB (default value).
        /// </remarks>
        public int BufferSize { get; set; }

        /// <summary>
        ///     Gets or sets the maximum message size.
        /// </summary>
        public int MaxMessageSize { get; set; }

        /// <summary>
        ///     Gets or sets the socket type.
        /// </summary>
        /// <remarks>
        ///     <see cref="SocketType.Stream"/> is recommended.
        /// </remarks>
        public SocketType TransferType { get; set; }

        /// <summary>
        ///     Gets or sets the protocol type of socket.
        /// </summary>
        /// <remarks>
        ///     <see cref="ProtocolType.Tcp"/> is recommended.
        /// </remarks>
        public ProtocolType UseProtocol { get; set; }

        public PacketProtocolSettings PacketProtocolSettings { get; set; }

        public bool AutoReConnect { get; set; }

        public int ConnectRetryTimes { get; set; }

        public double ConnectRetryInterval { get; set; }

        public ClientConfig()
        {
            Address = "127.0.0.1";
            TransferType = SocketType.Stream;
            UseProtocol = ProtocolType.Tcp;
            AutoReConnect = true;
            ConnectRetryTimes = 5;
            ConnectRetryInterval = 3.0;
            BufferSize = DefaultBufferSize;
            MaxMessageSize = DefaultMaxMessageSize;
        }
    }
}