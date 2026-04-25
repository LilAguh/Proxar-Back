using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Models;

namespace Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository, 
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null)
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        if (!user.Active)
            throw new UnauthorizedAccessException("Usuario desactivado");

        // Verificar contraseña
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        
        if (!isValidPassword)
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        // Generar token
        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        return new AuthResponseDto
        {
            Token = token,
            User = _mapper.Map<UserDto>(user),
            ExpiresAt = expiresAt
        };
    }

    public async Task<UserDto> RegisterAsync(RegisterUserRequest request)
    {
        // Verificar si el email ya existe
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("El email ya está registrado");

        // Hash de la contraseña
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = request.Role,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        // Verificar si el email ya existe en otro usuario
        if (user.Email != request.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("El email ya está en uso");
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.Role = request.Role;
        user.Active = request.Active;
        user.ModifiedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        user.Active = false;
        user.ModifiedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        // Verificar contraseña actual
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isValidPassword)
            throw new UnauthorizedAccessException("Contraseña actual incorrecta");

        // Actualizar contraseña
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ModifiedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}