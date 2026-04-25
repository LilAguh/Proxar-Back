namespace Services.DTOs.Responses;

public class TicketDetailsDto : TicketDto
{
    public List<TicketHistoryDto> History { get; set; } = new();
    public List<BoxMovementDto> Movements { get; set; } = new();
}