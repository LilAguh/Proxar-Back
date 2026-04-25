using AutoMapper;
using DataAccess.Repositories.Interfaces;
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
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
            throw new KeyNotFoundException("Empresa no encontrada");

        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto> GetBySlugAsync(string slug)
    {
        var company = await _companyRepository.GetBySlugAsync(slug);
        if (company == null)
            throw new KeyNotFoundException("Empresa no encontrada");

        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<IEnumerable<CompanyDto>> GetAllActiveAsync()
    {
        var companies = await _companyRepository.GetAllActiveAsync();
        return _mapper.Map<IEnumerable<CompanyDto>>(companies);
    }

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request)
    {
        // Verificar si el slug ya existe
        var existing = await _companyRepository.GetBySlugAsync(request.Slug);
        if (existing != null)
            throw new InvalidOperationException("El slug ya está en uso");

        var company = new Company
        {
            Name = request.Name,
            Slug = request.Slug.ToLower().Trim(),
            LogoUrl = request.LogoUrl,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _companyRepository.AddAsync(company);
        return _mapper.Map<CompanyDto>(created);
    }

    public async Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
            throw new KeyNotFoundException("Empresa no encontrada");

        company.Name = request.Name;
        company.LogoUrl = request.LogoUrl;
        company.Active = request.Active;

        await _companyRepository.UpdateAsync(company);
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task DeactivateCompanyAsync(Guid id, Guid deletedBy)
    {
        await _companyRepository.SoftDeleteAsync(id, deletedBy);
    }
}