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

        public bool TryWrapPacket(IPacket packet, out byte[] message)
        {
            if (packet == null)
            {
                message = null;
                return false;
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var binaryWriter = new BinaryWriter(memoryStream))
                    {
                        // Write header.
                        WriteSignature(memoryStream);

                        var packetFlags = (byte) packet.PacketFlags;
                        var packetType = (byte) packet.PacketType;

                        binaryWriter.Write(packetFlags);
                        binaryWriter.Write(packetType);

                        var content = packet.Content;

                        // Compress if necessary.
                        if (packet.PacketFlags.HasFlag(PacketFlags.Compressed))
                        {
                            if (CompressProvider != null)
                            {
                                content = CompressProvider.Compress(content);
                            }
                            else
                            {
                                GLogger.Error("Content requires compression but CompressProvider is null.");
                                message = null;
                                return false;
                            }
                        }

                        // Write content.
                        var contentLength = content.Length;

                        Debug.Assert(contentLength >= 0);
                        if (contentLength > Settings.MaxContentSize)
                        {
                            message = null;
                            return false;
                        }

                        binaryWriter.Write(contentLength);
                        binaryWriter.Write(content);

                        message = memoryStream.ToArray();
                        Encrypt(ref message);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                // Should never occurred.
                Debug.WriteLine(e);
                message = null;
                return false;
            }
        }

        void WriteSignature(Stream output)
        {
            Debug.Assert(output != null);
            Debug.Assert(Settings != null);

            if (Settings.HasSignature)
            {
                output.Write(Settings.Signature, 0, Settings.SignatureLength);
            }
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
                Decrypt(ref message);

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

                        var packetFlags = (PacketFlags)binaryReader.ReadByte();
                        var packetType = (PacketType)binaryReader.ReadByte();
                        
                        // Extract header and content.
                        var contentLength = binaryReader.ReadInt32();

                        if (contentLength < 0)
                        {
                            packet = default(IPacket);
                            return false;
                        }

                        if (contentLength > Settings.MaxContentSize)
                        {
                            packet = default(IPacket);
                            return false;
                        }

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

        void Encrypt(ref byte[] message)
        {
            if (CryptoProvider != null)
            {
                message = CryptoProvider.Encrypt(message);
            }
        }

        void Decrypt(ref byte[] message)
        {
            message = CryptoProvider != null ? CryptoProvider.Decrypt(message) : message;
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
