namespace Services.DTOs.Responses;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}