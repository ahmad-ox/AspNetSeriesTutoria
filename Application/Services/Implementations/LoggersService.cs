using Application.Models.DTOs;
using Application.Services.Interfaces;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Text.RegularExpressions;
namespace Application.Services.Implementations;

public class LoggersService : ILoggersService
{

    #region Counting Errors
    private static List<string> RetrieveLogFilePaths(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"The directory '{folderPath}' does not exist.");

            return Directory.EnumerateFiles(folderPath, "*.log", SearchOption.AllDirectories).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return [];
        }
    }

    private static readonly Regex ErrorRegex = new Regex(
        @"\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2}(?::\d{4})? .*?: (.*)",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static string? ExtractError(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var match = ErrorRegex.Match(line);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }


    #endregion

    #region US-10: Count Total Available Logs in a Period (Optimized)

    public async Task<int> CountLogsInPeriodAsync(DateRange range)
    {
        if (string.IsNullOrEmpty(range.DirectoryLoc))
            throw new ArgumentNullException(nameof(range.DirectoryLoc), "Directory location cannot be null or empty.");

        int count = 0;
        await foreach (var file in EnumerateLogFiles(range.DirectoryLoc, range.StartDate, range.EndDate))
            count++;

        return count;
    }

    private static async IAsyncEnumerable<string> EnumerateLogFiles(string directoryLoc, DateTime startDate, DateTime endDate)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            MatchCasing = MatchCasing.CaseInsensitive
        };

        foreach (var file in Directory.EnumerateFiles(directoryLoc, "*.log", options))
        {
            if (FileNameMatchesDateRange(file, startDate, endDate))
                yield return file;

            await Task.Yield();
        }
    }

    #endregion
    #region US-11: Delete Logs from a Period (Optimized)

    public async Task DeleteLogsByPeriodAsync(DateRange range)
    {
        if (string.IsNullOrEmpty(range.DirectoryLoc))
            throw new ArgumentNullException(nameof(range.DirectoryLoc), "Directory location cannot be null or empty.");

        await foreach (var file in EnumerateLogFiles(range.DirectoryLoc, range.StartDate, range.EndDate))
        {
            try
            {
                File.Delete(file); // Directly delete the file
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete file {file}: {ex.Message}");
            }
        }
    }

    #endregion

    #region US-12 : Counts number of unique errors per log files
    public async Task<Dictionary<string, int>> CountUniqueErrorsPerLogAsync(string folderPath)
    {
        var fileCount = new ConcurrentDictionary<string, int>();
        var filePaths = RetrieveLogFilePaths(folderPath);

        await Parallel.ForEachAsync(filePaths, async (file, _) =>
        {
            try
            {
                var distinctErrors = new HashSet<string>(); // HashSet is faster for unique values

                using var fileStream = File.OpenRead(file);
                using var memoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
                using var memoryMappedViewStream = memoryMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
                using var reader = new StreamReader(memoryMappedViewStream);

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var error = ExtractError(line);
                    if (error != null)
                        distinctErrors.Add(error);
                }

                fileCount[file] = distinctErrors.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
            }
        });

        return new Dictionary<string, int>(fileCount);
    }
    #endregion

    #region US-13: Counts number of duplicated errors per log files
    public async Task<Dictionary<string, int>> CountDuplicateErrorsPerLogAsync(string folderPath)
    {
        var fileDuplicateCounts = new ConcurrentDictionary<string, int>();
        var filePaths = RetrieveLogFilePaths(folderPath);

        await Parallel.ForEachAsync(filePaths, async (file, _) =>
        {
            try
            {
                var duplicateCounts = new ConcurrentDictionary<string, int>();

                using var fileStream = File.OpenRead(file);
                if (fileStream.Length > 10 * 1024 * 1024) // Adjust threshold for large files
                {
                    using var memoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
                    using var memoryMappedViewStream = memoryMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
                    using var reader = new StreamReader(memoryMappedViewStream);
                    await ProcessLinesInBatchesAsync(reader, duplicateCounts);
                }
                else
                {
                    using var bufferedStream = new BufferedStream(fileStream);
                    using var reader = new StreamReader(bufferedStream);
                    await ProcessLinesInBatchesAsync(reader, duplicateCounts);
                }

                fileDuplicateCounts[file] = duplicateCounts.Values.Where(count => count > 1).Sum(count => count - 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
            }
        });

        return new Dictionary<string, int>(fileDuplicateCounts);
    }

    private static async Task ProcessLinesInBatchesAsync(StreamReader reader, ConcurrentDictionary<string, int> duplicateCounts)
    {
        const int batchSize = 1000;
        var batch = ArrayPool<string>.Shared.Rent(batchSize);

        try
        {
            int batchCount;
            while ((batchCount = await ReadBatchAsync(reader, batch)) > 0)
            {
                for (int i = 0; i < batchCount; i++)
                {
                    var error = ExtractError(batch[i]);
                    if (error != null)
                        duplicateCounts.AddOrUpdate(error, 1, (_, count) => count + 1);
                }
            }
        }
        finally
        {
            ArrayPool<string>.Shared.Return(batch);
        }
    }

    private static async Task<int> ReadBatchAsync(StreamReader reader, string[] batch)
    {
        int count = 0;
        while (count < batch.Length && !reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
                batch[count++] = line;
        }

        return count;
    }
    #endregion

    #region US-14: Search logs per directory
    public async Task<List<string>> SearchLogsByDirectoryAsync(string directory)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");

        var logFiles = new List<string>();
        var files = Directory.EnumerateFiles(directory, "*.log", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            logFiles.Add(file);
            await Task.Yield();
        }

        return logFiles;
    }
    #endregion

    #region US-15: Search logs per size

    public async Task<List<string>> SearchLogsBySizeAsync(SizeRange range)
    {
        if (string.IsNullOrEmpty(range.DirectoryLoc))
            throw new ArgumentNullException(nameof(range.DirectoryLoc), "Directory location cannot be null or empty.");

        var logFiles = new List<string>();
        await foreach (var file in EnumerateLogFilesBySize(range.DirectoryLoc, range.MinSizeKb, range.MaxSizeKb))
            logFiles.Add(file);

        return logFiles;
    }

    private async IAsyncEnumerable<string> EnumerateLogFilesBySize(string directoryLoc, long minSizeKb, long maxSizeKb)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true
        };

        foreach (var file in Directory.EnumerateFiles(directoryLoc, "*", options))
        {
            if (file.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            {
                var sizeInKb = new FileInfo(file).Length / 1024;
                if (sizeInKb >= minSizeKb && sizeInKb <= maxSizeKb)
                    yield return file;
            }

            await Task.Yield();
        }
    }

    #endregion

    #region US-16: archive logs from a period
    public async Task ArchiveLogsAsync(DateRange range)
    {
        if (string.IsNullOrWhiteSpace(range.DirectoryLoc))
            throw new ArgumentNullException(nameof(range.DirectoryLoc), "Directory location cannot be null or empty.");

        var logsToArchive = await EnumerateLogsForArchiveAsync(range.DirectoryLoc, range.StartDate, range.EndDate);
        if (!logsToArchive.Any())
            return;

        var zipFileName = Path.Combine(range.DirectoryLoc, $"{range.StartDate:dd_MM_yyyy}-{range.EndDate:dd_MM_yyyy}.zip");
        using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
        {
            foreach (var log in logsToArchive)
                zip.CreateEntryFromFile(log, Path.GetFileName(log));
        }

        foreach (var log in logsToArchive)
        {
            try
            {
                File.Delete(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file '{log}': {ex.Message}");
            }
        }
    }

    private async Task<IEnumerable<string>> EnumerateLogsForArchiveAsync(string directoryLoc, DateTime startDate, DateTime endDate)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true
        };

        return await Task.Run(() =>
            Directory.EnumerateFiles(directoryLoc, "*.log", options)
                .Where(file => FileNameMatchesDateRange(file, startDate, endDate)));
    }

    private static bool FileNameMatchesDateRange(string file, DateTime startDate, DateTime endDate)
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        if (fileName == null)
            return false;

        var datePart = fileName.Split('_').FirstOrDefault();
        if (DateTime.TryParseExact(datePart, "yyyy.MM.dd", null, DateTimeStyles.None, out var fileDate))
            return fileDate >= startDate && fileDate <= endDate;

        return false;
    }

    #endregion
    #region US-17: Delete archives from a period (Optimized)

    public async Task DeleteArchivesAsync(DateRange range)
    {
        if (string.IsNullOrWhiteSpace(range.DirectoryLoc))
            throw new ArgumentNullException(nameof(range.DirectoryLoc), "Directory location cannot be null or empty.");

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true
        };

        await foreach (var file in EnumerateFilesAsync(range.DirectoryLoc, "*.zip", options))
        {
            if (FileMatchesDateRange(file, range))
            {
                try
                {
                    File.Delete(file); // Direct deletion
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file '{file}': {ex.Message}");
                }
            }
        }
    }

    private static bool FileMatchesDateRange(string file, DateRange range)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

        return TryParseDateRange(fileNameWithoutExtension, out var fileRange) &&
               fileRange.StartDate >= range.StartDate &&
               fileRange.EndDate <= range.EndDate;
    }

    private static bool TryParseDateRange(string fileName, out DateRange? range)
    {
        range = null;
        var parts = fileName.Split('-');
        if (parts.Length == 2 &&
            DateTime.TryParseExact(parts[0], "dd_MM_yyyy", null, DateTimeStyles.None, out var startDate) &&
            DateTime.TryParseExact(parts[1], "dd_MM_yyyy", null, DateTimeStyles.None, out var endDate))
        {
            range = new DateRange { StartDate = startDate, EndDate = endDate };
            return true;
        }

        return false;
    }

    private static async IAsyncEnumerable<string> EnumerateFilesAsync(string directory, string searchPattern, EnumerationOptions options)
    {
        foreach (var file in Directory.EnumerateFiles(directory, searchPattern, options))
        {
            yield return file;
            await Task.Yield();
        }
    }

    #endregion

    #region  US-18: upload logs on a remote server per API
    public async Task UploadLogsAsync(string filePath, string serverUrl)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The file '{filePath}' does not exist.");

        using var client = new HttpClient();
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var content = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "file", Path.GetFileName(filePath) }
        };

        var response = await client.PostAsync(serverUrl, content);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to upload log file. Status code: {response.StatusCode}");
    }
    #endregion

    #region US-18: Upload logs on a remote server per API
    public async Task UploadLogsAsync(IEnumerable<string> filePaths, string serverUrl)
    {
        if (filePaths == null || !filePaths.Any())
            throw new ArgumentException("No files provided for upload.", nameof(filePaths));

        using var client = new HttpClient();

        // Create tasks for uploading each file
        var uploadTasks = filePaths.Select(async filePath =>
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file '{filePath}' does not exist.");

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var content = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "file", Path.GetFileName(filePath) }
        };

            var response = await client.PostAsync(serverUrl, content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to upload file '{filePath}'. Status code: {response.StatusCode}");
        });

        // Wait for all uploads to complete
        await Task.WhenAll(uploadTasks);
    }
    #endregion

}
