using System;

namespace Tizsoft
{
    public static class GuidUtil
    {
        public static Guid New()
        {
            return Guid.NewGuid();
        }

        public static Guid Get(string guidValue)
        {
            return new Guid(guidValue);
        }

        public static string ToBase64(Guid guid)
        {
            return ToBase64(guid.ToByteArray());
        }

        public static string ToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static Guid FromBase64(string value)
        {
            var guidBytes = Convert.FromBase64String(value);
            return new Guid(guidBytes);
        }
    }
}
