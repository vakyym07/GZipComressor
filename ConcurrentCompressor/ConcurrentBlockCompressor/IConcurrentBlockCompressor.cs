using Core;

namespace ConcurrentCompressor.ConcurrentBlockCompressor
{
    public interface IConcurrentBlockCompressor
    {
        Result Compress();
    }
}