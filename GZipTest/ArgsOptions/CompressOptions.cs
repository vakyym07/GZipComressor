using CommandLine;

namespace GZipTest.ArgsOptions
{
    [Verb("compress", HelpText = "Compress file contents to the new one")]
    public class CompressOptions
    {
        [Value(0, Required = true, HelpText = "File which you want to compress", MetaName = "SourceFile")]
        public string SourceFile { get; set; }

        [Value(1, Required = true, HelpText = "File to save result of compression ", MetaName = "FileToSave")]
        public string CompressToFile { get; set; }
    }
}