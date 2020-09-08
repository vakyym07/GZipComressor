using System;
using System.Collections.Generic;
using Core;

namespace ConcurrentCompressor
{
    public interface IGzipActionPerformer
    {
        Result Perform(Func<byte[], byte[]> gzipAction, IEnumerable<(byte[] DataBatch, int BatchIndex)> gzipData);
    }
}