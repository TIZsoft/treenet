using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Tizsoft.Security.Cryptography;

namespace Tizsoft.Security.Tests.Cryptography
{
    [TestFixture]
    public class TestAesCryptoProvider
    {
        public class CryptoCaseSource
        {
            public CipherMode Mode { get; set; }

            public int KeySize { get; set; }

            public string PlainText { get; set; }

            public byte[] Key { get; private set; }

            public byte[] IV { get; private set; }

            public byte[] Plain { get; private set; }

            public byte[] Cipher { get; private set; }

            public void GenerateKey()
            {
                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Mode = Mode;
                    aes.KeySize = KeySize;
                    Key = aes.Key;
                }
            }

            public void GenerateIV()
            {
                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Mode = Mode;
                    aes.KeySize = KeySize;
                    IV = aes.IV;
                }
            }

            public void GeneratePlain()
            {
                Plain = Encoding.UTF8.GetBytes(PlainText);
            }

            public void GenerateCipher()
            {
                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Mode = Mode;
                    aes.KeySize = KeySize;

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var encryptor = aes.CreateEncryptor(Key, IV))
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(Plain, 0, PlainText.Length);
                        }

                        Cipher = memoryStream.ToArray();
                    }
                }
            }
        }

        public IEnumerable<CryptoCaseSource> CryptoCaseSources()
        {
            yield return new CryptoCaseSource
            {
                Mode = CipherMode.CBC,
                KeySize = 128,
                PlainText = string.Empty
            };

            yield return new CryptoCaseSource
            {
                Mode = CipherMode.CBC,
                KeySize = 256,
                PlainText = string.Empty
            };

            yield return new CryptoCaseSource
            {
                Mode = CipherMode.CBC,
                KeySize = 128,
                PlainText = "A"
            };

            yield return new CryptoCaseSource
            {
                Mode = CipherMode.CBC,
                KeySize = 256,
                PlainText = "A"
            };

            yield return new CryptoCaseSource
            {
                Mode = CipherMode.CBC,
                KeySize = 128,
                PlainText = "Test1234"
            };

            yield return new CryptoCaseSource
            {
                Mode = CipherMode.CBC,
                KeySize = 256,
                PlainText = "Test1234"
            };

            yield return new CryptoCaseSource
            {
                Mode = CipherMode.CBC,
                KeySize = 128,
                PlainText = "Test1234567890"
            };

            yield return new CryptoCaseSource
            {
                Mode = CipherMode.CBC,
                KeySize = 256,
                PlainText = "Test1234567890"
            };
        }

        [TestCaseSource("CryptoCaseSources")]
        public void TestCrypto(CryptoCaseSource caseSource)
        {
            caseSource.GeneratePlain();
            caseSource.GenerateKey();
            caseSource.GenerateIV();
            caseSource.GenerateCipher();

            Debug.Print("Key                ={0}\n", GetByteString(caseSource.Key));
            Debug.Print("IV                 ={0}\n", GetByteString(caseSource.IV));
            
            var aes = new AesCryptoProvider(caseSource.Mode)
            {
                Key = caseSource.Key,
                IV = caseSource.IV
            };

            Debug.Print("PlainText          =\"{0}\"\n", caseSource.PlainText);
            
            var expectedCipher = caseSource.Cipher;
            var actualCipher = aes.Encrypt(caseSource.Plain);

            Debug.Print("ExpectedCipher     ={0}\n", GetByteString(expectedCipher));
            Debug.Print("ActualCipher       ={0}\n", GetByteString(actualCipher));

            for (var i = 0; i != expectedCipher.Length; ++i)
            {
                if (expectedCipher[i] != actualCipher[i])
                {
                    Assert.Fail("Cipher does not matched.");
                }
            }

            var expectedPlain = caseSource.Plain;
            var actualPlain = aes.Decrypt(actualCipher);

            Debug.Print("ActualPlainText    =\"{0}\"\n", Encoding.UTF8.GetString(actualPlain));

            for (var i = 0; i != expectedPlain.Length; ++i)
            {
                if (expectedPlain[i] != actualPlain[i])
                {
                    Assert.Fail("PlainText does not matched.");
                }
            }
        }

        static string GetByteString(byte[] bytes)
        {
            if (bytes == null)
            {
                return "null";
            }

            var sb = new StringBuilder("\t");
            for (var i = 0; i != bytes.Length; ++i)
            {
                if (i != 0 &&
                    i % 16 == 0)
                {
                    sb.Append("\n\t\t\t");
                }

                sb.AppendFormat("{0:x2} ", bytes[i]);
            }
            return sb.ToString();
        }
    }
}
