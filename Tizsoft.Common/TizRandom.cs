using System;
using System.Security.Cryptography;
using System.Text;

namespace Tizsoft
{
    public class TizRandom
    {
        static readonly RandomNumberGenerator _generator = new RNGCryptoServiceProvider();
        static readonly byte[] RandomBytes = new byte[4];
        const string AvailableRandomChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// generate one non-negative integer
        /// </summary>
        public static int Next()
        {
            _generator.GetBytes(RandomBytes);
            var value = BitConverter.ToInt32(RandomBytes, 0);
            return value < 0 ? -value : value;
        }

        /// <summary>
        /// generate one non-negative interger less than max(excluded)
        /// </summary>
        /// <param name="max">max value</param>
        public static int Next(int max)
        {
            _generator.GetBytes(RandomBytes);
            var value = BitConverter.ToInt32(RandomBytes, 0);
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

        /// <summary>
        /// generate one unsigned interger less than max(excluded)
        /// </summary>
        /// <param name="max">max value</param>
        public static uint Next(uint max)
        {
            _generator.GetBytes(RandomBytes);
            var value = BitConverter.ToUInt32(RandomBytes, 0);
            return value % max;
        }

        /// <summary>
        /// generate one unsigned integer between min(included) and max(excluded)
        /// </summary>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        public static uint Next(uint min, uint max)
        {
            return Next(max - min) + min;
        }

        public static string RandomString(int length)
        {
            var stringBuilder = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                stringBuilder.Append(AvailableRandomChars[Next(AvailableRandomChars.Length)]);
            return stringBuilder.ToString();
        }
    }
}