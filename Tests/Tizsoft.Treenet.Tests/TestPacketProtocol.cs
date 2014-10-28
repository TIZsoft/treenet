using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Tizsoft.IO.Compression;
using Tizsoft.Security.Cryptography;
using Tizsoft.Treenet.Interface;

namespace Tizsoft.Treenet.Tests
{
    [TestFixture]
    public class TestPacketProtocol
    {
        [TestCase(null)]
        public void TestInvalidConstructorArgs(PacketProtocolSettings settings)
        {
            Assert.Catch<ArgumentException>(() => new PacketProtocol(settings));
        }

        public static IEnumerable<PacketProtocolSettings> TestValidConstructorArgsCaseSources()
        {
            yield return new PacketProtocolSettings();
        }

        [TestCaseSource("TestValidConstructorArgsCaseSources")]
        public void TestValidConstructorArgs(PacketProtocolSettings settings)
        {
            Assert.DoesNotThrow(() => new PacketProtocol(settings));
        }

        public class TryWrapPacketCaseSource 
        {
            public PacketProtocolSettings Settings { get; set; }

            public ICryptoProvider CryptoProvider { get; set; }

            public ICompressProvider CompressProvider { get; set; }

            public IPacket Packet { get; set; }
        }

        public static IEnumerable<TryWrapPacketCaseSource> TestValidTryWrapPacketCaseSources()
        {
            var expectedSignature = new byte[] { 12, 34, 56, 78, 90 };
            var content = Encoding.UTF8.GetBytes("G_G_In_In_Der~");
            var emptyContent = new byte[0];

            var cryptoProvider = new XorCryptoProvider("No Game No Life.");
            //var compressProvider = 


            #region Without crypto & compress.

            // Case 1: Normal packet.
            yield return new TryWrapPacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                },
                CryptoProvider = null,
                CompressProvider = null,
                Packet = new Packet
                {
                    PacketFlags = PacketFlags.None,
                    PacketType = PacketType.Stream,
                    Content = content
                }
            };

            // Case 2: Keepalive packet. Null content.
            yield return new TryWrapPacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                },
                CryptoProvider = null,
                CompressProvider = null,
                Packet = new Packet
                {
                    PacketFlags = PacketFlags.None,
                    PacketType = PacketType.Stream,
                    Content = null
                }
            };

            // Case 3: Keepalive packet. Empty content.
            yield return new TryWrapPacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                },
                CryptoProvider = null,
                CompressProvider = null,
                Packet = new Packet
                {
                    PacketFlags = PacketFlags.None,
                    PacketType = PacketType.Stream,
                    Content = emptyContent
                }
            };

            #endregion


            // TODO: Use Rijndael instead.
            #region Crypto only.

            // Case 1: Normal packet.
            yield return new TryWrapPacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                },
                CryptoProvider = cryptoProvider,
                CompressProvider = null,
                Packet = new Packet
                {
                    PacketFlags = PacketFlags.None,
                    PacketType = PacketType.Stream,
                    Content = content
                }
            };

            // Case 2: Keepalive packet. Null content.
            yield return new TryWrapPacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                },
                CryptoProvider = cryptoProvider,
                CompressProvider = null,
                Packet = new Packet
                {
                    PacketFlags = PacketFlags.None,
                    PacketType = PacketType.Stream,
                    Content = null
                }
            };

            // Case 3: Keepalive packet. Empty content.
            yield return new TryWrapPacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                },
                CryptoProvider = cryptoProvider,
                CompressProvider = null,
                Packet = new Packet
                {
                    PacketFlags = PacketFlags.None,
                    PacketType = PacketType.Stream,
                    Content = emptyContent
                }
            };

            #endregion


            // TODO: Compression.
            #region Compress only.



            #endregion


            // TODO: Both.
            #region With crypto & compress.



            #endregion
        }

        // Precondition: Implementation of ICryptoProvider and ICompressProvider must be correct.
        [TestCaseSource("TestValidTryWrapPacketCaseSources")]
        public void TestValidTryWrapPacket(TryWrapPacketCaseSource caseSource)
        {
            var packetProtocol = new PacketProtocol(caseSource.Settings)
            {
                CryptoProvider = caseSource.CryptoProvider,
                CompressProvider = caseSource.CompressProvider
            };

            byte[] message;
            if (packetProtocol.TryWrapPacket(caseSource.Packet, out message))
            {
                // Decrypt if necessary.
                if (caseSource.CryptoProvider != null)
                {
                    message = caseSource.CryptoProvider.Decrypt(message);
                }

                using (var memoryStream = new MemoryStream(message))
                {
                    using (var binaryReader = new BinaryReader(memoryStream))
                    {
                        var signature = binaryReader.ReadBytes(caseSource.Settings.SignatureLength);

                        // Verify if exist.
                        if (caseSource.Settings.HasSignature)
                        {
                            if (signature.Length > 0)
                            {
                                // Compare signature.
                                for (var i = 0; i != caseSource.Settings.SignatureLength; ++i)
                                {
                                    if (signature[i] != caseSource.Settings.Signature[i])
                                    {
                                        Assert.Fail("Signature is not matched.");
                                    }
                                }
                            }
                            else
                            {
                                Assert.Fail("Expected a non-zero length signature. Actual is empty.");
                            }
                        }

                        var packetFlags = (PacketFlags)binaryReader.ReadByte();
                        Assert.AreEqual(caseSource.Packet.PacketFlags, packetFlags);

                        var packetType = (PacketType)binaryReader.ReadByte();
                        Assert.AreEqual(caseSource.Packet.PacketType, packetType);

                        // Retrieve content.
                        var contentLength = binaryReader.ReadInt32();
                        Assert.GreaterOrEqual(contentLength, 0);

                        var content = binaryReader.ReadBytes(contentLength);

                        // Decompress if necessary.
                        if (caseSource.CompressProvider != null)
                        {
                            content = caseSource.CompressProvider.Decompress(content);
                        }

                        // Compare content.
                        for (var i = 0; i != contentLength; ++i)
                        {
                            if (caseSource.Packet.Content[i] != content[i])
                            {
                                Assert.Fail("Content is not matched.");
                            }
                        }
                    }
                }
            }
            else
            {
                Assert.Fail("Wrapping failure.");
            }
        }

        public class TryParsePacketCaseSource
        {
            public PacketProtocolSettings Settings { get; set; }

            
        }

        static byte[] GenerateRandomContent(int minLength, int maxLength)
        {
            var random = new Random();
            var length = random.Next(minLength, maxLength);
            var bytes = new byte[length];
            random.NextBytes(bytes);
            return bytes;
        }
        
        public static IEnumerable<TryParsePacketCaseSource> TestValidTryParsePacketCaseSources()
        {
            yield return new TryParsePacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    
                },
                
            };
        }
        
        // Precondition: TryWrapPacket must be correct.
        [TestCaseSource("TestValidTryParsePacketCaseSources")]
        public void TestValidTryParsePacket(TryParsePacketCaseSource caseSource)
        {
            
        }

        public void TestInvalidTryParsePacket()
        {
            
        }
    }
}
