using System;
using System.IO;
using System.Threading;
using BlockCompressor;
using Core;
using Core.ConcurrentQueue;
using Core.FileStreamTools;
using Core.PriorityLock;
using Core.ThreadPool;

namespace ConcurrentCompressor.ConcurrentBlockCompressor
{
    public class ConcurrentBlockCompressor : IConcurrentBlockCompressor
    {
        private readonly ICompressor _compressor;
        private readonly IFileStreamBatchEnumerator _fileStreamBatchEnumerator;

        private readonly ProducerConsumerQueue _producerConsumerCompressTask;
        private readonly ProducerConsumerQueue _producerConsumerWriteTask;
        private readonly GzipActionPerformer _gzipActionPerformer;
        private readonly FileStreamByBatchWriter _fileStreamByBatchWriter;

        private readonly int _chunkSize;
        private readonly string _saveToFile;

        public ConcurrentBlockCompressor(
            ICompressor compressor,
            string pathToFile,
            string saveToFile,
            int blockSize = 1024
            )
        {
            _saveToFile = saveToFile;
            var workerCount = Environment.ProcessorCount * 4;
            _chunkSize = 100;

            _compressor = compressor;
            _fileStreamByBatchWriter = new FileStreamByBatchWriter(saveToFile);

            _producerConsumerCompressTask = new ProducerConsumerQueue(workerCount);
            _producerConsumerWriteTask = new ProducerConsumerQueue();
            _gzipActionPerformer = new GzipActionPerformer(
                _producerConsumerCompressTask,
                _producerConsumerWriteTask,
                _fileStreamByBatchWriter
            );

            _fileStreamBatchEnumerator = new FileStreamBatchEnumerator(pathToFile, blockSize);
        }

        public Result Compress()
        {
            var performResult = Result.CreateSuccess();
            try
            {
                foreach (var chunk in _fileStreamBatchEnumerator.Enumerate().SplitToChunks(_chunkSize))
                {
                    performResult = _gzipActionPerformer.Perform(_compressor.Compress, chunk);
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
                _producerConsumerCompressTask.ShutDown();
                _producerConsumerWriteTask.ShutDown();
                _fileStreamByBatchWriter.Dispose();
            }

            return performResult;
        }
    }
}