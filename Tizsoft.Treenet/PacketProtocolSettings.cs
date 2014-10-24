using System;

namespace Tizsoft.Treenet
{
    /// <summary>
    /// Represents a settings of packet protocol.
    /// </summary>
    /// <remarks>
    /// See the following to understand the structure of header packing below.
    /// [ Signature (n bytes) | PacketFlags (8 bits) | Packet type (1 byte) | Message size (4 byte) ]
    /// </remarks>
    [Serializable]
    public class PacketProtocolSettings
    {
        byte[] _signature;

        /// <summary>
        ///     Gets or sets the signature for packet protocol.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If a incoming packet contains invalid signature or the signature does not exist, then drop it.
        ///     </para>
        ///     <para>
        ///         Null signature or zero-length signature are equality.
        ///     </para>
        /// </remarks>
        public byte[] Signature
        {
            get { return _signature; }
            set
            {
                _signature = value;
                ComputePrefixSize();
            }
        }

        public bool HasSignature { get { return Signature != null && Signature.Length > 0; } }

        /// <summary>
        ///     Gets or sets the maximum message size.
        /// </summary>
        public int MaxMessageSize { get; set; }

        /// <summary>
        ///     Gets the prefix size (count of bytes).
        /// </summary>
        public int PrefixSize { get; private set; }

        public bool IsValid
        {
            get { return MaxMessageSize > 0; }
        }

        void ComputePrefixSize()
        {
            // Signature        (n bytes)
            // PacketFlags      (8 bits)
            // PacketType       (1 byte)
            // Message size     (4 bytes)
            PrefixSize = (Signature != null ? sizeof(byte) * Signature.Length : 0) +
                sizeof(PacketFlags) +
                sizeof(PacketType) +
                sizeof(int);
        }
    }
}