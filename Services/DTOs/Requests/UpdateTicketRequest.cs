using Models.Enums;

namespace Services.DTOs.Requests;

public class UpdateTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public TicketType Type { get; set; }
    public Priority Priority { get; set; }
    public Guid? AssignedToId { get; set; }
}
