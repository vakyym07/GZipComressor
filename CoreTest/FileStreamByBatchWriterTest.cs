using System.IO;
using Core.FileStreamTools;
using FluentAssertions;
using NUnit.Framework;

namespace CoreTest
{
    public class FileStreamByBatchWriterTest
    {
        private string _tempInFileName;
        private IFileStreamBatchEnumerator _fileStreamBatchEnumerator;
        private FileStreamByBatchWriter _fileStreamByBatchWriter;
        private string _tempOutFileName;

        [SetUp]
        public void Setup()
        {
            _tempInFileName = Path.GetTempFileName();
            _tempOutFileName = Path.GetTempFileName();

            _fileStreamBatchEnumerator = new FileStreamBatchEnumerator(_tempInFileName, 2);
            _fileStreamByBatchWriter = new FileStreamByBatchWriter(_tempOutFileName);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_tempInFileName);
            File.Delete(_tempOutFileName);
        }

        [Test]
        public void WriteBatchTest()
        {
            var content = "payloadpayloadpayloadpayloadpayloadpayload";
            File.WriteAllText(_tempInFileName, content);

            foreach (var tuple in _fileStreamBatchEnumerator.Enumerate())
            {
                _fileStreamByBatchWriter.WriteBatch(tuple.DataBatch);
            }

            var actualBytes = File.ReadAllBytes(_tempOutFileName);
            var expectedBytes = File.ReadAllBytes(_tempInFileName);

            actualBytes.Should().BeEquivalentTo(expectedBytes);
        }
    }
}