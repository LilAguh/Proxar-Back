using Models.Enums;

namespace Services.DTOs.Responses;

public class TicketDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public ClientDto Client { get; set; } = null!;
    public UserDto CreatedBy { get; set; } = null!;
    public UserDto? AssignedTo { get; set; }
    public TicketType Type { get; set; }
    public TicketState Status { get; set; }
    public Priority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}