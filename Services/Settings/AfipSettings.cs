namespace Services.Settings;

public class AfipSettings
{
    public string Environment { get; set; } = "Testing"; // Testing o Production
    public string Cuit { get; set; } = string.Empty;
    public string CertificatePath { get; set; } = string.Empty;
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string? CertificatePassword { get; set; }
}
