namespace Host.Models;

public class Base64FileModel : BaseEntity
{
    public string FileName { get; set; }
    public string Base64Content { get; set; }
}