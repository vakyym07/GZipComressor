using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.FileStreamTools;
using FluentAssertions;
using NUnit.Framework;

namespace CoreTest
{
    public class FileStreamBatchEnumeratorTest
    {
        private FileStreamBatchEnumerator _fileStreamBatchEnumerator;
        private string _tempFileName;

        [SetUp]
        public void Setup()
        {
            _tempFileName = Path.GetTempFileName();
            _fileStreamBatchEnumerator = new FileStreamBatchEnumerator(_tempFileName, 30);
        }

        [Test]
        public void EnumeratorTest()
        {
            var content = "payloadpayloadpayloadpayloadpayloadpayload";
            File.WriteAllText(_tempFileName, content);

            var actualBytes = _fileStreamBatchEnumerator.Enumerate()
                .SelectMany(x => x.DataBatch)
                .ToArray();

            var expectedBytes = File.ReadAllBytes(_tempFileName);

            actualBytes.Should().BeEquivalentTo(expectedBytes);
        }

        [TestCaseSource(nameof(GenerateTestData))]
        public void SplitTest(byte[] content, byte[][] expectedBytes)
        {
            File.WriteAllBytes(_tempFileName, content);

            var actualBytes = _fileStreamBatchEnumerator.Split(new byte[] {31, 139, 8})
                .Where(x => x.DataBatch.Length != 0)
                .Select(x => x.DataBatch)
                .ToArray();

            actualBytes.Should().BeEquivalentTo(expectedBytes);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_tempFileName);
        }

        private static IEnumerable<TestCaseData> GenerateTestData()
        {
            yield return new TestCaseData(new byte[] {31, 139, 8, 24, 25, 31, 39, 31, 139, 8, 139, 2}, new[]
            {
                new byte[] {31, 139, 8, 24, 25, 31, 39},
                new byte[] {31, 139, 8, 139, 2}
            });

            yield return new TestCaseData(new byte[] {31, 139, 8, 24, 25, 31, 139, 31, 139, 8, 139, 2}, new[]
            {
                new byte[] {31, 139, 8, 24, 25, 31, 139},
                new byte[] {31, 139, 8, 139, 2}
            });

            yield return new TestCaseData(new byte[] {31, 139, 8, 24, 25, 31, 39, 31, 139, 0, 139, 2}, new[]
            {
                new byte[] {31, 139, 8, 24, 25, 31, 39, 31, 139, 0, 139, 2}
            });

            yield return new TestCaseData(new byte[] {}, Array.Empty<byte[]>());
        }
    }
}