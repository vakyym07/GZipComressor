namespace BlockCompressor
{
    public interface ICompressor
    {
        byte[] Compress(byte[] dataToCompress);
        byte[] Decompress(byte[] dataToDecompress);
    }
}