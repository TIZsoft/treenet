using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tizsoft.Security.Cryptography;

namespace Tizsoft.Security.Tests.Cryptography
{
    [TestFixture]
    public class TestXorCryptoProvider
    {
        XorCryptoProvider xor;

        [SetUp]
        public void Setup()
        {
            xor = new XorCryptoProvider("Test");
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
            xor = new XorCryptoProvider(key);
            Assert.Fail("Should throw an ArgumentException.");
        }

        [Test]
        [Category("Exception")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorWithNullArgumentByByte()
        {
            xor = new XorCryptoProvider((byte[])null);
            Assert.Fail("Should throw an ArgumentNullException.");
        }

        [Test]
        [Category("Exception")]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConstructorWithEmptyArgumentByByte()
        {
            xor = new XorCryptoProvider(new byte[0]);
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
        
        [Test]
        [Category("Exception")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDecryptWithEmptyArgument()
        {
            xor.Decrypt(null);
            Assert.Fail("Should throw an ArgumentNullException.");
        }
        
        static void Compare<T>(T[] expected, T[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
        
        public class CryptoCaseSource
        {
            public byte[] Input { get; set; }

            public byte[] Expected { get; set; }
        }

        public static IEnumerable<CryptoCaseSource> TestDecryptCaseSource()
        {
            yield return new CryptoCaseSource
            {
                Expected = new byte[] {  47,  10, 118 },

                Input    = new byte[] { 123,  10,  19 }
            };

            yield return new CryptoCaseSource
            {
                Expected = new byte[] {  85,   2, 102,   4, 118,   6, 115,   8,  93,  10 },

                Input    = new byte[] {   1,   2,   3,   4,   5,   6,   7,   8,   9,  10 }
            };

            yield return new CryptoCaseSource
            {
                Expected = new byte[] {  85,   2, 102,   4, 118,   6, 115,   8,  93, 10,
                                        100,   2, 112,   4, 113,   6,  83,   8, 108, 10,
                                        114,   2, 119,   4,  81,   6,  98,   8, 122, 10 },

                Input    = new byte[] {   1,   2,   3,   4,   5,   6,   7,   8,   9, 10,
                                          1,   2,   3,   4,   5,   6,   7,   8,   9, 10,
                                          1,   2,   3,   4,   5,   6,   7,   8,   9, 10 }
            };
        }

        [TestCaseSource("TestDecryptCaseSource")]
        public void TestDecrypt(CryptoCaseSource caseSource)
        {
            var inputData = new byte[caseSource.Input.Length];
            caseSource.Input.CopyTo(inputData, 0);

            var plain = xor.Decrypt(caseSource.Input);

            // Should not change the input data.
            Compare(inputData, caseSource.Input);

            // Verify plain.
            Compare(caseSource.Expected, plain);
        }
        
        public static IEnumerable<CryptoCaseSource> TestEncryptCaseSource()
        {
            yield return new CryptoCaseSource
            {
                Expected = new byte[] {  47,  10, 118 },

                Input    = new byte[] { 123,  10,  19 }
            };

            yield return new CryptoCaseSource
            {
                Expected = new byte[] {  85,   2, 102,   4, 118,   6, 115,   8,  93,  10 },

                Input    = new byte[] {   1,   2,   3,   4,   5,   6,   7,   8,   9,  10 }
            };

            yield return new CryptoCaseSource
            {
                Expected = new byte[] {  85,   2, 102,   4, 118,   6, 115,   8,  93, 10,
                                        100,   2, 112,   4, 113,   6,  83,   8, 108, 10,
                                        114,   2, 119,   4,  81,   6,  98,   8, 122, 10 },

                Input    = new byte[] {   1,   2,   3,   4,   5,   6,   7,   8,   9, 10,
                                          1,   2,   3,   4,   5,   6,   7,   8,   9, 10,
                                          1,   2,   3,   4,   5,   6,   7,   8,   9, 10 }
            };
        }

        public void TestEncrypt(CryptoCaseSource caseSource)
        {
            var inputData = new byte[caseSource.Input.Length];
            caseSource.Input.CopyTo(inputData, 0);

            var cipher = xor.Encrypt(caseSource.Input);

            // Should not change the input data.
            Compare(inputData, caseSource.Input);

            // Verify cipher.
            Compare(caseSource.Expected, cipher);
        }
    }
}
