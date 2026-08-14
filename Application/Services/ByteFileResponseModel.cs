namespace Application.Services;

public class ByteFileResponseModel
{
    public byte[] FileContent { get; set; }
    public string FileDownloadName { get; set; }
    public string ContentType { get; set; }

}