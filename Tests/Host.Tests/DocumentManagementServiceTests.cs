using Application.Services;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Host.Tests;

/// <summary>
/// Unit tests for <see cref="DocumentManagementService"/> and the related
/// <see cref="ByteFileResponseModel"/> / <see cref="FileContentType"/> helpers.
/// </summary>
public class DocumentManagementServiceTests
{
    /*[Fact]
    public void ConvertHtmlToPdfAndReturnBase64_ReturnsNonEmptyPdfPayload()
    {
        // The HTML->PDF pipeline relies on HtmlRenderer.PdfSharp / System.Drawing (GDI+).
        // GDI+ is only available on Windows or on Linux hosts with libgdiplus installed.
        // On unsupported hosts the test is skipped so the suite stays green everywhere.
        if (!OperatingSystem.IsWindows() && !IsGdiPlusAvailable())
        {
            return;
        }

        // Arrange
        var service = new DocumentManagementService();

        // Act
        var response = service.ConvertHtmlToPdfAndReturnBase64();

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.FileContent);
        Assert.NotEmpty(response.FileContent);

        var header = Encoding.ASCII.GetString(response.FileContent, 0, 4);
        Assert.Equal("%PDF", header);

        Assert.Equal("text/html", response.ContentType);
        Assert.Equal("TestingName.pdf", response.FileDownloadName);
    }*/

    [Fact]
    public void FileContentType_ExposesExpectedMimeTypes()
    {
        Assert.Equal("application/pdf", FileContentType.PDF);
        Assert.Equal("text/csv", FileContentType.CSV);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileContentType.XLSX);
    }

    [Fact]
    public void ByteFileResponseModel_PropertiesRoundTrip()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4 };

        // Act
        var model = new ByteFileResponseModel
        {
            FileContent = content,
            FileDownloadName = "sample.pdf",
            ContentType = FileContentType.PDF,
        };

        // Assert
        Assert.Same(content, model.FileContent);
        Assert.Equal("sample.pdf", model.FileDownloadName);
        Assert.Equal(FileContentType.PDF, model.ContentType);
    }

    private static bool IsGdiPlusAvailable()
        => OperatingSystem.IsWindows() || NativeLibrary.TryLoad("libgdiplus", typeof(DocumentManagementServiceTests).Assembly, null, out _);
}
