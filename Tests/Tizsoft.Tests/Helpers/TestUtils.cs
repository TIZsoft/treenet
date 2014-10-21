using System;
using NUnit.Framework;
using System.Collections.Generic;
using Tizsoft.Helpers;

namespace Tizsoft.Tests.Helpers
{
    [TestFixture]
    public class TestUtils
    {
        #region TestSplit case source.

        public class TestSplitSource
        {
            public static T[] CreateSourceArray<T>(int sourceArraySize)
            {
                if (sourceArraySize < 0)
                {
                    return null;
                }

                return new T[sourceArraySize];
            }

            public int SourceArraySize { get; set; }

            public int SegmentSize { get; set; }
        }

        public static IEnumerable<TestSplitSource> TestCaseSourceForTestSplitInvalidArgs()
        {
            yield return new TestSplitSource
            {
                SourceArraySize = 0,
                SegmentSize = 0
            };

            yield return new TestSplitSource
            {
                SourceArraySize = 0,
                SegmentSize = -100
            };

            yield return new TestSplitSource
            {
                SourceArraySize = -1,
                SegmentSize = 100
            };

            yield return new TestSplitSource
            {
                SourceArraySize = -1,
                SegmentSize = -100
            };
        }

        public static IEnumerable<TestSplitSource> TestCaseSourceForTestSplit()
        {
            yield return new TestSplitSource
            {
                SourceArraySize = 1,
                SegmentSize = 1
            };

            yield return new TestSplitSource
            {
                SourceArraySize = 1024,
                SegmentSize = 1
            };

            yield return new TestSplitSource
            {
                SourceArraySize = 4096,
                SegmentSize = 1024
            };

            yield return new TestSplitSource
            {
                SourceArraySize = 4096,
                SegmentSize = 8192
            };

            yield return new TestSplitSource
            {
                SourceArraySize = 10000,
                SegmentSize = 256
            };
        }

        #endregion


        [TestCaseSource("TestCaseSourceForTestSplitInvalidArgs")]
        public void TestSplitInvalidArgs(TestSplitSource testSource)
        {
            Assert.Catch<ArgumentException>(() => Utils.Split(
                TestSplitSource.CreateSourceArray<byte>(testSource.SourceArraySize),
                testSource.SegmentSize)
            );
        }

        [TestCaseSource("TestCaseSourceForTestSplit")]
        public void TestSplit(TestSplitSource testSource)
        {
            var sourceArray = TestSplitSource.CreateSourceArray<byte>(testSource.SourceArraySize);
            var random = new Random();
            random.NextBytes(sourceArray);

            var segments = Utils.Split(sourceArray, testSource.SegmentSize);
            var expectedSegmentCount = testSource.SourceArraySize / testSource.SegmentSize;
            var remainingElementCount = testSource.SourceArraySize % testSource.SegmentSize;

            if (remainingElementCount != 0)
            {
                ++expectedSegmentCount;
            }

            Assert.AreEqual(expectedSegmentCount, segments.Count);

            // Make sure the sequence is ordered and correct.
            for (var segmentIndex = 0; segmentIndex != segments.Count; ++segmentIndex)
            {
                var segment = segments[segmentIndex];

                for (var arrayIndex = 0; arrayIndex != segment.Count; ++arrayIndex)
                {
                    var expected = sourceArray[segmentIndex * testSource.SegmentSize + arrayIndex];
                    var actual = segment.Array[segment.Offset + arrayIndex];
                    Assert.AreEqual(expected, actual);
                }
            }
        }
    }
}
