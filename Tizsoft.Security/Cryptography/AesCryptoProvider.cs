using System;
using System.IO;
using System.Security.Cryptography;

namespace Tizsoft.Security.Cryptography
{
    /// <summary>
    ///     Represents an Advanced Encryption Standard (AES) cryptography service provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This algorithm supports key lengths of 128, 192 or 256 bits.
    ///     </para>
    /// </remarks>
    public class AesCryptoProvider : ICryptoProvider
    {
        readonly Aes _aes = new AesCryptoServiceProvider();

        /// <summary>
        ///     IV length is 16 bytes.
        /// </summary>
        public byte[] IV
        {
            get { return _aes.IV; }
            set { _aes.IV = value; }
        }

        public byte[] Key
        {
            get { return _aes.Key; }
            set { _aes.Key = value; }
        }

        public AesCryptoProvider()
            : this(CipherMode.CBC)
        {

        }

        public AesCryptoProvider(CipherMode cipherMode)
        {
            _aes.Mode = cipherMode;
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
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (plain.Length - offset < count)
            {
                throw new ArgumentException();
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var encryptor = _aes.CreateEncryptor())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plain, offset, count);
                }

                return memoryStream.ToArray();
            }
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
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (cipher.Length - offset < count)
            {
                throw new ArgumentException();
            }

            using (var memoryStream = new MemoryStream(cipher, offset, count))
            {
                using (var decryptor = _aes.CreateDecryptor())
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    var plain = new byte[count];
                    cryptoStream.Read(plain, offset, count);
                    return plain;
                }
            }
        }
    }
}
