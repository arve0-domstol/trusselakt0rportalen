using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Dtos;

public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RelatedCaseId { get; set; }

    public static NotificationDto From(Notification n) => new()
    {
        Id = n.Id,
        Message = n.Message,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
        RelatedCaseId = n.RelatedCaseId,
    };
}
