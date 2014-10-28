using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace Tizsoft.Treenet
{
    // TODO: Performance issue: Space complexity is O(2n) where n is message size. Applocation level may waste more spaces.
    // TODO: Optimize copy memory by message size and machine architecture.
    // Reference: http://code4k.blogspot.tw/2010/10/high-performance-memcpy-gotchas-in-c.html

    // Original source: http://blog.stephencleary.com/2009/04/sample-code-length-prefix-message.html
    /// <summary>
    ///     Maintains the necessary buffers for applying a length-prefix message framing protocol over a stream.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Create one instance of this class for each incoming stream, and assign a handler to
    ///         <see cref="MessageArrived" />. As bytes arrive at the stream, pass them to <see cref="DataReceived" />, which
    ///         will invoke <see cref="MessageArrived" /> as necessary.
    ///     </para>
    ///     <para>
    ///         If <see cref="DataReceived" /> raises <see cref="System.Net.ProtocolViolationException" />, then the stream
    ///         data should be considered invalid. After that point, no methods should be called on that
    ///         <see cref="MessageFraming" /> instance.
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
    public class MessageFraming
    {
        static readonly byte[] KeepaliveMessage = new byte[0];

        static readonly MessageArrivedEventArgs KeepaliveEventArgs = new MessageArrivedEventArgs(KeepaliveMessage);

        /// <summary>
        /// The maximum size of messages allowed.
        /// </summary>
        readonly int _maxMessageSize;

        /// <summary>
        ///     The buffer for the packet prefix.
        /// </summary>
        readonly byte[] _lengthBuffer;

        /// <summary>
        ///     The buffer for the data; this is null if we are receiving the length prefix buffer.
        /// </summary>
        /// <remarks>
        ///     The buffer is not used to low-level (e.g. socket) but application.
        /// </remarks>
        byte[] _dataBuffer;

        /// <summary>
        ///     The number of bytes already read into the buffer (the length buffer if <see cref="_dataBuffer" /> is null, otherwise
        ///     the data buffer).
        /// </summary>
        int _bytesReceived;

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

        /// <summary>
        ///     Initializes a new <see cref="MessageFraming" />, limiting message sizes to the given maximum size.
        /// </summary>
        /// <param name="maxMessageSize">
        ///     The maximum message size supported by this protocol. This may be less than or equal to
        ///     zero to indicate no maximum message size.
        /// </param>
        public MessageFraming(int maxMessageSize)
        {
            if (maxMessageSize <= 0)
            {
                throw new ArgumentOutOfRangeException("maxMessageSize", "Maximum message size is less than or equal to zero.");
            }

            // We allocate the buffer for receiving prefix immediately.
            _lengthBuffer = new byte[sizeof(int)];
            _maxMessageSize = maxMessageSize;
        }

        /// <summary>
        ///     Wraps a message. The wrapped message is ready to send to a stream.
        /// </summary>
        /// <remarks>
        ///     <para>Generates a length prefix for the message and returns the combined length prefix and message.</para>
        /// </remarks>
        /// <param name="message">The message to send.</param>
        /// <exception cref="ArgumentNullException"><see cref="message"/> is null.</exception>
        public static byte[] WrapMessage(byte[] message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            // Get the length prefix for the message.
            var lengthPrefix = BitConverter.GetBytes(message.Length);

            // Concatenate the length prefix and the message.
            var ret = new byte[lengthPrefix.Length + message.Length];
            lengthPrefix.CopyTo(ret, 0);
            message.CopyTo(ret, lengthPrefix.Length);

            return ret;
        }

        /// <summary>
        ///     Wraps a keepalive (0-length) message. The wrapped message is ready to send to a stream.
        /// </summary>
        public static byte[] WrapKeepaliveMessage()
        {
            return WrapMessage(KeepaliveMessage);
        }

        /// <summary>
        ///     Notifies the <see cref="MessageFraming" /> instance that incoming data has been received from the stream. This
        ///     method will invoke <see cref="MessageArrived" /> as necessary.
        /// </summary>
        /// <remarks>
        ///     <para>This method may invoke <see cref="MessageArrived" /> zero or more times.</para>
        ///     <para>
        ///         Zero-length receives are ignored. Many streams use a 0-length read to indicate the end of a stream, but
        ///         <see cref="MessageFraming" /> takes no action in this case.
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
            while (i < data.Length)
            {
                // Determine how many bytes we want to transfer to the buffer and transfer them.
                var bytesAvailable = data.Length - i;
                if (_dataBuffer != null)
                {
                    // We're reading into the data buffer.
                    var bytesRequested = _dataBuffer.Length - _bytesReceived;

                    // Copy the incoming bytes into the buffer.
                    var bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Debug.Assert(bytesTransferred >= 0);
                    Array.Copy(data, i, _dataBuffer, _bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    // Notify "read completion".
                    ReadCompleted(bytesTransferred);
                }
                else
                {
                    // We're reading into the length buffer.
                    var bytesRequested = _lengthBuffer.Length - _bytesReceived;

                    // Copy the incoming bytes into the buffer.
                    var bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Debug.Assert(bytesTransferred >= 0);
                    Array.Copy(data, i, _lengthBuffer, _bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    // Notify "read completion".
                    ReadCompleted(bytesTransferred);
                }
            }

            Debug.Assert(i == data.Length);
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
                if (_bytesReceived != sizeof(int))
                {
                    // We haven't gotten all the length buffer yet: just wait for more data to arrive.
                }
                else
                {
                    // We've gotten the length buffer.
                    var length = BitConverter.ToInt32(_lengthBuffer, 0);

                    // Sanity check for length < 0.
                    if (length < 0)
                    {
                        throw new ProtocolViolationException("Message length is less than zero");
                    }

                    // Another sanity check is needed here for very large packets, to prevent denial-of-service attacks.
                    if (_maxMessageSize > 0 &&
                        length > _maxMessageSize)
                    {
                        throw new ProtocolViolationException(string.Format(
                            "Message length {0} is  larger than maximum message size {1}.",
                            length.ToString(CultureInfo.InvariantCulture),
                            _maxMessageSize.ToString(CultureInfo.InvariantCulture))
                        );
                    }

                    // Zero-length packets are allowed as keepalives.
                    if (length == 0)
                    {
                        _bytesReceived = 0;
                        OnMessageArrived(KeepaliveEventArgs);
                    }
                    else
                    {
                        // Create the data buffer and start reading into it.
                        _dataBuffer = new byte[length];
                        _bytesReceived = 0;
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
                    // We've gotten an entire packet
                    OnMessageArrived(new MessageArrivedEventArgs(_dataBuffer));
                    
                    // Start reading the length buffer again.
                    _dataBuffer = null;
                    _bytesReceived = 0;
                }
            }
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