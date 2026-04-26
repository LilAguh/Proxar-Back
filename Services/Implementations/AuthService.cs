using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.Enums;
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
        // Normalizar email
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Buscar usuario por email en TODAS las empresas activas
        User? foundUser = null;
        Company? foundCompany = null;

        var allCompanies = await _companyRepository.GetAllActiveAsync();

        foreach (var company in allCompanies)
        {
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, company.Id);
            if (user != null)
            {
                foundUser = user;
                foundCompany = company;
                break;
            }
        }

        // Si no encontramos el usuario en ninguna empresa
        if (foundUser == null)
        {
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");
        }

        // Verificar que el usuario esté activo
        if (!foundUser.Active)
        {
            throw new UnauthorizedAccessException("Usuario desactivado. Contactá al administrador.");
        }

        // Verificar que la empresa esté activa
        if (foundCompany == null || !foundCompany.Active)
        {
            throw new UnauthorizedAccessException("La empresa no está activa");
        }

        // Verificar contraseña
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, foundUser.PasswordHash);

        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");
        }

        // Generar token JWT con companyId incluido
        var token = GenerateJwtToken(foundUser);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var userDto = _mapper.Map<UserDto>(foundUser);

        return new AuthResponseDto
        {
            Token = token,
            User = userDto,
            ExpiresAt = expiresAt
        };
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim("CompanyId", user.CompanyId.ToString()), // ← CRÍTICO: CompanyId en el token
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request)
    {
        // Normalizar slug
        var normalizedSlug = request.CompanySlug.Trim().ToLowerInvariant();

        // Verificar si el slug ya existe
        var existingCompany = await _companyRepository.GetBySlugAsync(normalizedSlug);
        if (existingCompany != null)
            throw new InvalidOperationException("El slug de empresa ya está en uso. Elegí otro.");

        // Crear la empresa
        var company = new Company
        {
            Name = request.CompanyName,
            Slug = normalizedSlug,
            LogoUrl = request.LogoUrl,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdCompany = await _companyRepository.CreateAsync(company);

        // Crear el usuario owner/admin
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            CompanyId = createdCompany.Id,
            Name = request.Name,
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);

        // Generar token
        var token = GenerateJwtToken(createdUser);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var userDto = _mapper.Map<UserDto>(createdUser);

        return new AuthResponseDto
        {
            Token = token,
            User = userDto,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponseDto> LoginBySlugAsync(string slug, LoginRequest request)
    {
        // Normalizar slug y email
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Buscar la empresa por slug
        var company = await _companyRepository.GetBySlugAsync(normalizedSlug);
        if (company == null)
            throw new UnauthorizedAccessException("Empresa no encontrada");

        // Verificar que la empresa esté activa
        if (!company.Active)
            throw new UnauthorizedAccessException("La empresa no está activa");

        // Buscar usuario por email en esta empresa específica
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, company.Id);
        if (user == null)
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        // Verificar que el usuario esté activo
        if (!user.Active)
            throw new UnauthorizedAccessException("Usuario desactivado. Contactá al administrador.");

        // Verificar contraseña
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        // Generar token JWT
        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            User = userDto,
            ExpiresAt = expiresAt
        };
    }
}