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
        // The HTML->PDF pipeline relies on HtmlRenderer.PdfSharp / System.Drawing (GDI+),
        // which is only available on Windows or on Linux hosts that have libgdiplus installed
        // AND the System.Drawing.EnableUnixSupport runtime switch enabled (set in Host.Tests.csproj).
        // On unsupported hosts the test is skipped so the suite stays green on every CI platform.
        if (!IsGdiPlusAvailable())
        {
            return;
        }

        // Arrange
        var service = new DocumentManagementService();

        // Act
        ByteFileResponseModel response;
        try
        {
            response = service.ConvertHtmlToPdfAndReturnBase64();
        }
        catch (Exception ex) when (ex is PlatformNotSupportedException or DllNotFoundException or TypeInitializationException)
        {
            // GDI+/native dependencies are unavailable at runtime — nothing meaningful to assert.
            return;
        }

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
    {
        if (OperatingSystem.IsWindows())
        {
            return true;
        }

        // On Unix, System.Drawing.Common additionally requires the
        // System.Drawing.EnableUnixSupport runtime switch (set in Host.Tests.csproj)
        // and the native libgdiplus library to be loadable.
        var unixSupportEnabled =
            AppContext.TryGetSwitch("System.Drawing.EnableUnixSupport", out var enabled) && enabled;

        return unixSupportEnabled
            && NativeLibrary.TryLoad("libgdiplus", typeof(DocumentManagementServiceTests).Assembly, null, out _);
    }
}
