using System;
using System.IO;
using System.Security.Cryptography;

namespace Tizsoft.Security.Cryptography
{
    /// <summary>
    /// Represents a Rijndael cryptography service provider.
    /// </summary>
    /// <remarks>
    /// This algorithm supports key lengths of 128, 192 or 256 bits.
    /// </remarks>
    public class RijndaelCryptoProvider : ICryptoProvider, IDisposable
    {
        readonly Rijndael _rijndael = new RijndaelManaged();
        readonly ICryptoTransform _rijndaelEncryptor;
        readonly ICryptoTransform _rijndaelDecryptor;
        readonly MemoryStream _encryptMemoryStream = new MemoryStream();
        readonly MemoryStream _decryptMemoryStream = new MemoryStream();

        public Rijndael Rijndael { get { return _rijndael; } }

        public RijndaelCryptoProvider(CipherMode cipherMode, byte[] IV, byte[] key)
        {
            if (IV == null)
            {
                throw new ArgumentNullException("IV");
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            _rijndael.Mode = cipherMode;
            _rijndael.IV = IV;
            _rijndael.Key = key;
            _rijndaelEncryptor = _rijndael.CreateEncryptor();
            _rijndaelDecryptor = _rijndael.CreateDecryptor();
        }

        ~RijndaelCryptoProvider()
        {
            Dispose(false);
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

            _encryptMemoryStream.Seek(0, SeekOrigin.Begin);
            _encryptMemoryStream.SetLength(count);
            using (var cryptoStream = new CryptoStream(_encryptMemoryStream, _rijndaelEncryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Seek(0, SeekOrigin.Begin);
                cryptoStream.Write(plain, offset, count);
                return _encryptMemoryStream.ToArray();
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

            _decryptMemoryStream.Seek(0, SeekOrigin.Begin);
            _decryptMemoryStream.SetLength(count);
            using (var cryptoStream = new CryptoStream(_decryptMemoryStream, _rijndaelDecryptor, CryptoStreamMode.Read))
            {
                //var plain = new byte[];
                //cryptoStream.Read()
                return _decryptMemoryStream.ToArray();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_encryptMemoryStream != null)
            {
                _encryptMemoryStream.Dispose();
            }

            if (_decryptMemoryStream != null)
            {
                _decryptMemoryStream.Dispose();
            }

            if (_rijndaelEncryptor != null)
            {
                _rijndaelEncryptor.Dispose();
            }

            if (_rijndaelDecryptor != null)
            {
                _rijndaelDecryptor.Dispose();
            }

            if (_rijndael != null)
            {
                _rijndael.Dispose();
            }
        }
    }
}
