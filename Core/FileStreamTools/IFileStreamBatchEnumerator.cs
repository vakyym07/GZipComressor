using System.Collections.Generic;

namespace Core.FileStreamTools
{
    public interface IFileStreamBatchEnumerator
    {
        IEnumerable<(byte[] DataBatch, int BatchIndex)> Enumerate();
        IEnumerable<(byte[] DataBatch, int BatchIndex)> Split(byte[] separator);
    }
}