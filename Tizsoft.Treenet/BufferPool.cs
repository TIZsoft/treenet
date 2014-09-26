using System;
using System.Collections.Concurrent;

namespace Tizsoft.Treenet
{
    /// <summary>
    /// Represents a wrapped array to divide it in segments to be used.
    /// This enables bufffers to be easily reused and reduces memory fragmentation.
    /// </summary>
    class BufferPool
    {
        public class Segment
        {
            public int Index { get; private set; }

            public int Length { get; private set; }

            BufferPool Owner { get; set; }

            public byte[] Buffer { get { return Owner.Buffer; } }

            internal Segment(int index, int length, BufferPool owner)
            {
                Index = index;
                Length = length;
                Owner = owner;
            }

            public void Free()
            {
                Owner.FreeSegment(this);
            }
        }

        int _bufferSize;
        byte[] _buffer;
        readonly ConcurrentStack<Segment> _freeSegments = new ConcurrentStack<Segment>();

        public byte[] Buffer { get { return _buffer; } }
        
        public void Initialize(int initialSegmentCount, int bufferSize)
        {
            if (initialSegmentCount <= 0)
            {
                throw new ArgumentOutOfRangeException("initialSegmentCount", initialSegmentCount, "Initial segment count must greater than zero.");
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Buffer size must greater than zero.");
            }

            var totalBufferSize = initialSegmentCount * bufferSize;
            _bufferSize = bufferSize;
            _buffer = new byte[totalBufferSize];
            AllocateSegments(0, totalBufferSize, bufferSize);
        }

        /// <summary>
        /// Gets an unused segment.
        /// </summary>
        /// <example>
        /// readonly BufferPool _pool = new BufferPool();
        /// 
        /// void Setup()
        /// {
        ///     _pool.Initialize(8192, 4096);
        /// }
        /// 
        /// void SetBuffer(SocketAsyncEventArgs e)
        /// {
        ///     var segment = _pool.GetSegment();
        ///     e.SetBuffer(segment.Buffer, segment.Index, segment.Length);
        /// }
        /// </example>
        /// <returns></returns>
        public Segment GetSegment()
        {
            while (true)
            {
                Segment segment;
                if (_freeSegments.TryPop(out segment))
                {
                    return segment;
                }

                GrowBuffer();
            }
        }

        void GrowBuffer()
        {
            var originalLength = _buffer.Length;
            Array.Resize(ref _buffer, originalLength + originalLength);
            AllocateSegments(originalLength, _buffer.Length, _bufferSize);
        }

        void AllocateSegments(int startIndex, int length, int bufferSize)
        {
            // TODO: Paralle?
            for (var i = startIndex; i < length; i += bufferSize)
            {
                _freeSegments.Push(new Segment(i, bufferSize, this));
            }
        }

        void FreeSegment(Segment segment)
        {
            if (segment != null)
            {
                _freeSegments.Push(segment);
            }
        }
    }
}
