using System;
using System.Text;

namespace Tizsoft.Security.Cryptography
{
    public class XorCryptoServiceProvider : IXorCrypto
    {
        readonly byte[] _key;

        static byte[] XorBytes(byte[] key, byte[] rawData)
        {
            if (null == rawData)
            {
                throw new ArgumentNullException("rawData");
            }

            if (null == key)
            {
                throw new ArgumentNullException("key");
            }

            var count = rawData.Length;
            var result = new byte[count];
            var keylen = key.Length;
            for (var i = 0; i < count; i++)
            {
                var idx = i%keylen;
                result[i] = (byte)(rawData[i] ^ key[idx]);
            }

            return result;
        }

        public XorCryptoServiceProvider(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value is null, empty or white space", "key");
            }

            var byteConverter = new UnicodeEncoding();
            _key = byteConverter.GetBytes(key);
        }

        public XorCryptoServiceProvider(byte[] key)
        {
            if (null == key)
            {
                throw new ArgumentNullException("key");
            }

            if (key.Length == 0)
            {
                throw new ArgumentException("Length is zero", "key");
            }

            _key = key;
        }

        public byte[] Decrypt(byte[] data)
        {
            return XorBytes(_key, data);
        }

        public byte[] Encrypt(byte[] data)
        {
            return XorBytes(_key, data);
        }
    }
}
