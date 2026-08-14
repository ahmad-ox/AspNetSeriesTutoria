using Application;
using Application.Services;
using Host.ContextData;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;

//using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocumentExporterController(ApplicationDbContext context, IDocumentManagementService document) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDocumentManagementService _document = document;

    [HttpPost("pdfExport")]
    public async Task<IActionResult> PdfExporter()
    {
        var response = await GenerateLoanOfferPdfAsync();
        return File(response.FileContent, response.ContentType, response.FileDownloadName);
    }

    [HttpPost("HtmlPdfExport")]
    public async Task<IActionResult> HtmlPdfExporter()
    {
        var response = _document.ConvertHtmlToPdfAndReturnBase64();
        return File(response.FileContent, response.ContentType, response.FileDownloadName);
    }

    private static async Task<ByteFileResponseModel> GenerateLoanOfferPdfAsync()
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            PdfWriter pdfWriter = new PdfWriter(memoryStream);
            PdfDocument pdfDoc = new PdfDocument(pdfWriter);
            // Set the document size to A4
            pdfDoc.SetDefaultPageSize(PageSize.A4);
            Document document = new Document(pdfDoc);

            // Set the font
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Add a logo placeholder
            document.Add(new Paragraph("Sterling Bank Logo")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFont(boldFont)
                .SetFontSize(14));


            // Add a horizontal line for separation
            document.Add(new LineSeparator(new SolidLine())
                .SetMarginBottom(10));

            // Create a table with two columns
            Table table = new Table(2);
            table.SetWidth(UnitValue.CreatePercentValue(100)); // Full width

            // Define alternating background colors
            Color evenRowColor = new DeviceRgb(240, 240, 240); // Light ash color for even rows
            Color oddRowColor = ColorConstants.WHITE; // No background for odd rows

            // Add rows to the table
            AddTableRow(table, "EFFECTIVE DATE:", "Tuesday, 24 September 2024", boldFont, regularFont, oddRowColor);
            AddTableRow(table, "NAME:", "AYODEJI JOSHUA FAMUWAGUN", boldFont, regularFont, evenRowColor);
            AddTableRow(table, "ADDRESS:", "Chicken Street Lagos", boldFont, regularFont, oddRowColor);
            AddTableRow(table, "ACCOUNT NUMBER:", "0097520637", boldFont, regularFont, evenRowColor);
            AddTableRow(table, "BVN NUMBER:", "22165556651", boldFont, regularFont, oddRowColor);
            AddTableRow(table, "AMOUNT:", "₦ 10,000.00", boldFont, regularFont, evenRowColor);
            AddTableRow(table, "TENOR:", "1 Month", boldFont, regularFont, oddRowColor);
            AddTableRow(table, "REPAYMENT:", "₦ 10,333.33", boldFont, regularFont, evenRowColor);
            AddTableRow(table, "REPAYMENT DATE:", "28th day of every month.", boldFont, regularFont, oddRowColor);
            AddTableRow(table, "ANNUAL PERCENTAGE RATE:", "40%", boldFont, regularFont, evenRowColor);

            // Add the table to the document
            document.Add(table);

            // Add line separator
            LineSeparator line = new LineSeparator(new SolidLine(1f));
            document.Add(line);

            // Add the letter body
            document.Add(new Paragraph("Dear Sir/Ma,")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginTop(10));

            document.Add(new Paragraph("OFFER OF ₦ 10,000.00 (Ten Thousand Naira and Kobo ONLY) PERSONAL LOAN FACILITY")
                .SetFont(boldFont)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFontSize(12));

            document.Add(new Paragraph("Further to your application, we are pleased to inform you that Sterling Bank Limited (“Sterling” or the “Bank”) "
                + "has approved a Personal Loan Facility in your favor under the following terms and conditions:")
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(10));






            Table table2 = new Table(2);
            table.SetWidth(UnitValue.CreatePercentValue(100)); // Full width

            // Define alternating background colors
            //Color evenRowColor2 = new DeviceRgb(240, 240, 240); // Light ash color for even rows
            //Color oddRowColor2 = ColorConstants.WHITE; // No background for odd rows

            // Add rows to the table
            AddTableRow(table2, "LOAN AMOUNT:", "₦ 10,000.00", boldFont, regularFont, oddRowColor);
            AddTableRow(table2, "MONTHLY REPAYMENT:", "₦ 10,333.33", boldFont, regularFont, evenRowColor);
            AddTableRow(table2, "INTEREST RATE:", " 40%", boldFont, regularFont, oddRowColor);
            AddTableRow(table2, "FEES:", "", boldFont, boldFont, evenRowColor);
            AddTableRow(table2, "MANAGEMENT FEE:", "₦ 100.00", boldFont, regularFont, oddRowColor);
            AddTableRow(table2, "AMOUINSURANCE FEE PER ANNUMNT:", "₦ 350.00", boldFont, regularFont, evenRowColor);
            AddTableRow(table2, "STAMP DUTY:", "₦ 0.00 of the facility amount", boldFont, regularFont, oddRowColor);
            // Add the table to the document
            document.Add(table2);

            // Add line separator
            document.Add(line);
            document.Add(line);

            // Add the terms section
            document.Add(new Paragraph("TERMS")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginTop(10)
                .SetMarginBottom(10));


            //// Add borrower obligations
            //document.Add(new Paragraph("TERMS")
            //    .SetFont(boldFont)
            //    .SetFontSize(12)
            //    .SetMarginTop(10));
            // Add terms content with bold borrower and bank names

            // Terms content with bold parts
            string termsText = "I, AYODEJI JOSHUA FAMUWAGUN (“Borrower”) hereby authorize Sterling Bank Ltd to debit my account for all fees associated with the loan, immediately the facility is disbursed. " +
                "I irrevocably undertake and covenant that I shall at all times make funds available in my account for the purpose of meeting my obligations as and when due. The foregoing shall be construed as a continuing instruction and shall not be revoked by me until I have fully paid down the loan availed to me.";
            string termsText2 = "I consent that Sterling may collect, use and disclose my/our transaction/information from/to the appointed Credit Bureaus and other agencies who may use the information for any approved business purposes as may from time to time be prescribed by the Central Bank of Nigeria (CBN) and/or any relevant statute or regulation.";


            document.Add(new Paragraph()
                .Add(new Text("I, ").SetFont(regularFont))
                .Add(new Text("AYODEJI JOSHUA FAMUWAGUN ").SetFont(boldFont)) // Borrower's name in bold
                .Add(new Text("hereby authorize "))
                .Add(new Text("Sterling Bank Ltd ").SetFont(boldFont)) // Bank name in bold
                .Add(new Text(termsText.Substring(termsText.IndexOf("to debit"))))
                .SetTextAlignment(TextAlignment.JUSTIFIED) // Rest of the terms
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginBottom(10)
                .SetTextAlignment(TextAlignment.JUSTIFIED));

            document.Add(new Paragraph(termsText2)
                .SetFont(regularFont)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFontSize(12).SetMarginBottom(10));

            // Add repayment source
            document.Add(new Paragraph("REPAYMENT SOURCE for the loan is direct debit of borrower's account via tokenization service.")
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginBottom(10));

            // Add conditions
            document.Add(new Paragraph("CONDITIONS")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginTop(10)
                .SetMarginBottom(10));

            List conditionsList = new List(ListNumberingType.ZAPF_DINGBATS_1);
            conditionsList.Add(new ListItem("All expenses however incurred in the arrangement, documentation, enforcement or recovery of any part of this Loan facility (including all legal fees and/or expenses, and any other expenses paid or incurred by the Bank in collecting or enforcing repayment of the loan), all professional and legal fees, valuation fees, monitoring fees, taxes and commissions (if any), would be borne by the Borrower, and the Bank shall be entitled to debit the Borrower’s account for such expenses when due."))
                .SetFont(regularFont)
                .SetTextAlignment(TextAlignment.JUSTIFIED);

            // .SetFont(regularFont).SetFontSize(12));

            conditionsList.Add(new ListItem("Utilization of the facility or any part thereof shall be at the sole discretion of Sterling and is subject to satisfactory documentation and regulation of CBN as may be laid down from time to time."));

            // Add the list of conditions to the document
            document.Add(conditionsList);

            //document.Add(new Paragraph("I, AYODEJI JOSHUA FAMUWAGUN (“Borrower”) hereby authorize Sterling Bank Ltd to debit my account for all fees "
            //    + "associated with the loan, immediately after the facility is disbursed. I irrevocably undertake to ensure my account has sufficient funds "
            //    + "to meet my obligations as they fall due.")
            //    .SetFont(regularFont)
            //    .SetFontSize(12)
            //    .SetMarginBottom(10));

            document.Add(new Paragraph("EVENTS OF DEFAULT")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginTop(10));

            document.Add(new Paragraph("The occurrence of any default will trigger immediate repayment of all outstanding amounts under this facility.")
                .SetFont(regularFont)
                .SetFontSize(12));

            // Add remaining sections similarly (Conditions, Right of Set-Off, Facility Review, etc.)
            // Closing the document
            pdfDoc.Close();

            // Convert the generated PDF to byte array
            byte[] pdfBytes = memoryStream.ToArray();
            var responseData = new ByteFileResponseModel
            {
                FileContent = pdfBytes,
                ContentType = FileContentType.PDF,
                FileDownloadName = "fileDownloadName.pdf",
            };

            return responseData;
            //return await Task.FromResult(pdfBytes);
        }
    }

    private static void AddTableRow(Table table, string label, string value, PdfFont labelFont, PdfFont valueFont, Color backgroundColor)
    {
        // Create label cell
        Cell labelCell = new Cell()
            .Add(new Paragraph(label).SetFont(labelFont))
            .SetBackgroundColor(backgroundColor)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetPadding(5);

        // Create value cell
        Cell valueCell = new Cell()
            .Add(new Paragraph(value).SetFont(valueFont))
            .SetBackgroundColor(backgroundColor)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetPadding(5);

        // Create value cell
        //Cell valueCell1 = new Cell()
        //    .Add(new Paragraph(value).SetFont(valueFont))
        //    .SetBackgroundColor(backgroundColor)
        //    .SetTextAlignment(TextAlignment.LEFT)
        //    .SetPadding(5);

        // Add cells to the table
        table.AddCell(labelCell);
        table.AddCell(valueCell);
        //table.AddCell(valueCell1);
    }

}