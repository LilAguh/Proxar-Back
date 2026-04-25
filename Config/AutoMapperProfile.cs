using AutoMapper;
using Models;
using Services.DTOs.Requests;
using Services.DTOs.Responses;

namespace Config;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Company, CompanyDto>();
        
        // Client mappings
        CreateMap<Client, ClientDto>();
        CreateMap<CreateClientRequest, Client>();
        CreateMap<UpdateClientRequest, Client>();

        // User mappings
        CreateMap<User, UserDto>();

        // Ticket mappings
        CreateMap<Ticket, TicketDto>();
        CreateMap<Ticket, TicketDetailDto>();
        CreateMap<CreateTicketRequest, Ticket>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Models.Enums.TicketState.Nuevo))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // TicketHistory mappings
        CreateMap<TicketHistory, TicketHistoryDto>();

        // Account mappings
        CreateMap<Account, AccountDto>();
        CreateMap<CreateAccountRequest, Account>()
            .ForMember(dest => dest.CurrentBalance, opt => opt.MapFrom(src => src.InitialBalance));

        // BoxMovement mappings
        CreateMap<BoxMovement, BoxMovementDto>()
            .ForMember(dest => dest.TicketNumber, opt => opt.MapFrom(src => src.Ticket != null ? src.Ticket.Number : (int?)null));
        CreateMap<RegisterMovementRequest, BoxMovement>()
            .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}