using System.IO;
using System.IO.Compression;

namespace BlockCompressor
{
    public class Compressor : ICompressor
    {
        public byte[] Compress(byte[] dataToCompress)
        {
            using var compressedStream = new MemoryStream();
            using (var compressionStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                compressionStream.Write(dataToCompress);
            }
            return compressedStream.ToArray();
        }

        public byte[] Decompress(byte[] dataToDecompress)
        {
            using var compressedFile = new MemoryStream(dataToDecompress);
            using var decompressionStream = new GZipStream(compressedFile, CompressionMode.Decompress);
            using var decompressedStream = new MemoryStream();

            decompressionStream.CopyTo(decompressedStream);
            return decompressedStream.ToArray();
        }
    }
}