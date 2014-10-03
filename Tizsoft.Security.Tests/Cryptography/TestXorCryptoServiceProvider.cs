using System;
using NUnit.Framework;
using Tizsoft.Security.Cryptography;

namespace Tizsoft.Security.Tests.Cryptography
{
    [TestFixture]
    public class TestXorCryptoServiceProvider
    {
        XorCryptoServiceProvider xor;

        [SetUp]
        public void Setup()
        {
            xor = new XorCryptoServiceProvider("Test");
        }

        [TearDown]
        public void Teardown()
        {
            xor = null;
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [Category("Exception")]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConstructorWithInvalidArgumentByString(string key)
        {
            xor = new XorCryptoServiceProvider(key);
            Assert.Fail("Should throw an ArgumentException.");
        }

        [Test]
        [Category("Exception")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorWithNullArgumentByByte()
        {
            xor = new XorCryptoServiceProvider((byte[])null);
            Assert.Fail("Should throw an ArgumentNullException.");
        }

        [Test]
        [Category("Exception")]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConstructorWithEmptyArgumentByByte()
        {
            xor = new XorCryptoServiceProvider(new byte[0]);
            Assert.Fail("Should throw an ArgumentException.");
        }

        [Test]
        [Category("Exception")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestEncryptWithEmptyArgument()
        {
            xor.Encrypt(null);
            Assert.Fail("Should throw an ArgumentNullException.");
        }

        [TestCase((byte)123, (byte)10, (byte)19)]
        [TestCase((byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10)]
        [TestCase((byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10)]
        public void TestDecrypt(params byte[] data)
        {
            xor.Decrypt(data);
        }

        [TestCase((byte)123, (byte)10, (byte)19)]
        [TestCase((byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10)]
        [TestCase((byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10)]
        public void TestEncrypt(params byte[] data)
        {
            xor.Encrypt(data);
        }
    }
}
