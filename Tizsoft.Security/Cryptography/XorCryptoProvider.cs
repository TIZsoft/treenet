using System;
using System.Text;

namespace Tizsoft.Security.Cryptography
{
    public class XorCryptoProvider : ICryptoProvider
    {
        readonly byte[] _key;

        static byte[] XorBytes(byte[] key, byte[] input, int offset, int count)
        {
            if (null == input)
            {
                throw new ArgumentNullException("input");
            }

            if (null == key)
            {
                throw new ArgumentNullException("key");
            }

            if (offset < 0 || offset > input.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0 || offset + count > input.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // DO NOT change the content of input.
            var keylen = key.Length;
            var ret = new byte[count];
            for (var i = 0; i < count; i++)
            {
                var keyidx = i%keylen;
                ret[i + offset] = (byte)(input[i + offset] ^ key[keyidx]);
            }

            return ret;
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

        public byte[] Decrypt(byte[] cipher)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            return Decrypt(cipher, 0, cipher.Length);
        }

        public byte[] Decrypt(byte[] cipher, int offset, int count)
        {
            return XorBytes(_key, cipher, offset, count);
        }

        public byte[] Encrypt(byte[] plain)
        {
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            return Encrypt(plain, 0, plain.Length);
        }

        public byte[] Encrypt(byte[] plain, int offset, int count)
        {
            return XorBytes(_key, plain, offset, count);
        }
    }
}