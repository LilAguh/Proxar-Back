using Models.Enums;

namespace Services.DTOs.Requests;

public class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool Active { get; set; }
}