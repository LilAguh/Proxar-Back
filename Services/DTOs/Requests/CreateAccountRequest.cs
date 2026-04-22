using Models.Enums;

namespace Services.DTOs.Requests;

public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; } = 0;
}