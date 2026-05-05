using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DataAccess.Services;

/// <summary>
/// AES-256 encryption service for sensitive data.
/// Key is loaded from configuration (appsettings.json or environment variables).
/// Each encryption generates a random IV (Initialization Vector) for security.
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "Encryption key not found in configuration. Set 'Encryption:Key' in appsettings.json or environment variable.");

        // Key must be 256 bits (32 bytes) for AES-256
        if (keyString.Length != 64) // 32 bytes = 64 hex chars
        {
            throw new InvalidOperationException(
                "Encryption key must be 64 hexadecimal characters (32 bytes for AES-256). " +
                "Generate one with: openssl rand -hex 32");
        }

        _key = Convert.FromHexString(keyString);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV(); // Random IV for each encryption

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV + CipherText and encode as Base64
        var combined = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var combined = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV (first 16 bytes) and CipherText (rest)
            var iv = new byte[16];
            var cipherBytes = new byte[combined.Length - 16];
            Buffer.BlockCopy(combined, 0, iv, 0, 16);
            Buffer.BlockCopy(combined, 16, cipherBytes, 0, cipherBytes.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            throw new CryptographicException(
                "Failed to decrypt data. This may indicate a wrong encryption key or corrupted data.", ex);
        }
    }
}
