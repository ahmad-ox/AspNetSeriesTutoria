using Application.Services;
using Host.ContextData;
using Host.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Host.Tests;

/// <summary>
/// Unit tests for <see cref="DocumentExporterController"/>.
/// The controller's primary constructor requires an <see cref="ApplicationDbContext"/>,
/// which is not used by the tested actions, so it is created with empty options.
/// </summary>
public class DocumentExporterControllerTests
{
    private const string PdfMagicHeader = "%PDF";

    private static DocumentExporterController CreateController(IDocumentManagementService documentService)
    {
        var context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
        return new DocumentExporterController(context, documentService);
    }

    [Fact]
    public async Task PdfExporter_ReturnsPdfFileContentResult_WithExpectedMetadataAndContent()
    {
        // Arrange
        var controller = CreateController(new Mock<IDocumentManagementService>().Object);

        // Act
        var result = await controller.PdfExporter();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(FileContentType.PDF, fileResult.ContentType);
        Assert.Equal("fileDownloadName.pdf", fileResult.FileDownloadName);
        Assert.NotNull(fileResult.FileContents);
        Assert.NotEmpty(fileResult.FileContents);

        // The generated payload must be a valid PDF (starts with the %PDF magic header).
        var header = System.Text.Encoding.ASCII.GetString(fileResult.FileContents, 0, 4);
        Assert.Equal(PdfMagicHeader, header);
    }

    [Fact]
    public async Task HtmlPdfExporter_ReturnsFileContentResult_WithServiceResponse()
    {
        // Arrange
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 }; // "%PDF-1.4"
        var serviceResponse = new ByteFileResponseModel
        {
            FileContent = pdfBytes,
            ContentType = "text/html",
            FileDownloadName = "TestingName.pdf",
        };

        var documentServiceMock = new Mock<IDocumentManagementService>();
        documentServiceMock
            .Setup(service => service.ConvertHtmlToPdfAndReturnBase64())
            .Returns(serviceResponse);

        var controller = CreateController(documentServiceMock.Object);

        // Act
        var result = await controller.HtmlPdfExporter();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Same(pdfBytes, fileResult.FileContents);
        Assert.Equal("text/html", fileResult.ContentType);
        Assert.Equal("TestingName.pdf", fileResult.FileDownloadName);

        documentServiceMock.Verify(
            service => service.ConvertHtmlToPdfAndReturnBase64(),
            Times.Once);
    }

    [Fact]
    public async Task HtmlPdfExporter_AlwaysDelegatesToDocumentManagementService()
    {
        // Arrange
        var documentServiceMock = new Mock<IDocumentManagementService>();
        documentServiceMock
            .Setup(service => service.ConvertHtmlToPdfAndReturnBase64())
            .Returns(new ByteFileResponseModel
            {
                FileContent = Array.Empty<byte>(),
                ContentType = "application/pdf",
                FileDownloadName = "Empty.pdf",
            });

        var controller = CreateController(documentServiceMock.Object);

        // Act
        await controller.HtmlPdfExporter();

        // Assert
        documentServiceMock.Verify(
            service => service.ConvertHtmlToPdfAndReturnBase64(),
            Times.Once);
    }
}
