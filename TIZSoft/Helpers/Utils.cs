using System;
using System.Collections.Generic;

namespace Tizsoft.Helpers
{
    public class Utils
    {
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