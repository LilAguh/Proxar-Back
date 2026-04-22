using Models.Enums;

namespace Services.DTOs.Requests;

public class UpdateTicketStatusRequest
{
    public TicketState NewStatus { get; set; }
    public string? Comment { get; set; }
}