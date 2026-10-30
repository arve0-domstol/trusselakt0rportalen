using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Dtos;

public class AuditEventDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }

    public static AuditEventDto From(AuditEvent e) => new()
    {
        Id = e.Id,
        Action = e.Action,
        Details = e.Details,
        UserName = e.User?.FullName,
        Timestamp = e.Timestamp,
    };
}
