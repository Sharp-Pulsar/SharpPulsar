using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SharpPulsar.TestContainer.TestUtils
{
    public static class ArchiveHelper
    {
        public static void ExtractSingleFileFromTarToDisk(Stream tarStream, string filePath)
        {
            ReadSingleFileFromTar(tarStream,
                reader => reader.WriteEntryToFile(filePath, new ExtractionOptions { Overwrite = true }));
        }

        public static async Task<string> ExtractSingleFileFromTarToStringAsync(Stream tarStream)
        {
            using var memStr = new MemoryStream();
            ReadSingleFileFromTar(tarStream,
                reader => reader.WriteEntryTo(memStr));

            memStr.Position = 0;
            using var reader = new StreamReader(memStr);
            return await reader.ReadToEndAsync();
        }

        public static void ReadSingleFileFromTar(Stream tarStream, Action<IReader> readerAction)
        {
            var first = true;
            using var reader = ReaderFactory.Open(tarStream, new ReaderOptions { LeaveStreamOpen = true });
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    if (!first)
                    {
                        throw new InvalidOperationException("More than one file in tar");
                    }
                    first = false;
                    readerAction(reader);
                }
            }
        }

        public static MemoryStream CreateSingleFileTarStream(string sourceFile, string fileNameInTar)
        {
            var tarStream = new MemoryStream();
            using var writer = WriterFactory.Open(tarStream, ArchiveType.Tar, new WriterOptions(CompressionType.None)
            {
                LeaveStreamOpen = true
            });

            writer.Write(fileNameInTar, sourceFile);
            return tarStream;
        }

        public static MemoryStream CreateDirectoryTarStream(string directoryPath)
        {
            var tarStream = new MemoryStream();
            using var writer = WriterFactory.Open(tarStream, ArchiveType.Tar, new WriterOptions(CompressionType.None)
            {
                LeaveStreamOpen = true
            });

            var basePath = Directory.GetParent(directoryPath).FullName; //use parent so the dir name will be included in the tar
            foreach (var filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                var filePathInTar = Path.GetRelativePath(basePath, filePath);
                writer.Write(filePathInTar, filePath);
            }

            return tarStream;
        }
    }
}
