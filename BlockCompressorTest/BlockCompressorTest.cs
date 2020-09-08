using System;
using System.IO;
using BlockCompressor;
using FluentAssertions;
using NUnit.Framework;

namespace BlockCompressorTest
{
    public class BlockCompressorTest
    {
        private Compressor _compressor;
        private string _tempFileName;

        [SetUp]
        public void Setup()
        {
            _compressor = new Compressor();
            _tempFileName = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_tempFileName);
        }

        [Test]
        public void TestCompressDecompress()
        {
            var content = "payloadpayloadpayloadpayloadpayloadpayload";

            File.WriteAllText(_tempFileName, content);

            var expectedBytes = File.ReadAllBytes(_tempFileName);
            var actualBytes = CompressDecompress(expectedBytes);

            actualBytes.Should().BeEquivalentTo(expectedBytes);
        }

        private byte[] CompressDecompress(byte[] originalFile)
        {
            var compressFile = _compressor.Compress(originalFile);
            var decompressFile = _compressor.Decompress(compressFile);
            return decompressFile;
        }
    }
}