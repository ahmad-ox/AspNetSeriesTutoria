using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers;

public interface IServicesTemp
{
    Task<bool> BlobExistsAsync(string blobName);
    Task<FileContentResult?> DownloadFileAsync(string blobName);
    Task DownloadFileAsync(string blobName, string downloadFilePath);
    Task<BlobProperties> GetBlobInfoAsync(string blobName);
    Task<Response<BlobContentInfo>> UploadFileAsync(byte[] fileBytes, string blobName);
    Task UploadFileAsync(string blobName, string filePath, string isBit);
}
