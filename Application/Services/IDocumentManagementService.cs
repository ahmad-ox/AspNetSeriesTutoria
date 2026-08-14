namespace Application.Services;

public interface IDocumentManagementService
{
    ByteFileResponseModel ConvertHtmlToPdfAndReturnBase64();
    string ConvertHtmlToPdfAndSave();
    void PDFConverter();
}
