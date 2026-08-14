namespace Host.Controllers.EncryptDecrypt;

public class AesRequest
{
    public dynamic PayLoad { get; set; }
}

public record SoapTes
{
    public string? Text { get; set; }
}

public class AesConstantsRequest
{
    public string InitializationVector { get; set; }
    public string SecretKey { get; set; }
    public AesOperation Operation { get; set; } = AesOperation.Encrypt;
    public AesMode Mode { get; set; } = AesMode.CBC;
    public AesKeySize KeySize { get; set; } = AesKeySize.Key128;
    public AesOutputFormat OutputFormat { get; set; } = AesOutputFormat.Hex;
}

public enum AesMode
{
    CBC = 1,
    ECB
}

public enum AesKeySize
{
    Key128 = 128,
    Key192 = 192,
    Key256 = 256
}

public enum AesOperation
{
    Encrypt,
    Decrypt
}

public enum AesOutputFormat
{
    Hex = 16,
    Base64 = 64,
}