using System;
using System.Security.Cryptography;

namespace Tizsoft
{
    public class RngRandom
    {
        static readonly RandomNumberGenerator _generator = new RNGCryptoServiceProvider();
        static readonly byte[] _randomBytes = new byte[4];

        /// <summary>
        /// generate one non-negative integer
        /// </summary>
        public static int Next()
        {
            _generator.GetBytes(_randomBytes);
            int value = BitConverter.ToInt32(_randomBytes, 0);
            return value < 0 ? -value : value;
        }

        /// <summary>
        /// generate one non-negative interger less than max(excluded)
        /// </summary>
        /// <param name="max">max value</param>
        public static int Next(int max)
        {
            _generator.GetBytes(_randomBytes);
            int value = BitConverter.ToInt32(_randomBytes, 0);
            value %= max;
            return value < 0 ? -value : value;
        }

        /// <summary>
        /// generate one non-negative integer between min(included) and max(excluded)
        /// </summary>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        public static int Next(int min, int max)
        {
            return Next(max - min) + min;
        }
    }
}