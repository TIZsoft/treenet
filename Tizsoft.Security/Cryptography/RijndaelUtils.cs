using System.Security.Cryptography;

namespace Tizsoft.Security.Cryptography
{
    public static class RijndaelUtils
    {
        public static void GenerateIVKey(CipherMode cipherMode, out byte[] IV, out byte[] key)
        {
            using (var rijndael = new RijndaelManaged())
            {
                rijndael.Mode = cipherMode;
                rijndael.GenerateIV();
                rijndael.GenerateKey();
                
                IV = rijndael.IV;
                key = rijndael.Key;
            }
        }
    }
}