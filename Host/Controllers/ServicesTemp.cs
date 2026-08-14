using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers;

public class ServicesTemp : IServicesTemp
{
    private readonly BlobServiceClient _blobServiceClient;

    public ServicesTemp(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }


    //passed
    public async Task<Azure.Response<BlobContentInfo>> UploadFileAsync(byte[] fileBytes, string blobName)
    {
        var extension = Path.GetExtension(blobName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(blobName);
        blobName = $"spectadmin/{fileNameWithoutExtension}{extension}";
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("specta");
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        using var stream = new MemoryStream(fileBytes);
        Azure.Response<BlobContentInfo> blobResponse = await blobClient.UploadAsync(stream, overwrite: true);
        return blobResponse;
    }

    //pass 
    // Get Blob Information (BlobProperties)
    public async Task<BlobProperties?> GetBlobInfoAsync(string blobName)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("specta");
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var blobExist = await blobClient.ExistsAsync();
        if (!blobExist)
            return default;

        BlobProperties properties = await blobClient.GetPropertiesAsync();
        return properties;
    }

    //pass
    public async Task<bool> BlobExistsAsync(string blobName)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("specta");
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var blobExist = await blobClient.ExistsAsync();
        return blobExist;
    }

    //pass
    public async Task<FileContentResult?> DownloadFileAsync(string blobName)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("specta");
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var blobExist = await blobClient.ExistsAsync();
        if (!blobExist)
            return null;

        BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();
        using var memoryStream = new MemoryStream();
        await blobDownloadInfo.Content.CopyToAsync(memoryStream);
        var extension = Path.GetExtension(blobName);
        var contentType = GetContentType(extension);
        return new FileContentResult(memoryStream.ToArray(), contentType)
        {
            FileDownloadName = blobName,
            EnableRangeProcessing = true,
        };
    }

    //passed
    private string GetContentType(string extension) => extension.ToLower() switch
    {
        ".pdf" => "application/pdf",
        ".csv" or ".txt" => "text/csv",
        ".doc" or ".docx" => "application/msword",
        ".xls" or ".xlsx" => "application/vnd.ms-excel",
        ".ppt" or ".pptx" => "application/vnd.ms-powerpoint",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        _ => "application/octet-stream",
    };






    // Download a file from Blob Storage
    public async Task DownloadFileAsync(string blobName, string downloadFilePath)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("specta");

        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        // Download the file
        BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();

        using FileStream fs = File.OpenWrite(downloadFilePath);
        await blobDownloadInfo.Content.CopyToAsync(fs);
        fs.Close();

        //Console.WriteLine($"File downloaded from blob '{blobName}' to '{downloadFilePath}'.");
    }

    public async Task UploadFileAsync(string blobName, string filePath, string isBit)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("specta");
        // Ensure the container exists (it creates the container if it doesn't exist)
        //await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(filePath, overwrite: true);
        Console.WriteLine($"File '{filePath}' uploaded to blob '{blobName}'.");
    }
    public static FileContentResult GenerateFilecont(byte[] FileInByte)
    {
        var response = new FileContentResult(FileInByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = "justtestfile.xlsx",
            EnableRangeProcessing = true,
        };
        return response;
    }
}