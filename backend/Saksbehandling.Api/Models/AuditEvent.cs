namespace Saksbehandling.Api.Models;

/// <summary>Historikk / audit log entry for a case or other tracked entity.</summary>
public class AuditEvent
{
    public int Id { get; set; }

    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
