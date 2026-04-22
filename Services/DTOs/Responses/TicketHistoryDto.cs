using Models.Enums;

namespace Services.DTOs.Responses;

public class TicketHistoryDto
{
    public Guid Id { get; set; }
    public UserDto User { get; set; } = null!;
    public ActionHistorial Action { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; }
}