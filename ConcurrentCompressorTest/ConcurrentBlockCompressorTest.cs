using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using BlockCompressor;
using ConcurrentCompressor.ConcurrentBlockCompressor;
using ConcurrentCompressor.ConcurrentBlockDecompressor;
using Core.ConcurrentQueue;
using Core.ThreadPool;
using FluentAssertions;
using NUnit.Framework;

namespace ConcurrentCompressorTest
{
    public class ConcurrentBlockCompressorTest
    {
        private ConcurrentBlockCompressor _concurrentBlockCompressor;
        private ConcurrentBlockDecompressor _concurrentBlockDecompressor;

        private string _tempInFileName;
        private string _tempOutFileName;
        private string _tempDecompressFileName;

        [SetUp]
        public void Setup()
        {
            _tempInFileName = Path.GetTempFileName();
            _tempOutFileName = Path.GetTempFileName();

            _concurrentBlockCompressor = new ConcurrentBlockCompressor(
                new Compressor(),
                _tempInFileName,
                _tempOutFileName,
                20
                );
            _tempDecompressFileName = Path.GetTempFileName();
            _concurrentBlockDecompressor = new ConcurrentBlockDecompressor(
                new Compressor(),
                _tempOutFileName,
                _tempDecompressFileName
            );
        }

        [Test]
        public void CompressDecompressTest()
        {
            File.WriteAllText(_tempInFileName, GeneratePayload(100));
            _concurrentBlockCompressor.Compress();
            _concurrentBlockDecompressor.Decompress();

            var actualBytes = File.ReadAllBytes(_tempDecompressFileName);
            var expectedBytes = File.ReadAllBytes(_tempInFileName);

            actualBytes.Should().BeEquivalentTo(expectedBytes);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_tempInFileName);
            File.Delete(_tempOutFileName);
            File.Delete(_tempDecompressFileName);
        }

        private string GeneratePayload(int size)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                sb.Append("a");
            }

            return sb.ToString();
        }
    }
}