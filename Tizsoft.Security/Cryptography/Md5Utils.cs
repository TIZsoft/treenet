using System.Text;

namespace Tizsoft.Security.Cryptography
{
    public class Md5Utils
    {
        public static string ToHexString(byte[] contents)
        {
            var sb = new StringBuilder();

            foreach (var t in contents)
                sb.Append(t.ToString("x2"));

            return sb.ToString();
        }

        public static string ToHexString(byte[] contents, int from, int count)
        {
            return ToHexString(contents).Substring(from, count);
        }
    }
}