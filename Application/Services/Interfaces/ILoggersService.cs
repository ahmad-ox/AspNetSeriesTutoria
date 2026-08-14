using Application.Models.DTOs;

namespace Application.Services.Interfaces;

public interface ILoggersService
{
    Task ArchiveLogsAsync(DateRange range);
    Task<Dictionary<string, int>> CountDuplicateErrorsPerLogAsync(string folderPath);
    Task<int> CountLogsInPeriodAsync(DateRange range);
    Task<Dictionary<string, int>> CountUniqueErrorsPerLogAsync(string folderPath);
    Task DeleteArchivesAsync(DateRange range);
    Task DeleteLogsByPeriodAsync(DateRange range);
    Task<List<string>> SearchLogsByDirectoryAsync(string directory);
    Task<List<string>> SearchLogsBySizeAsync(SizeRange range);
    Task UploadLogsAsync(string filePath, string serverUrl);
}