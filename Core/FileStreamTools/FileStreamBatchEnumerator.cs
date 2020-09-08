using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.FileStreamTools
{
    public class FileStreamBatchEnumerator : IFileStreamBatchEnumerator
    {
        private readonly string _pathToFile;
        private readonly int _blockSize;

        public FileStreamBatchEnumerator(string pathToFile, int blockSize = 1024)
        {
            _pathToFile = pathToFile;
            _blockSize = blockSize;
        }

        public IEnumerable<(byte[] DataBatch, int BatchIndex)> Enumerate()
        {
            using var originFileStream = File.OpenRead(_pathToFile);
            var batchIndex = 0;

            foreach (var batch in InternalEnumerateByBatch(originFileStream))
            {
                yield return (batch, batchIndex);
                batchIndex++;
            }
        }

        public IEnumerable<(byte[] DataBatch, int BatchIndex)> Split(byte[] separator)
        {
            using var originFileStream = File.OpenRead(_pathToFile);

            if (originFileStream.Length == 0)
            {
                yield break;
            }

            var batchIndex = 0;
            var dataBatchPosition = separator.Length;
            var separatorStartPosition = -1;

            var dataBatch = new byte[_blockSize * 2];
            originFileStream.Read(dataBatch, 0, separator.Length);

            foreach (var readByte in InternalEnumerateByBatch(originFileStream).SelectMany(x => x))
            {
                if (readByte == separator[0])
                {
                    separatorStartPosition = dataBatchPosition;
                }

                dataBatch[dataBatchPosition++] = readByte;

                if (IsSeparator())
                {
                    var array = dataBatch.Take(separatorStartPosition).ToArray();
                    yield return (array, batchIndex);
                    batchIndex++;

                    dataBatch.ShiftToStart(separatorStartPosition, separator.Length);
                    dataBatchPosition = separator.Length;
                    separatorStartPosition = -1;
                }
            }

            yield return (dataBatch.Take(dataBatchPosition).ToArray(), batchIndex);

            bool IsSeparator()
            {
                return separatorStartPosition != -1
                       && dataBatchPosition - separatorStartPosition == separator.Length
                       && dataBatch.StartWith(separator, separatorStartPosition);
            }
        }

        private IEnumerable<byte[]> InternalEnumerateByBatch(FileStream stream)
        {
            var streamLength = stream.Length;
            while (true)
            {
                var remainPartOfStream = streamLength - stream.Position;
                var dataBatch = new byte[Math.Min(_blockSize, remainPartOfStream)];

                var readBytesCount = stream
                    .Read(dataBatch, 0, dataBatch.Length);
                if (readBytesCount == 0)
                {
                    break;
                }

                yield return dataBatch;
            }
        }

        private void IsValidHeaderOrThrowException()
        {

        }
    }
}