using Host.Controllers.EncryptDecrypt;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Xml;
using System.Xml.Serialization;

namespace Host.Controllers.TestingSoap;
[Route("api/[controller]")]
[ApiController]
public class SoapTestingController : ControllerBase
{
    private static readonly string BaseUrl = "http://10.0.0.230/ewService/service.asmx";

    private static readonly string SoapAction = "http://tempuri.org/GetBranches";
    /* [HttpPost("soapSeracice")]
     public async Task<IActionResult> TestSoap([FromBody] SoapTes request)
     {
         var baseUrl = "http://10.0.0.230/ewService/service.asmx";
         try
         {
             var soapEnvelope = @"<?xml version=""1.0"" encoding=""utf-8""?>
             <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
               <soap:Body>
                 <GetBranches xmlns=""http://tempuri.org/"" />
               </soap:Body>
             </soap:Envelope>";

             using var client = new HttpClient();
             client.DefaultRequestHeaders.Add("SOAPAction", "http://tempuri.org/GetBranches");
             var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
             var response = await client.PostAsync(baseUrl, content);
             var responseContent = await response.Content.ReadAsStringAsync();


             // Deserialize the XML response
             var serializer = new XmlSerializer(typeof(NewDataSet), new XmlRootAttribute("diffgram"));
             var result = new NewDataSet();
             using (var stringReader = new StringReader(responseContent))
             {
                 result = serializer.Deserialize(stringReader) as NewDataSet;
             }

             return Ok(result);
         }
         catch (Exception ex)
         {
             return BadRequest(ex);
         }
     }
 */

    [HttpPost("AsoapSeracice")]
    public async Task<IActionResult> TestSoap([FromBody] SoapTes srequest)
    {
        //var baseUrl = "http://10.0.0.230/ewService/service.asmx";
        try
        {
            var client = new RestClient(BaseUrl);

            // Create the SOAP request
            var request = new RestRequest(BaseUrl, Method.Post);
            request.AddHeader("Content-Type", "text/xml; charset=utf-8");
            request.AddHeader("SOAPAction", SoapAction);

            var soapEnvelope = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
              <soap:Body>
                <GetBranches xmlns=""http://tempuri.org/"" />
              </soap:Body>
            </soap:Envelope>";

            request.AddParameter("text/xml; charset=utf-8", soapEnvelope, ParameterType.RequestBody);

            // Execute the request
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                // Parse the response
                return Ok(ParseSoapResponse(response.Content));
            }
            else
            {
                return BadRequest($"Error: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    private static List<Branch> ParseSoapResponse(string responseContent)
    {
        var branches = new List<Branch>();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(responseContent);

        var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
        namespaceManager.AddNamespace("ns", "http://tempuri.org/");

        var diffgramNode = xmlDocument.SelectSingleNode("//*[local-name()='diffgram']");
        if (diffgramNode != null)
        {
            var branchNodes = diffgramNode.SelectNodes("//ns:recs", namespaceManager);
            foreach (XmlNode branchNode in branchNodes)
            {
                var branch = new Branch
                {
                    BranchCode = branchNode.SelectSingleNode("ns:BRANCHCODE", namespaceManager)?.InnerText,
                    BranchName = branchNode.SelectSingleNode("ns:BRANCHNAME", namespaceManager)?.InnerText,
                    BranchAlias = branchNode.SelectSingleNode("ns:BRANCHALIAS", namespaceManager)?.InnerText,
                    SubdivCode = branchNode.SelectSingleNode("ns:SUBDIVCODE", namespaceManager)?.InnerText,
                    StateId = decimal.TryParse(branchNode.SelectSingleNode("ns:STATEID", namespaceManager)?.InnerText, out var stateId) ? stateId : null,
                    StateName = branchNode.SelectSingleNode("ns:STATENAME", namespaceManager)?.InnerText,
                    RegionId = decimal.TryParse(branchNode.SelectSingleNode("ns:REGIONID", namespaceManager)?.InnerText, out var regionId) ? regionId : null,
                    RegionName = branchNode.SelectSingleNode("ns:REGIONNAME", namespaceManager)?.InnerText
                };

                branches.Add(branch);
            }
        }

        return branches;
    }
}

public class Branch
{
    public string BranchCode { get; set; }
    public string BranchName { get; set; }
    public string BranchAlias { get; set; }
    public string SubdivCode { get; set; }
    public decimal? StateId { get; set; }
    public string StateName { get; set; }
    public decimal? RegionId { get; set; }
    public string RegionName { get; set; }
}

/*public class Branch
{
    public string BRANCHCODE { get; set; }
    public string BRANCHNAME { get; set; }
    public string BRANCHALIAS { get; set; }
    public string SUBDIVCODE { get; set; }
    public decimal? STATEID { get; set; }
    public string STATENAME { get; set; }
    public decimal? REGIONID { get; set; }
    public string REGIONNAME { get; set; }
}
*/

[XmlRoot(ElementName = "NewDataSet")]
public class NewDataSet
{
    [XmlElement(ElementName = "recs")]
    public List<Branch> Branches { get; set; }
}
