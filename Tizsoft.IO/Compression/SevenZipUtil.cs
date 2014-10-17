using System;
using System.IO;
using SevenZip;

namespace Tizsoft.IO.Compression
{
    public static class SevenZipUtil
    {
        public static void SetLibraryPath(string path)
        {
            SevenZipBase.SetLibraryPath(path);
        }

        public static void Compress(Stream inStream, Stream outStream, EventHandler<EventArgs> onFinished)
        {
            Compress(inStream, outStream,
                CompressionLevel.Normal, CompressionMethod.Default,
                onFinished);
        }

        public static void Compress(Stream inStream, Stream outStream,
            CompressionLevel level,
            CompressionMethod method,
            EventHandler<EventArgs> onFinished)
        {
            if (null == inStream)
            {
                throw new ArgumentNullException("inStream");
            }

            if (null == outStream)
            {
                throw new ArgumentNullException("outStream");
            }

            // setting
            var compressor = new SevenZipCompressor();
            if (null != onFinished)
            {
                compressor.CompressionFinished += onFinished;
            }
            compressor.CompressionLevel = level;
            compressor.CompressionMethod = method;

            // compress
            compressor.CompressStream(inStream, outStream);
        }

        public static void Extract(Stream inStream, Stream outStream, EventHandler<EventArgs> onFinished)
        {
            if (null == inStream)
            {
                throw new ArgumentNullException("inStream");
            }

            using (var extractor = new SevenZipExtractor(inStream))
            {
                // setting
                if (null != onFinished)
                {
                    extractor.ExtractionFinished += onFinished;
                }

                // extract
                extractor.ExtractFile(0, outStream);
            }
        }
    }
}