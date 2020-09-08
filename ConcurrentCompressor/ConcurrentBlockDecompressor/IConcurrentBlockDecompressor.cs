using Core;

namespace ConcurrentCompressor.ConcurrentBlockDecompressor
{
    public interface IConcurrentBlockDecompressor
    {
        Result Decompress();
    }
}