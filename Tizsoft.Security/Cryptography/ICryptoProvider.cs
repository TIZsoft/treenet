namespace Tizsoft.Security.Cryptography
{
    public interface ICryptoProvider
    {
        byte[] Encrypt(byte[] plain);

        byte[] Encrypt(byte[] plain, int offset, int count);

        byte[] Decrypt(byte[] cipher);

        byte[] Decrypt(byte[] cipher, int offset, int count);
    }
}
