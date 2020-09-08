using System;
using System.Collections.Generic;
using Core;
using Core.FileStreamTools;
using Core.PriorityLock;
using Core.ThreadPool;

namespace ConcurrentCompressor
{
    public class GzipActionPerformer : IGzipActionPerformer
    {
        private readonly IProducerConsumerQueue _producerConsumerGzipTask;
        private readonly IProducerConsumerQueue _producerConsumerWriteTask;
        private readonly IFileStreamByBatchWriter _fileStreamByBatchWriter;
        private readonly ICustomLock _priorityLock;

        public GzipActionPerformer(
            IProducerConsumerQueue producerConsumerGzipTask,
            IProducerConsumerQueue producerConsumerWriteTask,
            IFileStreamByBatchWriter fileStreamByBatchWriter
        )
        {
            _producerConsumerGzipTask = producerConsumerGzipTask;
            _producerConsumerWriteTask = producerConsumerWriteTask;
            _priorityLock = new PriorityLock();

            _fileStreamByBatchWriter = fileStreamByBatchWriter;
        }

        public Result Perform(
            Func<byte[], byte[]> gzipAction,
            IEnumerable<(byte[] DataBatch, int BatchIndex)> gzipData
        )
        {
            foreach (var (dataBatch, batchIndex) in gzipData)
            {
                _producerConsumerGzipTask.TryExecute(() => PerformBatchAction(dataBatch, batchIndex, gzipAction));
                if (IsErrorOccured())
                {
                    break;
                }
            }

            _producerConsumerGzipTask.AwaitResult();
            if (IsErrorOccured())
            {
                var gzipErrors =_producerConsumerGzipTask.Errors.FormExceptionMessage();
                var writeErrors = _producerConsumerWriteTask.Errors.FormExceptionMessage();
                return Result.CreateFail($"{gzipErrors}\n{writeErrors}");
            }

            return Result.CreateSuccess();
        }

        private void PerformBatchAction(
            byte[] dataBatch,
            int batchIndex,
            Func<byte[], byte[]> action
        )
        {
            var actionResult = action(dataBatch);
            _priorityLock.Wait(batchIndex);

            _producerConsumerWriteTask.TryExecute(() => _fileStreamByBatchWriter.WriteBatch(actionResult));
            Console.WriteLine($"blockNumber:{batchIndex};SourceSize:{dataBatch.Length};ResultSize:{actionResult.Length}");
            _priorityLock.PulseAll();
        }

        private bool IsErrorOccured()
        {
            return _producerConsumerGzipTask.Errors.Length != 0 || _producerConsumerWriteTask.Errors.Length != 0;
        }
    }
}