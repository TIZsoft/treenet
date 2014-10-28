using NUnit.Framework;

namespace Tizsoft.Treenet.Tests
{
    [TestFixture]
    public class TestPacketProtocolSettings
    {
        [Test]
        public void TestProperties()
        {
            var baseHeaderSize = sizeof(PacketFlags) + sizeof(PacketType);

            // Test default settings.
            var settings = new PacketProtocolSettings();
            Assert.AreEqual(0, settings.SignatureLength);
            Assert.IsFalse(settings.HasSignature);
            Assert.AreEqual(baseHeaderSize, settings.HeaderSize);

            // Test signature is null.
            settings.Signature = null;
            Assert.AreEqual(0, settings.SignatureLength);
            Assert.IsFalse(settings.HasSignature);
            Assert.AreEqual(baseHeaderSize, settings.HeaderSize);

            // Test signature is empty.
            settings.Signature = new byte[0];
            Assert.AreEqual(0, settings.SignatureLength);
            Assert.IsFalse(settings.HasSignature);
            Assert.AreEqual(baseHeaderSize, settings.HeaderSize);

            // Test signature is not null or empty.
            var signature = new byte[] { 12, 34, 56, 78, 90 };
            settings.Signature = signature;
            Assert.AreEqual(signature.Length, settings.SignatureLength);
            Assert.IsTrue(settings.HasSignature);
            Assert.AreEqual(baseHeaderSize + signature.Length, settings.HeaderSize);

            // Check signature content.
            for (var i = 0; i != signature.Length; ++i)
            {
                Assert.AreEqual(signature[i], settings.Signature[i]);
            }
        }
    }
}
