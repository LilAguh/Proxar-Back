using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DataAccess.Services;

namespace DataAccess.Converters;

/// <summary>
/// EF Core Value Converter for transparent encryption/decryption of string properties.
/// When EF Core writes to DB: plainText → Encrypt → cipherText (Base64)
/// When EF Core reads from DB: cipherText (Base64) → Decrypt → plainText
/// </summary>
public class EncryptedStringConverter : ValueConverter<string?, string?>
{
    public EncryptedStringConverter(IEncryptionService encryptionService)
        : base(
            plainText => plainText != null ? encryptionService.Encrypt(plainText) : null,
            cipherText => cipherText != null ? encryptionService.Decrypt(cipherText) : null)
    {
    }
}
