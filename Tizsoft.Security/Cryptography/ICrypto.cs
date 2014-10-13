namespace Tizsoft.Security.Cryptography
{
    public interface IXorCrypto
    {
        byte[] Encrypt(byte[] data);
        byte[] Encrypt(byte[] data, int offset, int count);
        byte[] Decrypt(byte[] data);
        byte[] Decrypt(byte[] data, int offset, int count);
    }
}
