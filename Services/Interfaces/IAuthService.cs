using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequest request);
    Task<UserDto> GetUserByIdAsync(Guid userId, Guid companyId);
    Task<IEnumerable<UserDto>> GetAllUsersByCompanyAsync(Guid companyId);
    Task<UserDto> RegisterUserAsync(RegisterUserRequest request, Guid companyId);
    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid companyId);
    Task DeactivateUserAsync(Guid userId, Guid companyId, Guid deletedBy);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, Guid companyId);
}