using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public IEnumerable<PacketProtocolSettings> TestValidConstructorArgsCaseSources()
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

            public override string ToString()
            {
                var builder = new StringBuilder();

                builder.AppendFormat("Settings={0}\n", Settings);
                builder.AppendFormat("Packet={0}\n", Packet);

                return builder.ToString();
            }
        }

        public IEnumerable<TryWrapPacketCaseSource> TestValidTryWrapPacketCaseSources()
        {
            var expectedSignature = new byte[] { 12, 34, 56, 78, 90 };
            const int maxContentSize = 4 * 1024 * 1024;
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
                    MaxContentSize = maxContentSize
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
                    MaxContentSize = maxContentSize
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
                    MaxContentSize = maxContentSize
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


            // TODO: Use Aes instead.
            #region Crypto only.

            // Case 1: Normal packet.
            yield return new TryWrapPacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                    MaxContentSize = maxContentSize
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
                    MaxContentSize = maxContentSize
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
                    MaxContentSize = maxContentSize
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
            Debug.WriteLine(caseSource);

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

        [Test]
        public void TestInvalidTryWrapPacket()
        {
            var expectedSignature = new byte[] { 12, 34, 56, 78, 90 };
            const int maxContentSize = 4 * 1024 * 1024;
            var content = Encoding.UTF8.GetBytes("G_G_In_In_Der~");
            var cryptoProvider = new XorCryptoProvider("TIZSoft");
            var packetProtocolSettings = new PacketProtocolSettings
            {
                Signature = expectedSignature,
                MaxContentSize = maxContentSize
            };
            var packetProtocol = new PacketProtocol(packetProtocolSettings)
            {
                CryptoProvider = cryptoProvider,
                CompressProvider = null
            };

            byte[] message;

            // Case 1: Null packet.
            var isWrapSuccess = packetProtocol.TryWrapPacket(null, out message);
            Assert.IsFalse(isWrapSuccess);

            // Case 2: Requires compression.
            var packet = new Packet
            {
                Content = content,
                PacketFlags = PacketFlags.Compressed,
                PacketType = PacketType.Stream
            };
            isWrapSuccess = packetProtocol.TryWrapPacket(packet, out message);
            Assert.IsFalse(isWrapSuccess);

            // Case 3: Content size is larger than max content size.
            packet = new Packet
            {
                Content = new byte[maxContentSize + 1],
                PacketFlags = PacketFlags.None,
                PacketType = PacketType.Stream
            };
            var rand = new System.Random();
            rand.NextBytes(packet.Content);
            isWrapSuccess = packetProtocol.TryWrapPacket(packet, out message);
            Assert.IsFalse(isWrapSuccess);
        }

        public class TryParsePacketCaseSource
        {
            public PacketProtocolSettings Settings { get; set; }

            public ICryptoProvider CryptoProvider { get; set; }

            public ICompressProvider CompressProvider { get; set; }

            public PacketFlags PacketFlags { get; set; }

            public PacketType PacketType { get; set; }

            public int MinContentSize { get; set; }

            public int MaxContentSize { get; set; }

            public byte[] Content { get; set; }

            public byte[] CreateMessage(PacketProtocol  packetProtocol)
            {
                Content = GenerateRandomContent(MinContentSize, MaxContentSize);

                var packet = new Packet
                {
                    PacketFlags = PacketFlags,
                    PacketType = PacketType,
                    Content = Content
                };

                byte[] message;
                packetProtocol.TryWrapPacket(packet, out message);
                return message;
            }

            public override string ToString()
            {
                var builder = new StringBuilder();

                builder.AppendFormat("Settings={0}\n", Settings);
                builder.AppendFormat("PacketFlags={0}\n", PacketFlags);
                builder.AppendFormat("PacketType={0}\n", PacketType);
                builder.AppendFormat("MinContentSize={0}\n", MinContentSize);
                builder.AppendFormat("MaxContentSize={0}\n", MaxContentSize);

                return builder.ToString();
            }
        }

        static byte[] GenerateRandomContent(int minLength, int maxLength)
        {
            var random = new System.Random();
            var length = random.Next(minLength, maxLength);
            var bytes = new byte[length];
            random.NextBytes(bytes);
            return bytes;
        }

        // Precondition: TryWrapMessage is correct.
        public IEnumerable<TryParsePacketCaseSource> TestValidTryParsePacketCaseSources()
        {
            var expectedSignature = new byte[] { 12, 34, 56, 78, 90 };
            const int maxContentSize = 4 * 1024 * 1024;

            var cryptoProvider = new XorCryptoProvider("No Game No Life.");

            // TODO: Case 4: Requires compression.

            #region Without crypto & compress.

            // Case 1: Normal case.
            yield return new TryParsePacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                    MaxContentSize = maxContentSize,
                },
                CryptoProvider = null,
                CompressProvider = null,
                PacketFlags = PacketFlags.None,
                PacketType = PacketType.Stream,
                MinContentSize = 1024,
                MaxContentSize = 1024,
            };

            // Case 2: Max content size.
            yield return new TryParsePacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                    MaxContentSize = maxContentSize,
                },
                CryptoProvider = null,
                CompressProvider = null,
                PacketFlags = PacketFlags.None,
                PacketType = PacketType.Stream,
                MinContentSize = maxContentSize,
                MaxContentSize = maxContentSize,
            };

            // Case 3: Keepalive or empty content.
            yield return new TryParsePacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                    MaxContentSize = maxContentSize,
                },
                CryptoProvider = null,
                CompressProvider = null,
                PacketFlags = PacketFlags.None,
                PacketType = PacketType.Stream,
                MinContentSize = 0,
                MaxContentSize = 0,
            };

            #endregion


            #region Crypto only.

            // Case 1: Normal case.
            yield return new TryParsePacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                    MaxContentSize = maxContentSize,
                },
                CryptoProvider = cryptoProvider,
                CompressProvider = null,
                PacketFlags = PacketFlags.None,
                PacketType = PacketType.Stream,
                MinContentSize = 1024,
                MaxContentSize = 1024,
            };

            // Case 2: Max content size.
            yield return new TryParsePacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                    MaxContentSize = maxContentSize,
                },
                CryptoProvider = cryptoProvider,
                CompressProvider = null,
                PacketFlags = PacketFlags.None,
                PacketType = PacketType.Stream,
                MinContentSize = maxContentSize,
                MaxContentSize = maxContentSize,
            };

            // Case 3: Keepalive or empty content.
            yield return new TryParsePacketCaseSource
            {
                Settings = new PacketProtocolSettings
                {
                    Signature = expectedSignature,
                    MaxContentSize = maxContentSize,
                },
                CryptoProvider = cryptoProvider,
                CompressProvider = null,
                PacketFlags = PacketFlags.None,
                PacketType = PacketType.Stream,
                MinContentSize = 0,
                MaxContentSize = 0,
            };

            #endregion
        }

        // Precondition: TryWrapPacket must be correct.
        [TestCaseSource("TestValidTryParsePacketCaseSources")]
        public void TestValidTryParsePacket(TryParsePacketCaseSource caseSource)
        {
            Debug.WriteLine(caseSource);

            var packetProtocol = new PacketProtocol(caseSource.Settings)
            {
                CryptoProvider = caseSource.CryptoProvider,
                CompressProvider = caseSource.CompressProvider
            };

            IPacket packet;
            if (packetProtocol.TryParsePacket(caseSource.CreateMessage(packetProtocol), out packet))
            {
                Assert.IsNotNull(packet);
                Assert.AreEqual(caseSource.PacketFlags, packet.PacketFlags);
                Assert.AreEqual(caseSource.PacketType, packet.PacketType);

                var actualContent = packet.Content;
                Assert.AreEqual(caseSource.Content.Length, actualContent.Length);

                for (var i = 0; i != caseSource.Content.Length; ++i)
                {
                    if (caseSource.Content[i] != actualContent[i])
                    {
                        Assert.Fail("Content is not matched.");
                    }
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        public void TestInvalidTryParsePacket()
        {

        }
    }
}
