using System.IO;

namespace Core.FileStreamTools
{
    public class FileStreamByBatchWriter : IFileStreamByBatchWriter
    {
        private readonly string _pathToFile;
        private readonly FileStream _fileStream;

        public FileStreamByBatchWriter(string pathToFile)
        {
            _pathToFile = pathToFile;
            _fileStream = new FileStream(_pathToFile, FileMode.Append);
        }

        public void WriteBatch(byte[] dataBatch)
        {
            _fileStream.Write(dataBatch);
        }

        public void Dispose()
        {
            _fileStream.Close();
        }
    }
}