using System;
using BlockCompressor;
using CommandLine;
using ConcurrentCompressor.ConcurrentBlockCompressor;
using ConcurrentCompressor.ConcurrentBlockDecompressor;
using Core;
using GZipTest.ArgsOptions;

namespace GZipTest
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CompressOptions, DecompressOptions>(args)
                .MapResult(
                    (CompressOptions opts) => RunCompressAndReturnExitCode(opts),
                    (DecompressOptions opts) => RunDecompressAndReturnExitCode(opts),
                    errs => 1);
        }

        private static int RunCompressAndReturnExitCode(CompressOptions options)
        {
            var compressor = new ConcurrentBlockCompressor(
                new Compressor(),
                options.SourceFile,
                options.CompressToFile,
                1048576
            );
            return RunCommand(compressor.Compress, options.CompressToFile);
        }

        private static int RunDecompressAndReturnExitCode(DecompressOptions options)
        {
            var decompressor = new ConcurrentBlockDecompressor(
                new Compressor(),
                options.SourceFile,
                options.DecompressToFile,
                1048576
            );
            return RunCommand(decompressor.Decompress, options.DecompressToFile);
        }

        private static int RunCommand(Func<Result> action, string saveToFile)
        {
            Console.WriteLine("Processing...");
            var result = action();
            if (result.IsSuccess)
            {
                Console.WriteLine($"\nResult save to file {saveToFile}");
                Console.WriteLine(0);
                return 0;
            }

            Console.WriteLine(result.ErrorMessage);
            Console.WriteLine(1);
            return 1;
        }
    }
}