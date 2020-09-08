using System;

namespace Core.FileStreamTools
{
    public interface IFileStreamByBatchWriter : IDisposable
    {
        void WriteBatch(byte[] dataBatch);
    }
}