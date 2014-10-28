using System;

namespace Tizsoft.Treenet
{
    // TODO: Crypto and compression settings.
    /// <summary>
    ///     Represents a settings of packet header handler.
    /// </summary>
    /// <remarks>
    ///     See https://bitbucket.org/tizsoftcoltd/treenet/wiki/PacketProtocol.
    /// </remarks>
    [Serializable]
    public class PacketProtocolSettings
    {
        const int DefaultMaxContentSize = 64 * 1024;

        byte[] _signature;
        int _maxContentSize;

        /// <summary>
        ///     Gets or sets the signature for packet header handler.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If a incoming packet contains invalid signature or the signature does not exist, then drop it.
        ///     </para>
        ///     <para>
        ///         Given a null signature or zero-length signature are equality.
        ///     </para>
        /// </remarks>
        public byte[] Signature
        {
            get { return _signature; }
            set
            {
                _signature = value;
                ComputeHeaderSize();
            }
        }

        public int SignatureLength { get { return Signature != null ? Signature.Length : 0; } }

        public bool HasSignature { get { return SignatureLength > 0; } }

        /// <summary>
        ///     Gets the header size (count of bytes).
        /// </summary>
        public int HeaderSize { get; private set; }

        public int MaxContentSize
        {
            get { return _maxContentSize; }
            set { _maxContentSize = value > 0 ? value : 1; }
        }

        public PacketProtocolSettings()
        {
            Signature = null;
            MaxContentSize = DefaultMaxContentSize;
        }

        void ComputeHeaderSize()
        {
            // Signature        (n bytes)
            // PacketFlags      (8 bits = 1 byte)
            // PacketType       (1 byte)
            // Content Length   (4 bytes)
            HeaderSize = SignatureLength * sizeof(byte) +
                sizeof(PacketFlags) +
                sizeof(PacketType) +
                sizeof(int);
        }
    }
}