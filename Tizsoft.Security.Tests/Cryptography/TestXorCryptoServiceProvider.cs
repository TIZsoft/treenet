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

        [TestCase(new byte[] { 47, 10, 118 },
            (byte)123, (byte)10, (byte)19)]
        [TestCase(new byte[] { 85, 2, 102, 4, 118, 6, 115, 8, 93, 10 },
            (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10)]
        [TestCase(new byte[] { 85, 2, 102, 4, 118, 6, 115, 8, 93, 10, 100, 2, 112, 4, 113, 6, 83, 8, 108, 10, 114, 2, 119, 4, 81, 6, 98, 8, 122, 10 },
            (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10,
            (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10,
            (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10)]
        public void TestDecrypt(byte[] correctResult, params byte[] data)
        {
            var result = xor.Decrypt(data);

            Assert.AreEqual(correctResult.Length, result.Length);

            for (int i = 0; i < correctResult.Length; i++)
            {
                Assert.AreEqual(correctResult[i], result[i]);
            }
        }

        [TestCase(new byte[] { 47, 10, 118 }, 
            (byte)123, (byte)10, (byte)19)]
        [TestCase(new byte[] { 85, 2, 102, 4, 118, 6, 115, 8, 93, 10 }, 
            (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10)]
        [TestCase(new byte[] { 85, 2, 102, 4, 118, 6, 115, 8, 93, 10, 100, 2, 112, 4, 113, 6, 83, 8, 108, 10, 114, 2, 119, 4, 81, 6, 98, 8, 122, 10 },
            (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10,
            (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10,
            (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10)]
        public void TestEncrypt(byte[] correctResult, params byte[] data)
        {
            var result = xor.Encrypt(data);

            Assert.AreEqual(correctResult.Length, result.Length);

            for (int i = 0; i < correctResult.Length; i++)
            {
                Assert.AreEqual(correctResult[i], result[i]);
            }
        }
    }
}
