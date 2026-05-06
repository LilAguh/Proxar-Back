using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.Enums;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAccountRepository accountRepository,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

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

        if (foundUser == null)
            throw new UnauthorizedAccessException(AppMessages.Auth.InvalidCredentials);

        if (!foundUser.Active)
            throw new UnauthorizedAccessException(AppMessages.Auth.UserDisabled);

        if (foundCompany == null || !foundCompany.Active)
            throw new UnauthorizedAccessException(AppMessages.Auth.CompanyInactive);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, foundUser.PasswordHash))
            throw new UnauthorizedAccessException(AppMessages.Auth.InvalidCredentials);

        return await BuildAuthResponseAsync(foundUser);
    }

    public async Task<AuthResponseDto> LoginBySlugAsync(string slug, LoginRequest request)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var company = await _companyRepository.GetBySlugAsync(normalizedSlug);
        if (company == null)
            throw new UnauthorizedAccessException(AppMessages.Company.NotFound);

        if (!company.Active)
            throw new UnauthorizedAccessException(AppMessages.Auth.CompanyInactive);

        var user = await _userRepository.GetByEmailAsync(normalizedEmail, company.Id);
        if (user == null)
            throw new UnauthorizedAccessException(AppMessages.Auth.InvalidCredentials);

        if (!user.Active)
            throw new UnauthorizedAccessException(AppMessages.Auth.UserDisabled);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException(AppMessages.Auth.InvalidCredentials);

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request)
    {
        var normalizedSlug = request.CompanySlug.Trim().ToLowerInvariant();

        var existingCompany = await _companyRepository.GetBySlugAsync(normalizedSlug);
        if (existingCompany != null)
            throw new ConflictException(AppMessages.Auth.SlugAlreadyInUse);

        var company = new Company
        {
            Name = request.CompanyName,
            Slug = normalizedSlug,
            LegalName = request.LegalName ?? request.CompanyName,
            LogoUrl = request.LogoUrl,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,

            // Datos fiscales
            CUIT = request.CUIT,
            IVA = request.IVA,
            IIBB = request.IIBB,
            FiscalAddress = request.FiscalAddress,
            FiscalCity = request.FiscalCity,
            FiscalProvince = request.FiscalProvince,
            FiscalPostalCode = request.FiscalPostalCode,
            StartOfActivities = DateTime.SpecifyKind(request.StartOfActivities, DateTimeKind.Utc),
            DefaultSalesPoint = request.DefaultSalesPoint,

            // Contacto
            Email = request.CompanyEmail,
            Phone = request.Phone
        };

        var createdCompany = await _companyRepository.CreateAsync(company);

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

        // Crear cuentas por defecto para la empresa
        var defaultAccounts = new[]
        {
            new Account
            {
                CompanyId = createdCompany.Id,
                Name = "Efectivo",
                Type = AccountType.Efectivo,
                CurrentBalance = 0,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Account
            {
                CompanyId = createdCompany.Id,
                Name = "Banco",
                Type = AccountType.Banco,
                CurrentBalance = 0,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Account
            {
                CompanyId = createdCompany.Id,
                Name = "Mercado Pago",
                Type = AccountType.MercadoPago,
                CurrentBalance = 0,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            }
        };

        foreach (var account in defaultAccounts)
        {
            await _accountRepository.AddAsync(account);
        }

        return await BuildAuthResponseAsync(createdUser);
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, Guid companyId)
    {
        var user = await _userRepository.GetByIdAsync(userId, companyId)
            ?? throw new NotFoundException(AppMessages.User.NotFound);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersByCompanyAsync(Guid companyId)
    {
        var users = await _userRepository.GetAllByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> RegisterUserAsync(RegisterUserRequest request, Guid companyId)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, companyId);
        if (existingUser != null)
            throw new ConflictException(AppMessages.Auth.EmailAlreadyRegistered);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            CompanyId = companyId,
            Name = request.Name,
            Email = normalizedEmail,
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
        var user = await _userRepository.GetByIdAsync(userId, companyId)
            ?? throw new NotFoundException(AppMessages.User.NotFound);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (user.Email != normalizedEmail)
        {
            var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, companyId);
            if (existingUser != null)
                throw new ConflictException(AppMessages.Auth.EmailAlreadyInUse);
        }

        user.Name = request.Name;
        user.Email = normalizedEmail;
        user.Role = request.Role;
        user.Active = request.Active;

        await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }

    public async Task DeactivateUserAsync(Guid userId, Guid companyId, Guid deletedBy)
    {
        // SEGURIDAD: Desactivar usuario Y revocar todos sus refresh tokens
        // Sin esto, el usuario desactivado puede seguir usando tokens existentes
        await _userRepository.SoftDeleteAsync(userId, companyId, deletedBy);
        await _refreshTokenRepository.RevokeAllForUserAsync(userId, companyId);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, Guid companyId)
    {
        var user = await _userRepository.GetByIdAsync(userId, companyId)
            ?? throw new NotFoundException(AppMessages.User.NotFound);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException(AppMessages.Auth.WrongCurrentPassword);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await _refreshTokenRepository.RevokeIfActiveAsync(tokenHash);
        if (stored == null)
            throw new UnauthorizedAccessException(AppMessages.Auth.InvalidRefreshToken);

        return await BuildAuthResponseAsync(stored.User);
    }

    public async Task RevokeTokenAsync(string refreshToken, Guid userId, Guid companyId)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash)
            ?? throw new UnauthorizedAccessException(AppMessages.Auth.InvalidRefreshToken);

        if (stored.UserId != userId)
            throw new ForbiddenException();

        stored.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(stored);
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var accessToken = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var (plainRefreshToken, refreshTokenEntity) = GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        // Obtener la company del usuario
        var company = await _companyRepository.GetByIdAsync(user.CompanyId)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = plainRefreshToken,
            User = _mapper.Map<UserDto>(user),
            Company = _mapper.Map<CompanyDto>(company),
            ExpiresAt = expiresAt,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
        };
    }

    private (string plain, RefreshToken entity) GenerateRefreshToken(User user)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var plainToken = Convert.ToBase64String(randomBytes);

        var entity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(plainToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        return (plainToken, entity);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("CompanyId", user.CompanyId.ToString()),
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
}
