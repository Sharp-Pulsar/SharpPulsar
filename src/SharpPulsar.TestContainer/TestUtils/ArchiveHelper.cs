﻿using SharpCompress.Common;
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

            var basePath = Directory.GetParent(directoryPath)?.FullName; //use parent so the dir name will be included in the tar
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));

            foreach (var filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                var filePathInTar = Path.GetRelativePath(basePath, filePath);
                writer.Write(filePathInTar, filePath);
            }

            return tarStream;
        }

        public static void Extract(Stream inStream, string folderPath, string? pathToRemove = null)
        {
            if (inStream == null)
                throw new ArgumentNullException(nameof(inStream));
            if (folderPath == null)
                throw new ArgumentNullException(nameof(folderPath));

            pathToRemove ??= string.Empty;

            var reader = ReaderFactory.Open(inStream);

            while (reader.MoveToNextEntry())
            {
                if (reader.Entry.IsDirectory)
                    continue;

                var outputDirectory = Path.Combine(
                    folderPath,
                    pathToRemove,
                    reader.Entry.Key);

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                reader.WriteEntryToDirectory(outputDirectory, new ExtractionOptions
                {
                    ExtractFullPath = false,
                    Overwrite = true
                });
            }
        }
    }
}
