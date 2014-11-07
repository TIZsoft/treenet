using System.Security.Cryptography;

namespace Tizsoft.Security.Cryptography
{
    public static class AesUtils
    {
        public static byte[] GenerateKey(CipherMode cipherMode)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Mode = cipherMode;
                aes.GenerateKey();
                return aes.Key;
            }
        }
    }
}