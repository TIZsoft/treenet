using System;
using System.Text;

namespace Tizsoft.Security.Cryptography
{
    public class XorCryptoProvider : ICryptoProvider
    {
        readonly byte[] _key;

        static byte[] XorBytes(byte[] key, byte[] rawData, int offset, int count)
        {
            if (null == rawData)
            {
                throw new ArgumentNullException("rawData");
            }

            if (null == key)
            {
                throw new ArgumentNullException("key");
            }

            if (offset < 0 || offset > rawData.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0 || offset + count > rawData.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var keylen = key.Length;
            for (var i = 0; i < count; i++)
            {
                var keyidx = i%keylen;
                rawData[i + offset] = (byte)(rawData[i + offset] ^ key[keyidx]);
            }

            return rawData;
        }

        public XorCryptoProvider(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value is null, empty or white space", "key");
            }

            var byteConverter = new UnicodeEncoding();
            _key = byteConverter.GetBytes(key);
        }

        public XorCryptoProvider(byte[] key)
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
            return Decrypt(data, 0, data.Length);
        }

        public byte[] Decrypt(byte[] data, int offset, int count)
        {
            return XorBytes(_key, data, offset, count);
        }

        public byte[] Encrypt(byte[] data)
        {
            return Encrypt(data, 0, data.Length);
        }

        public byte[] Encrypt(byte[] data, int offset, int count)
        {
            return XorBytes(_key, data, offset, count);
        }
    }
}