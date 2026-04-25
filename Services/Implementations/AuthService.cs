using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request)
    {
        // Extraer companyId del email o usar slug
        // Por ahora asumimos que el email viene con formato: user@company-slug
        // O podríamos pasar companyId explícitamente en el request
        
        // TEMPORAL: buscar en todas las companies hasta encontrar el usuario
        // En producción, el login debería recibir el companyId o slug
        User? user = null;
        var companies = await _companyRepository.GetAllActiveAsync();
        
        foreach (var company in companies)
        {
            user = await _userRepository.GetByEmailAsync(request.Email, company.Id);
            if (user != null) break;
        }
        
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

    public async Task<UserDto> GetUserByIdAsync(Guid userId, Guid companyId)
    {
        var user = await _userRepository.GetByIdAsync(userId, companyId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersByCompanyAsync(Guid companyId)
    {
        var users = await _userRepository.GetAllByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> RegisterUserAsync(RegisterUserRequest request, Guid companyId)
    {
        // Verificar si el email ya existe en la empresa
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, companyId);
        if (existingUser != null)
            throw new InvalidOperationException("El email ya está registrado en esta empresa");

        // Hash de la contraseña
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            CompanyId = companyId,
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

    public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid companyId)
    {
        var user = await _userRepository.GetByIdAsync(userId, companyId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        // Verificar si el email ya existe en otro usuario de la empresa
        if (user.Email != request.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, companyId);
            if (existingUser != null)
                throw new InvalidOperationException("El email ya está en uso en esta empresa");
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.Role = request.Role;
        user.Active = request.Active;

        await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }

    public async Task DeactivateUserAsync(Guid userId, Guid companyId, Guid deletedBy)
    {
        await _userRepository.SoftDeleteAsync(userId, companyId, deletedBy);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, Guid companyId)
    {
        var user = await _userRepository.GetByIdAsync(userId, companyId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        // Verificar contraseña actual
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isValidPassword)
            throw new UnauthorizedAccessException("Contraseña actual incorrecta");

        // Actualizar contraseña
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("CompanyId", user.CompanyId.ToString()),
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