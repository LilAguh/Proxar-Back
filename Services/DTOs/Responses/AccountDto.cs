using Models.Enums;

namespace Services.DTOs.Responses;

public class AccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}