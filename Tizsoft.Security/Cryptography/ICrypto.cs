namespace Tizsoft.Security.Cryptography
{
    interface ICrypto
    {
        byte[] Encrypt(byte[] data, byte[] key);
        byte[] Decrypt(byte[] data, byte[] key);
    }
}
