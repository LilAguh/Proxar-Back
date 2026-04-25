using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequest request);
    Task<UserDto> RegisterAsync(RegisterUserRequest request);
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task DeactivateUserAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}