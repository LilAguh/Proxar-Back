using Models.Enums;

namespace Services.DTOs.Requests;

public class UpdateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public bool Active { get; set; } = true;
}
