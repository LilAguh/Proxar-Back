namespace Services.DTOs.Requests;

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string CompanySlug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }
}
