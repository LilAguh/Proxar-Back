namespace Services.DTOs.Requests;

public class UpdateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool Active { get; set; }
}