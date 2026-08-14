using Application;
using Application.Services;
using Host.ContextData;
using Host.Models;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Host.Controllers;
[Route("api/[controller]")]
[ApiController]
public class FileManagementController(ApplicationDbContext context, IServicesTemp servicesTemp) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly IServicesTemp _servicesTemp = servicesTemp;
    // Endpoint to take IFormFile and return base64
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();
        var base64String = Convert.ToBase64String(fileBytes);
        var fileModel = new Base64FileModel
        {
            Base64Content = base64String,
            FileName = $"{Guid.NewGuid}{file.FileName}",
        };
        await _context.AddAsync(fileModel);
        await _context.SaveChangesAsync();
        return Ok(base64String);
    }

    // Endpoint to take base64 and return the file
    [HttpPost("download")]
    public async Task<IActionResult> DownloadFile([FromBody] Base64FileModel request)
    {
        if (request == null || string.IsNullOrEmpty(request.Base64Content))
        {
            return BadRequest("Invalid base64 string");
        }

        try
        {
            var extension = Path.GetExtension(request.FileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(request.FileName);

            string fileName = $"{fileNameWithoutExtension}{extension}";
            //string fileName = $"{fileNameWithoutExtension}{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}{extension}";
            var fileBytes = Convert.FromBase64String(request.Base64Content);
            var response = await _servicesTemp.UploadFileAsync(fileBytes, fileName);
            //var response =await _servicesTemp.GetBlobInfoAsync(request.FileName);
            //var response = await _servicesTemp.DownloadFileAsync(request.FileName);
            //var resul = ServicesTemp.GenerateFilecont(fileBytes);
            //var fileModel = new Base64FileModel
            //{
            //    Base64Content = "Null",
            //    FileName = fileName,
            //};
            //await _context.AddAsync(fileModel);
            //await _context.SaveChangesAsync();
            if (response is null)
                return BadRequest("File not found");

            return Ok(response);
        }
        catch (FormatException)
        {
            return BadRequest("Invalid base64 format");
        }
    }

    /*
     
    // Endpoint to take base64 and return the file
    [HttpPost("download")]
    public IActionResult DownloadFile([FromBody] Base64FileModel request)
    {
        if (request == null || string.IsNullOrEmpty(request.Base64Content))
        {
            return BadRequest("Invalid base64 string");
        }

        try
        {
            var fileBytes = Convert.FromBase64String(request.Base64Content);
            return File(fileBytes, "application/octet-stream", request.FileName);
        }
        catch (FormatException)
        {
            return BadRequest("Invalid base64 format");
        }
    }

     */

    private async Task<ByteFileResponseModel> GenerateReportPdf()
    {
        var reportData = new List<ByteFileResponseModel>();
        var data = new PdfDocument();
        string htmlContent = "<div style='margin: 10px auto; max-width: 600px; padding: 10px; border: 1px solid #ccc; background-color: #FFFFFF; font-family: Arial, sans-serif;'>";
        htmlContent += "<header style=\"text-align: center; line-height: -3;\">\r\n    <h2>IN THE NAME OF ALLAH, THE GRACIOUS, THE MERCIFUL</h2>\r\n    <h3>MAJLIS ANSARULLAH SILSILA ALIYA AHMADIYYA, NIGERIA</h3>\r\n    <h4>MONTHLY ACTIVITIES REPORT</h4>\r\n</header>";
        foreach (var section in reportData)
        {
            htmlContent += $"<h3>{section.FileContent}</h3>";

            htmlContent += "<table style='width: 100%; border-collapse: collapse;'>";
            htmlContent += "<thead>";
            htmlContent += "<tr>";
            htmlContent += "<th style='padding: 8px; text-align: left; border-bottom: 1px solid #ddd;'>S/N</th>";
            htmlContent += "<th style='padding: 8px; text-align: left; border-bottom: 1px solid #ddd;'>Question</th>";
            htmlContent += "<th style='padding: 8px; text-align: left; border-bottom: 1px solid #ddd;'>Question Response</th>";
            htmlContent += "<th style='padding: 8px; text-align: left; border-bottom: 1px solid #ddd;'>Score</th>";
            htmlContent += "</tr>";
            htmlContent += "</thead>";
            htmlContent += "<tbody>";

            int serialNumber = 1;

            foreach (var respons in reportData)
            {
                htmlContent += "<tr>";
                htmlContent += $"<td style='padding: 8px; text-align: left; border-bottom: 1px solid #ddd;'>{serialNumber}</td>";
                htmlContent += $"<td style='padding: 8px; text-align: left; border-bottom: 1px solid #ddd;'>{respons.FileContent}</td>";
                htmlContent += $"<td style='padding: 8px; text-align: left; border-bottom: 1px solid #ddd;'>{respons.FileContent}</td>";
                htmlContent += $"<td style='padding: 8px; text-align: left; border-bottom: 1px solid #ddd;'>{respons.FileDownloadName}</td>";
                htmlContent += "</tr>";

                serialNumber++;
            }

            htmlContent += "</tbody>";
            htmlContent += "</table>";
        }
        htmlContent += $"<footer> <h4>This report response was generated on {DateTime.UtcNow.ToString("dddd, MMMM dd, yyyy hh:mm:ss tt")}</h4></ footer>";

        PdfGenerator.GeneratePdf("data", PdfSharp.PageSize.A4);
        byte[]? fileContent = null;
        using (MemoryStream ms = new MemoryStream())
        {
            data.Save(ms);
            fileContent = ms.ToArray();
        }

        string fileDownloadName = "ReportData" + DateTime.UtcNow.ToString() + ".pdf";
        var responseModel = new ByteFileResponseModel
        {
            ContentType = "application/pdf",
            FileDownloadName = fileDownloadName,
            FileContent = fileContent,
        };

        return new ByteFileResponseModel();
    }


}
