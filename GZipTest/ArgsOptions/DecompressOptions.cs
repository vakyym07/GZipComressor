using CommandLine;

namespace GZipTest.ArgsOptions
{
    [Verb("decompress", HelpText = "Decompress file contents to the new file")]
    public class DecompressOptions
    {
        [Value(0, Required = true, HelpText = "File which you want to decompress", MetaName = "SourceFile")]
        public string SourceFile { get; set; }

        [Value(1, Required = true, HelpText = "File to save result of decompression", MetaName = "SourceFile")]
        public string DecompressToFile { get; set; }
    }
}