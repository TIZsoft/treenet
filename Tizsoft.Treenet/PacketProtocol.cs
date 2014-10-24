using SevenZip;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using Tizsoft.Security.Cryptography;

namespace Tizsoft.Treenet
{
    // Original source: http://blog.stephencleary.com/2009/04/sample-code-length-prefix-message.html
    /// <summary>
    ///     Maintains the necessary buffers for applying a length-prefix message framing protocol over a stream.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <see cref="PacketProtocolSettings" /> to configure an instance of <see cref="PacketProtocol" />.
    ///     </para>
    ///     <para>
    ///         Create one instance of this class for each incoming stream, and assign a handler to
    ///         <see cref="MessageArrived" />. As bytes arrive at the stream, pass them to <see cref="DataReceived" />, which
    ///         will invoke <see cref="MessageArrived" /> as necessary.
    ///     </para>
    ///     <para>
    ///         If <see cref="DataReceived" /> raises <see cref="System.Net.ProtocolViolationException" />, then the stream
    ///         data should be considered invalid. After that point, no methods should be called on that
    ///         <see cref="PacketProtocol" /> instance.
    ///     </para>
    ///     <para>
    ///         This class uses a 4-byte signed integer length prefix, which allows for message sizes up to 2 GB. Keepalive
    ///         messages are supported as messages with a length prefix of 0 and no message data.
    ///     </para>
    ///     <para>
    ///         This is EXAMPLE CODE! It is not particularly efficient; in particular, if this class is rewritten so that a
    ///         particular interface is used (e.g., Socket's IAsyncResult methods), some buffer copies become unnecessary and
    ///         may be removed.
    ///     </para>
    /// </remarks>
    public class PacketProtocol
    {
        class Prefix
        {
            public byte[] Signature { get; set; }

            public PacketFlags PacketFlags { get; set; }

            public PacketType PacketType { get; set; }

            public int MessageLength { get; set; }
        }

        static readonly byte[] KeepaliveMessage = new byte[0];

        /// <summary>
        ///     The buffer for the packet prefix.
        /// </summary>
        readonly byte[] _prefixBuffer;

        /// <summary>
        ///     The buffer for the data; this is null if we are receiving the length prefix buffer.
        /// </summary>
        byte[] _dataBuffer;

        /// <summary>
        ///     The number of bytes already read into the buffer (the length buffer if <see cref="_dataBuffer" /> is null, otherwise
        ///     the data buffer).
        /// </summary>
        int _bytesReceived;

        Prefix _prefix;

        /// <summary>
        ///     The packet protocol settings to determine prefix signature, maximum message size, etc.
        /// </summary>
        readonly PacketProtocolSettings _settings;

        /// <summary>
        ///     Indicates the completion of a message read from the stream.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This may be called with an empty message, indicating that the other end had sent a keepalive message. This
        ///         will never be called with a null message.
        ///     </para>
        ///     <para>
        ///         This event is invoked from within a call to <see cref="DataReceived" />. Handlers for this event should not
        ///         call <see cref="DataReceived" />.
        ///     </para>
        /// </remarks>
        public event EventHandler<MessageArrivedEventArgs> MessageArrived;

        public ICryptoProvider CryptoProvider { get; set; }

        /// <summary>
        ///     Wraps a message. The wrapped message is ready to send to a stream.
        /// </summary>
        /// <remarks>
        ///     <para>Generates a packet header for the message.</para>
        /// </remarks>
        /// <param name="message">The message to send.</param>
        /// <param name="packetFlags"></param>
        /// <param name="packetType"></param>
        public byte[] WrapMessage(byte[] message, PacketFlags packetFlags, PacketType packetType)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (packetFlags.HasFlag(PacketFlags.Compressed))
            {
                // TODO: Non-blocking compress.
                message = SevenZipCompressor.CompressBytes(message);
            }

            var wrappedMessage = new byte[_settings.PrefixSize + message.Length];

            using (var stream = new MemoryStream(wrappedMessage))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    // [ Signature (n bytes) | PacketFlags (8 bits) | PacketType (1 byte) | Message Length (4 bytes) ]
                    if (_settings.Signature != null)
                    {
                        writer.Write(_settings.Signature);
                    }

                    writer.Write((byte)packetFlags);
                    writer.Write((byte)packetType);
                    writer.Write(message.Length);
                    writer.Write(message);

                    var needToCrypto = CryptoProvider != null;
                    if (needToCrypto)
                    {
                        CryptoProvider.Encrypt(wrappedMessage, 0, _settings.PrefixSize);
                        CryptoProvider.Encrypt(wrappedMessage, _settings.PrefixSize, message.Length);
                    }

                }
            }

            return wrappedMessage;
        }

        /// <summary>
        ///     Wraps a keepalive (0-length) message. The wrapped message is ready to send to a stream.
        /// </summary>
        public byte[] WrapKeepaliveMessage()
        {
            return WrapMessage(KeepaliveMessage, PacketFlags.None, PacketType.Stream);
        }

        /// <summary>
        ///     Initializes a new <see cref="PacketProtocol" />, limiting message sizes to the given maximum size.
        /// </summary>
        /// <param name="settings">
        ///     The maximum message size supported by this protocol. This may be less than or equal to
        ///     zero to indicate no maximum message size.
        /// </param>
        public PacketProtocol(PacketProtocolSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            if (!settings.IsValid)
                throw new ArgumentException("Settings is invalid.");

            // We allocate the buffer for receiving prefix immediately.
            _prefixBuffer = new byte[settings.PrefixSize];
            _settings = settings;
        }

        /// <summary>
        ///     Notifies the <see cref="PacketProtocol" /> instance that incoming data has been received from the stream. This
        ///     method will invoke <see cref="MessageArrived" /> as necessary.
        /// </summary>
        /// <remarks>
        ///     <para>This method may invoke <see cref="MessageArrived" /> zero or more times.</para>
        ///     <para>
        ///         Zero-length receives are ignored. Many streams use a 0-length read to indicate the end of a stream, but
        ///         <see cref="PacketProtocol" /> takes no action in this case.
        ///     </para>
        /// </remarks>
        /// <param name="data">The data received from the stream. Cannot be null.</param>
        /// <exception cref="System.Net.ProtocolViolationException">If the data received is not a properly-formed message.</exception>
        public void DataReceived(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            // Process the incoming data in chunks, as the ReadCompleted requests it.

            // Logically, we are satisfying read requests with the received data, instead of processing the
            // incoming buffer looking for messages.

            var i = 0;
            while (i != data.Length)
            {
                // Determine how many bytes we want to transfer to the buffer and transfer them.
                var bytesAvailable = data.Length - i;
                if (_dataBuffer != null)
                {
                    // We're reading into the data buffer.
                    var bytesRequested = _dataBuffer.Length - _bytesReceived;

                    // Copy the incoming bytes into the buffer.
                    var bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Array.Copy(data, i, _dataBuffer, _bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    // Notify "read completion".
                    ReadCompleted(bytesTransferred);
                }
                else
                {
                    // We're reading into the prefix buffer.
                    var bytesRequested = _prefixBuffer.Length - _bytesReceived;

                    // Copy the incoming bytes into the buffer.
                    var bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Debug.Assert(bytesTransferred >= 0);
                    Array.Copy(data, i, _prefixBuffer, _bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    // Notify "read completion".
                    ReadCompleted(bytesTransferred);
                }
            }
        }

        /// <summary>
        ///     Called when a read completes. Parses the received data and calls <see cref="MessageArrived" /> if necessary.
        /// </summary>
        /// <param name="count">The number of bytes read.</param>
        /// <exception cref="System.Net.ProtocolViolationException">If the data received is not a properly-formed message.</exception>
        void ReadCompleted(int count)
        {
            Debug.Assert(count >= 0);

            // Get the number of bytes read into the buffer.
            _bytesReceived += count;

            if (_dataBuffer == null)
            {
                // We're currently receiving the length buffer.

                if (_bytesReceived != _settings.PrefixSize)
                {
                    // We haven't gotten all the length buffer yet: just wait for more data to arrive.
                }
                else
                {
                    // We've gotten the prefix buffer.
                    if (TryParsePrefix(out _prefix))
                    {
                        var length = _prefix.MessageLength;

                        // Sanity check for length < 0
                        if (length < 0)
                            throw new ProtocolViolationException("Message length is less than zero");

                        // Another sanity check is needed here for very large packets, to prevent denial-of-service attacks.
                        if (_settings.MaxMessageSize > 0 &&
                            length > _settings.MaxMessageSize)
                        {
                            throw new ProtocolViolationException(string.Format("Message length {0} is larger than maximum message size {1}.",
                                length.ToString(CultureInfo.InvariantCulture),
                                _settings.MaxMessageSize.ToString(CultureInfo.InvariantCulture))
                            );
                        }

                        // Zero-length packets are allowed as keepalives.
                        if (length == 0)
                        {
                            _bytesReceived = 0;
                            OnMessageArrived(new MessageArrivedEventArgs(new byte[0]));
                        }
                        else
                        {
                            // Create the data buffer and start reading into it.
                            _dataBuffer = new byte[length];
                            _bytesReceived = 0;
                        }
                    }
                }
            }
            else
            {
                if (_bytesReceived != _dataBuffer.Length)
                {
                    // We haven't gotten all the data buffer yet: just wait for more data to arrive.
                }
                else
                {
                    if (_prefix != null &&
                        _prefix.PacketFlags.HasFlag(PacketFlags.Compressed))
                    {
                        // TODO: Decompressing.
                    }

                    // We've gotten an entire packet.
                    OnMessageArrived(new MessageArrivedEventArgs(_dataBuffer));

                    // Start reading the length buffer again.
                    _dataBuffer = null;
                    _prefix = null;
                    _bytesReceived = 0;
                }
            }
        }

        bool TryParsePrefix(out Prefix prefix)
        {
            if (_prefixBuffer == null ||
                _prefixBuffer.Length != _settings.PrefixSize)
            {
                prefix = default(Prefix);
                return false;
            }

            var prefixBuffer = _prefixBuffer;

            if (CryptoProvider != null)
            {
                prefixBuffer = CryptoProvider.Decrypt(_prefixBuffer);
            }

            using (var memoryStream = new MemoryStream(prefixBuffer))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    prefix = new Prefix();

                    if (_settings.HasSignature)
                    {
                        var signature = binaryReader.ReadBytes(prefix.Signature.Length);

                        if (VerifySignature(signature))
                        {
                            prefix.Signature = signature;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    prefix.PacketFlags = (PacketFlags)binaryReader.ReadByte();
                    prefix.PacketType = (PacketType)binaryReader.ReadByte();
                    prefix.MessageLength = binaryReader.ReadInt32();
                    return true;
                }
            }
        }

        bool VerifySignature(byte[] signature)
        {
            if (_settings.HasSignature)
            {
                if (signature == null ||
                    signature.Length != _settings.Signature.Length)
                {
                    return false;
                }

                for (var i = 0; i != _settings.Signature.Length; ++i)
                {
                    if (signature[i] != _settings.Signature[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        void OnMessageArrived(MessageArrivedEventArgs e)
        {
            if (MessageArrived != null)
            {
                MessageArrived(this, e);
            }
        }
    }
}