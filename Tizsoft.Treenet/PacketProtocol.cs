using System;
using System.Diagnostics;
using System.IO;
using Tizsoft.IO.Compression;
using Tizsoft.Log;
using Tizsoft.Security.Cryptography;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet
{
    // TODO: Non-blocking decrypt & decompress.
    public class PacketProtocol
    {
        PacketProtocolSettings Settings { get; set; }

        public ICryptoProvider CryptoProvider { private get; set; }

        public ICompressProvider CompressProvider { private get; set; }

        public PacketProtocol(PacketProtocolSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            Settings = settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool TryParsePacket(byte[] message, out IPacket packet)
        {
            if (message == null)
            {
                packet = default(IPacket);
                return false;
            }

            Debug.Assert(Settings != null);

            if (message.Length < Settings.HeaderSize)
            {
                packet = default(IPacket);
                return false;
            }

            try
            {
                message = Decrypt(message);

                // Begin parse packet header.
                using (var memoryStream = new MemoryStream(message))
                {
                    using (var binaryReader = new BinaryReader(memoryStream))
                    {
                        // Verify signature.
                        if (Settings.HasSignature)
                        {
                            var signature = binaryReader.ReadBytes(Settings.SignatureLength);
                            if (!CheckSignature(signature))
                            {
                                packet = default(IPacket);
                                return false;
                            }
                        }

                        // Extract header and content.
                        var contentLength = message.Length - Settings.HeaderSize;
                        Debug.Assert(contentLength >= 0);

                        var packetFlags = (PacketFlags)binaryReader.ReadByte();
                        var packetType = (PacketType)binaryReader.ReadByte();
                        var content = binaryReader.ReadBytes(contentLength);
                        
                        if (packetFlags.HasFlag(PacketFlags.Compressed))
                        {
                            if (TryDecompress(content, out content))
                            {
                                // Decompression successful.
                            }
                            else
                            {
                                // Decompression failed.
                                packet = default(IPacket);
                                return false;
                            }
                        }

                        packet = new Packet
                        {
                            PacketFlags = packetFlags,
                            PacketType = packetType,
                            Content = content,
                        };
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                // Should never occurred.
                Debug.WriteLine(e);

                // Fallback object state.
                packet = default(IPacket);
                return false;
            }
        }

        bool CheckSignature(byte[] signature)
        {
            Debug.Assert(Settings != null);

            if (Settings.HasSignature)
            {
                if (signature == null ||
                    signature.Length != Settings.SignatureLength)
                {
                    return false;
                }

                var signatureLength = Settings.SignatureLength;

                for (var i = 0; i != signatureLength; ++i)
                {
                    if (signature[i] != Settings.Signature[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        byte[] Decrypt(byte[] message)
        {
            Debug.Assert(message != null);
            return CryptoProvider != null ? CryptoProvider.Decrypt(message) : message;
        }

        bool TryDecompress(byte[] content, out byte[] decompressed)
        {
            if (CompressProvider != null)
            {
                Debug.Assert(content != null);
                decompressed = CompressProvider.Decompress(content);
                return true;
            }

            GLogger.Error("Content requires compression but CompressProvider is null.");
            decompressed = null;
            return false;
        }
    }
}
