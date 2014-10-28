using System;

namespace Tizsoft.IO.Compression
{
    /// <summary>
    /// Provides compress and decompress operations.
    /// </summary>
    public interface ICompressProvider
    {
        /// <summary>
        /// Compress the raw data synchronously.
        /// </summary>
        /// <param name="buffer">Data to compress.</param>
        /// <returns>The compressed data.</returns>
        byte[] Compress(byte[] buffer);

        byte[] Compress(byte[] buffer, int offset, int length);

        /// <summary>
        /// Decompress the compressed data synchronously.
        /// </summary>
        /// <param name="buffer">The compressed data.</param>
        /// <returns>The decompressed data.</returns>
        byte[] Decompress(byte[] buffer);

        byte[] Decompress(byte[] buffer, int offset, int length);

        /// <summary>
        /// Compress the raw data asynchronously.
        /// </summary>
        /// <param name="buffer">Data to compress.</param>
        /// <param name="onDone">If the task of compression is done, then invoke the action.</param>
        void CompressAsync(byte[] buffer, Action<byte[]> onDone);

        void CompressAsync(byte[] buffer, int offset, int length, Action<byte[]> onDone);

        /// <summary>
        /// Decompress the compressed data asynchronously.
        /// </summary>
        /// <param name="buffer">The compressed data.</param>
        /// <param name="onDone">If the task of decompression is done, then invoke the action.</param>
        void DecompressAsync(byte[] buffer, Action<byte[]> onDone);

        void DecompressAsync(byte[] buffer, int offset, int length, Action<byte[]> onDone);
    }
}
