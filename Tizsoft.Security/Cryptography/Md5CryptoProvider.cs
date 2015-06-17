using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Tizsoft.Security.Cryptography
{
    public class Md5CryptoProvider : ICryptoProvider
    {
        public byte[] Encrypt(byte[] plain)
        {
            if (plain == null || plain.Length == 0)
                throw new ArgumentNullException();

            using (var md5 = MD5.Create())
                return md5.ComputeHash(plain);
        }

        public byte[] Encrypt(byte[] plain, int offset, int count)
        {
            var result = new List<byte>(Encrypt(plain));
            return result.Skip(offset).Take(count).ToArray();
        }

        public byte[] Decrypt(byte[] cipher)
        {
            throw new InvalidOperationException("MD5 can't be decrypt");
        }

        public byte[] Decrypt(byte[] cipher, int offset, int count)
        {
            throw new InvalidOperationException("MD5 can't be decrypt");
        }
    }
}
