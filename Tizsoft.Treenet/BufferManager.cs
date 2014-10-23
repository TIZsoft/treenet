using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tizsoft.Log;

namespace Tizsoft.Treenet
{
    /// <summary>
    /// This class creates a single large buffer which can be divided up
    /// and assigned to SocketAsyncEventArgs objects for use with each
    /// socket I/O operation.
    /// This enables bufffers to be easily reused and guards against
    /// fragmenting heap memory.
    /// 
    /// The operations exposed on the BufferManager class are not thread safe.
    /// </summary>
    public class BufferManager
    {
        // The underlying byte array maintained by the Buffer Manager.
        byte[] _buffer;

        // The total number of bytes controlled by the buffer pool.
        int _segmentSize;

        int _currentSegmentIndex;

        readonly Stack<int> _segmentIndexPool = new Stack<int>();
        
        public int BufferSize { get; private set; }

        /// <summary>
        /// Allocates buffer space used by the buffer pool.
        /// </summary>
        /// <param name="segmentCount"></param>
        /// <param name="segmentSize"></param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="segmentCount"/> or <see cref="segmentSize"/> are less than or equal to zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// <see cref="segmentCount"/> multiplies <see cref="segmentSize"/> is greater than <see cref="System.Int32.MaxValue"/>.
        /// </exception>
        /// <remarks>
        /// This method may throws an <see cref="OutOfMemoryException"/> when requires too large memory block.
        /// </remarks>
        public void InitBuffer(int segmentCount, int segmentSize)
        {
            if (segmentCount <= 0)
            {
                throw new ArgumentOutOfRangeException("segmentCount", "Buffer count is less than or equal to zero.");
            }

            if (segmentSize <= 0)
            {
                throw new ArgumentOutOfRangeException("segmentSize", "Buffer size is less than or equal to zero.");
            }

            _segmentSize = segmentSize;
            BufferSize = checked(segmentCount * segmentSize);
            _buffer = new byte[BufferSize];
            _segmentIndexPool.Clear();
        }

        /// <summary>
        /// Assigns a buffer from the buffer pool to the  
        /// specified SocketAsyncEventArgs object.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>If the buffer was set successfully, then returns true, otherwise returns false.</returns>
        public bool SetBuffer(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                return false;
            }

            try
            {
                if (_segmentIndexPool.Count > 0)
                {
                    var offset = _segmentIndexPool.Pop();
                    e.SetBuffer(_buffer, offset, _segmentSize);
                }
                else
                {
                    var remaingingBufferSize = BufferSize - _segmentSize;

                    if (remaingingBufferSize < _currentSegmentIndex)
                    {
                        return false;
                    }

                    e.SetBuffer(_buffer, _currentSegmentIndex, _segmentSize);
                    _currentSegmentIndex += _segmentSize;
                }
            }
            catch (Exception ex)
            {
                GLogger.Fatal(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.<br />
        /// This frees the buffer back to the buffer pool.
        /// </summary>
        /// <param name="e"></param>
        public void FreeBuffer(SocketAsyncEventArgs e)
        {
            if (e != null)
            {
                _segmentIndexPool.Push(e.Offset);
            }
        }
    }
}