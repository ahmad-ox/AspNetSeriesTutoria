using PdfSharp;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Application.Services;

public class DocumentManagementService : IDocumentManagementService
{
    public void PDFConverter()
    {
        PdfDocument pdf = PdfGenerator.GeneratePdf("<p><h1>Hello World</h1>This is html rendered text</p>", PageSize.A4);
        pdf.Save("document.pdf");
    }

    public ByteFileResponseModel ConvertHtmlToPdfAndReturnBase64()
    {
        string htmlContent = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        h1 { color: blue; }
    </style>
</head>
<body>
    <h1>Hello World</h1>
    <p>This is html rendered text</p>
</body>
</html>";//HTMLDocs();
        // Generate PDF from HTML
        PdfDocument pdfDocument = PdfGenerator.GeneratePdf(htmlContent, PageSize.A4, 10);

        // Save PDF to a byte array
        using MemoryStream stream = new MemoryStream();
        pdfDocument.Save(stream, false); // Save to memory stream
        byte[] pdfBytes = stream.ToArray();

        // Convert PDF to Base64 string
        var response = Convert.ToBase64String(pdfBytes);
        return new ByteFileResponseModel()
        {
            ContentType = "text/html",
            FileContent = pdfBytes,
            FileDownloadName = "TestingName.pdf"
        };
    }

    public string ConvertHtmlToPdfAndSave()
    {
        string fileName = "Testdocument";
        string htmlContent = HTMLDocs();
        // Ensure the "Files" directory exists
        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate PDF from HTML
        PdfDocument pdfDocument = PdfGenerator.GeneratePdf(htmlContent, PageSize.A4);

        // Define the full file path
        string filePath = Path.Combine(folderPath, $"{fileName}.pdf");

        // Save the PDF to the specified folder
        pdfDocument.Save(filePath);
        return filePath;
    }

    private static string HTMLDocs()
    {
        return @"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Audit Logs</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
            padding: 0;
        }

        /* Header Section */
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 2px solid #ccc;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }

        .header img {
            height: 80px;
            width: 80px;
        }

        .org-info {
            text-align: center;
            flex: 1;
            color: #B22222;
            /* Light red color */
        }

        .org-info h1 {
            margin: 0;
            font-size: 24px;
            font-weight: bold;
        }

        .org-info p {
            margin: 5px 0;
            font-size: 16px;
        }

        /* Table Section */
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        table,
        th,
        td {
            border: 1px solid #ccc;
        }

        th,
        td {
            padding: 10px;
            text-align: left;
        }

        th {
            background-color: #f2f2f2;
            font-weight: bold;
        }

        tr:nth-child(even) {
            background-color: #f9f9f9;
        }
    </style>
</head>

<body>

    <!-- Header Section -->
    <div class=""header"">
        <div class=""logo"">
            <img src=""https://www.clipartmax.com/png/small/166-1665531_sterling-bank-plc-lagos-nigeria.png""
                alt=""Sterling Bank Plc Lagos Nigeria"">
        </div>
        <div class=""org-info"">
            <h1>Sterling Bank Plc.</h1>
            <p>Sterling Towers, 20 Marina, Lagos Island, Lagos.</p>
            <p>017003270</p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th>Email</th>
                <th>Affected Columns</th>
                <th>DateTime</th>
                <th>Action Type</th>
                <th>Remote IP Address</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>user1@example.com</td>
                <td>Name, Address</td>
                <td>2024-09-30 14:30</td>
                <td>Update</td>
                <td>192.168.0.1</td>
            </tr>
            <tr>
                <td>user2@example.com</td>
                <td>Email</td>
                <td>2024-09-30 15:00</td>
                <td>Delete</td>
                <td>192.168.0.2</td>
            </tr>
        </tbody>
    </table>

</body>

</html>
";
    }

}
