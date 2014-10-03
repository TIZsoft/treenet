using System.Collections.Generic;

namespace Tizsoft.Security.Cryptography
{
    public interface IXorCrypto
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }
}
