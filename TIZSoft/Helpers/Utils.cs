using System;
using System.Collections.Generic;

namespace Tizsoft.Helpers
{
    public class Utils
    {
        /// <summary>
        /// The TIMESTAMP data type is used for values that contain both date and time parts. 
        /// Min value is '1970-01-01 00:00:01' UTC.
        /// </summary>
        public static DateTime MySqlTimeStampMinValue { get { return new DateTime(1970, 1, 1, 0, 0, 1); } }
        /// <summary>
        /// The TIMESTAMP data type is used for values that contain both date and time parts. 
        /// Max value is '2038-01-19 03:14:07' UTC.
        /// </summary>
        public static DateTime MySqlTimeStampMaxValue { get { return new DateTime(2038, 1, 19, 3, 14, 7); } }
        /// <summary>
        /// The DATE type is used for values with a date part but no time part. 
        /// MySQL retrieves and displays DATE values in 'YYYY-MM-DD' format. 
        /// The min value is '1000-01-01'.
        /// </summary>
        public static DateTime MySqlDateMinValue { get { return new DateTime(1000, 1, 1); } }
        /// <summary>
        /// The DATE type is used for values with a date part but no time part. 
        /// MySQL retrieves and displays DATE values in 'YYYY-MM-DD' format. 
        /// The max value is '1000-01-01' to '9999-12-31'.
        /// </summary>
        public static DateTime MySqlDateMaxValue { get { return new DateTime(9999, 12, 31); } }
        /// <summary>
        /// The DATETIME type is used for values that contain both date and time parts. 
        /// MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format. 
        /// The min value is '1000-01-01 00:00:00'.
        /// </summary>
        public static DateTime MySqlDateTimeMinValue { get { return new DateTime(1000, 1, 1, 0, 0, 0);} }
        /// <summary>
        /// The DATETIME type is used for values that contain both date and time parts. 
        /// MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format. 
        /// The max value is '1000-01-01 00:00:00' to '9999-12-31 23:59:59'.
        /// </summary>
        public static DateTime MySqlDateTimeMaxValue { get { return new DateTime(9999, 12, 31, 23, 59, 59);} }

        public static bool IsBitSet(int number, int index)
        {
            if (index < 0 || index > sizeof(int))
                return false;

            return (number & (1 << index % 32)) != 0;
        }

        public static int SetBit(int number, int index, bool bitState)
        {
            if (index < 0 || index > sizeof(int))
                return number;

            return bitState ? number | (1 << index % 32) : number & ~(1 << index % 32);
        }

        /// <summary>
        /// Split up an array into a list of segments by segment size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="segmentSize">The segment size.</param>
        /// <returns>The result of splited source.</returns>
        /// <remarks>
        /// This helper method is mostly used to <see cref="System.Net.Sockets.SocketAsyncEventArgs.BufferList"/>.
        /// </remarks>
        /// <example>
        /// using Tizsoft.Helpers;
        /// 
        /// namespace Example
        /// {
        ///     static class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             var source = new byte[1024];
        ///             var segments1 = Utils.Split(source, 256);
        ///             var segments2 = Utils.Split(source, 500);
        /// 
        ///             // Output: 
        ///             // segments1.Count = 4
        ///             // segments1.Count = 3
        ///             Console.WriteLine("segments1.Count = {0}", segments1.Count);
        ///             Console.WriteLine("segments2.Count = {0}", segments2.Count);
        ///         }
        ///     }
        /// }
        /// </example>
        public static IList<ArraySegment<T>> Split<T>(T[] source, int segmentSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (segmentSize <= 0)
            {
                throw new ArgumentOutOfRangeException("source", "Segment size must greater than zero.");
            }

            var segmentCount = source.Length / segmentSize;
            var remaining = source.Length - segmentCount * segmentSize;
            var segments = new ArraySegment<T>[segmentCount + (remaining > 0 ? 1 : 0)];

            for (var i = 0; i != segmentCount; ++i)
            {
                segments[i] = new ArraySegment<T>(source, i * segmentSize, segmentSize);
            }

            if (remaining > 0)
            {
                var remainingIndex = segments.Length - 1;
                segments[remainingIndex] = new ArraySegment<T>(source, remainingIndex * segmentSize, remaining);
            }

            return segments;
        }

        /// <summary>
        /// Split up an array into a list of array by segment size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="segmentSize"></param>
        /// <returns></returns>
        public static IList<T[]> SplitArray<T>(T[] source, int segmentSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (segmentSize <= 0)
            {
                throw new ArgumentOutOfRangeException("source", "Segment size must greater than zero.");
            }

            var segmentCount = source.Length / segmentSize;
            var remaining = source.Length - segmentCount * segmentSize;
            var segments = new T[segmentCount + (remaining > 0 ? 1 : 0)][];

            for (var i = 0; i != segmentCount; ++i)
            {
                var segment = new T[segmentSize];
                Array.Copy(source, i * segmentSize, segment, 0, segmentSize);
                segments[i] = segment;
            }

            if (remaining > 0)
            {
                var remainingIndex = segments.Length - 1;
                var segment = new T[remaining];
                Array.Copy(source, remainingIndex * segmentSize, segment, 0, remaining);
                segments[remainingIndex] = segment;
            }

            return segments;
        }
    }
}