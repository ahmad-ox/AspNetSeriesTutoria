using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Host.Controllers.EncryptDecrypt;
[Route("api/[controller]")]
[ApiController]
public class EncryptDecryptController : ControllerBase
{
    [HttpPost("encrypt")]
    public IActionResult Encrypt([FromBody] AesRequest request, [FromQuery] AesConstantsRequest aesConstants)
    {
        try
        {
            var text = request.PayLoad.ToString();
            var aes = CreateAes(aesConstants.Mode.ToString(), (int)aesConstants.KeySize, aesConstants.InitializationVector, aesConstants.SecretKey);

            byte[] encrypted;
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(text);
                encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }

            string result = aesConstants.OutputFormat
                .Equals(AesOutputFormat.Base64)
                ? Convert.ToBase64String(encrypted)
                : BitConverter.ToString(encrypted).Replace("-", "");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("decrypt")]
    public IActionResult Decrypt([FromBody] AesRequest request, [FromQuery] AesConstantsRequest aesConstants)
    {
        try
        {
            var aes = CreateAes(aesConstants.Mode.ToString(), (int)aesConstants.KeySize, aesConstants.InitializationVector, aesConstants.SecretKey);
            var text = request.PayLoad.ToString();

            byte[] cipherBytes = aesConstants.OutputFormat
                .Equals(AesOutputFormat.Base64)
                ? Convert.FromBase64String(text)
                : HexStringToByteArray(text);

            string plainText;
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                plainText = Encoding.UTF8.GetString(decryptedBytes);
            }

            return Ok(plainText);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("decrypt-encrypt")]
    public IActionResult DecryptEncrypt([FromBody] AesRequest request, [FromQuery] AesConstantsRequest aesConstants)
    {
        try
        {
            var text = request.PayLoad.ToString();
            var aes = CreateAes(aesConstants.Mode.ToString(), (int)aesConstants.KeySize, aesConstants.InitializationVector, aesConstants.SecretKey);

            if (aesConstants.Operation == AesOperation.Encrypt)
            {
                byte[] encrypted;
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(text);
                    encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }

                string result = aesConstants.OutputFormat == AesOutputFormat.Base64
                    ? Convert.ToBase64String(encrypted)
                    : BitConverter.ToString(encrypted).Replace("-", "");

                return Ok(result);
            }
            else if (aesConstants.Operation == AesOperation.Decrypt)
            {
                byte[] cipherBytes = aesConstants.OutputFormat == AesOutputFormat.Base64
                    ? Convert.FromBase64String(text)
                    : HexStringToByteArray(text);

                string plainText;
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    plainText = Encoding.UTF8.GetString(decryptedBytes);
                }

                return Ok(plainText);
            }
            else
            {
                return BadRequest("Invalid operation.");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    private static Aes CreateAes(string mode, int keySize, string iv, string secretKey)
    {
        var aes = Aes.Create();
        aes.KeySize = keySize;
        aes.Mode = mode.Equals(CipherMode.ECB) ?
            CipherMode.ECB : CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        aes.Key = Encoding.UTF8.GetBytes(secretKey);
        if (aes.Mode == CipherMode.CBC)
            aes.IV = Encoding.UTF8.GetBytes(iv);

        return aes;
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
}
