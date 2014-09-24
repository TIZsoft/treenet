using System;
using System.Collections.Generic;
using System.Net.Sockets;

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
        // The total number of bytes controlled by the buffer pool.
        int _totalBytes;

        // The underlying byte array maintained by the Buffer Manager.
        byte[] _buffer;

        readonly Stack<int> _freeIndexPool = new Stack<int>();
        int _currentIndex;
        int _bufferSize;

        // Allocates buffer space used by the buffer pool.
        public void InitBuffer(int totalBytes, int bufferSize)
        {
            try
            {
                _totalBytes = totalBytes;
                _currentIndex = 0;
                _bufferSize = bufferSize;
                _freeIndexPool.Clear();

                // Create one big large buffer and divide that  
                // out to each SocketAsyncEventArgs object.
                _buffer = new byte[_totalBytes];
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        /// <summary>
        /// Assigns a buffer from the buffer pool to the  
        /// specified SocketAsyncEventArgs object.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>If the buffer was set successfully, then returns true, otherwise returns false.</returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (_freeIndexPool.Count > 0)
            {
                args.SetBuffer(_buffer, _freeIndexPool.Pop(), _bufferSize);
            }
            else
            {
                if ((_totalBytes - _bufferSize) < _currentIndex)
                {
                    return false;
                }
                args.SetBuffer(_buffer, _currentIndex, _bufferSize);
                _currentIndex += _bufferSize;
            }
            return true;
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.<br />
        /// This frees the buffer back to the buffer pool.
        /// </summary>
        /// <param name="args"></param>
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            if (args != null)
            {
                _freeIndexPool.Push(args.Offset);
            }
        }
    }
}