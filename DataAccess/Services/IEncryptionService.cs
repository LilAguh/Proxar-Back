namespace DataAccess.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive data.
/// Used by EF Core Value Converters to transparently encrypt fields in the database.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plain text string using AES-256.
    /// Returns Base64-encoded string: IV (16 bytes) + CipherText
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts a Base64-encoded cipher text using AES-256.
    /// Expects format: IV (16 bytes) + CipherText
    /// </summary>
    string Decrypt(string cipherText);
}
