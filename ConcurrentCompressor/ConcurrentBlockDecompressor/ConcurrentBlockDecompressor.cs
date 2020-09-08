using System;
using System.IO;
using System.Linq;
using System.Threading;
using BlockCompressor;
using Core;
using Core.FileStreamTools;
using Core.PriorityLock;
using Core.ThreadPool;

namespace ConcurrentCompressor.ConcurrentBlockDecompressor
{
    public class ConcurrentBlockDecompressor : IConcurrentBlockDecompressor
    {
        private readonly IProducerConsumerQueue _producerConsumerDecompressTask;
        private readonly IProducerConsumerQueue _producerConsumerWriteTask;
        private readonly GzipActionPerformer _gzipActionPerformer;


        private readonly ICompressor _compressor;
        private readonly IFileStreamBatchEnumerator _fileStreamBatchEnumerator;

        private readonly byte[] _gzipHeader = {31, 139, 8, 0, 0, 0, 0, 0, 0, 10};
        private readonly FileStreamByBatchWriter _fileStreamByBatchWriter;

        private readonly int _chunkSize;
        private readonly string _pathToFile;
        private readonly string _saveToFile;

        public ConcurrentBlockDecompressor(
            ICompressor compressor,
            string pathToFile,
            string saveToFile,
            int batchSize = 1024
        )
        {
            var workerCount = Environment.ProcessorCount * 5;
            _chunkSize = 100;

            _compressor = compressor;

            _saveToFile = saveToFile;
            _fileStreamByBatchWriter = new FileStreamByBatchWriter(_saveToFile);

            _producerConsumerDecompressTask = new ProducerConsumerQueue(workerCount);
            _producerConsumerWriteTask = new ProducerConsumerQueue();
            _gzipActionPerformer = new GzipActionPerformer(
                _producerConsumerDecompressTask,
                _producerConsumerWriteTask,
                _fileStreamByBatchWriter
            );


            _pathToFile = pathToFile;
            _fileStreamBatchEnumerator = new FileStreamBatchEnumerator(_pathToFile, batchSize);
        }

        public Result Decompress()
        {
            var performResult = Result.CreateSuccess();
            try
            {
                if (!IsValidGZipHeader())
                {
                    throw new InvalidDataException("This file has unknown format. Decompression is impossible");
                }

                foreach (var chunk in _fileStreamBatchEnumerator.Split(_gzipHeader).SplitToChunks(_chunkSize))
                {
                    performResult = _gzipActionPerformer.Perform(_compressor.Decompress, chunk);
                    if (!performResult.IsSuccess)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                performResult = Result.CreateFail(e.Message);
                _fileStreamByBatchWriter.Dispose();
                File.Delete(_saveToFile);
            }
            finally
            {
                _producerConsumerDecompressTask.ShutDown();
                _producerConsumerWriteTask.ShutDown();
                _fileStreamByBatchWriter.Dispose();
            }

            return performResult;
        }

        private bool IsValidGZipHeader()
        {
            using var fileStream = File.OpenRead(_pathToFile);
            var gzipHeader = new byte[_gzipHeader.Length];
            fileStream.Read(gzipHeader, 0, gzipHeader.Length);
            return gzipHeader.StartWith(_gzipHeader);
        }
    }
}