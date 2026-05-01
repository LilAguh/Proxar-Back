using AutoMapper;
using DataAccess.Repositories.Interfaces;
using Exceptions;
using Models;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;

    public CompanyService(ICompanyRepository companyRepository, IMapper mapper)
    {
        _companyRepository = companyRepository;
        _mapper = mapper;
    }

    public async Task<CompanyDto> GetByIdAsync(Guid id)
    {
        var company = await _companyRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto> GetBySlugAsync(string slug)
    {
        var company = await _companyRepository.GetBySlugAsync(slug)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<IEnumerable<CompanyDto>> GetAllActiveAsync()
    {
        var companies = await _companyRepository.GetAllActiveAsync();
        return _mapper.Map<IEnumerable<CompanyDto>>(companies);
    }

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request)
    {
        var existing = await _companyRepository.GetBySlugAsync(request.Slug);
        if (existing != null)
            throw new ConflictException(AppMessages.Company.SlugAlreadyInUse);

        var company = new Company
        {
            Name = request.Name,
            Slug = request.Slug.ToLower().Trim(),
            LogoUrl = request.LogoUrl,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _companyRepository.CreateAsync(company);
        return _mapper.Map<CompanyDto>(created);
    }

    public async Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request)
    {
        var company = await _companyRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(AppMessages.Company.NotFound);

        // Identificación
        company.Name = request.Name;
        company.LegalName = request.LegalName;
        company.LogoUrl = request.LogoUrl;
        company.Website = request.Website;

        // Datos fiscales
        company.CUIT = request.CUIT;
        if (!string.IsNullOrWhiteSpace(request.IVA) && Enum.TryParse<Models.Enums.IVACondition>(request.IVA, out var ivaCondition))
        {
            company.IVA = ivaCondition;
        }
        company.IIBB = request.IIBB;
        company.FiscalAddress = request.FiscalAddress;
        company.FiscalCity = request.FiscalCity;
        company.FiscalProvince = request.FiscalProvince;
        company.FiscalPostalCode = request.FiscalPostalCode;
        company.StartOfActivities = request.StartOfActivities;
        company.DefaultSalesPoint = request.DefaultSalesPoint;

        // Contacto
        company.Email = request.Email;
        company.Phone = request.Phone;
        company.MobilePhone = request.MobilePhone;
        company.SupportEmail = request.SupportEmail;

        // Configuración regional
        if (!string.IsNullOrWhiteSpace(request.Currency))
            company.Currency = request.Currency;
        if (!string.IsNullOrWhiteSpace(request.TimeZoneId))
            company.TimeZoneId = request.TimeZoneId;
        if (!string.IsNullOrWhiteSpace(request.Language))
            company.Language = request.Language;
        if (!string.IsNullOrWhiteSpace(request.DateFormat))
            company.DateFormat = request.DateFormat;

        company.UpdatedAt = DateTime.UtcNow;

        await _companyRepository.UpdateAsync(company);
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task DeactivateCompanyAsync(Guid id, Guid deletedBy)
    {
        await _companyRepository.SoftDeleteAsync(id, deletedBy);
    }
}
