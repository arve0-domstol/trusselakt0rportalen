namespace Saksbehandling.Api.Models;

/// <summary>Varsel. An in-app notification for a user.</summary>
public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? RelatedCaseId { get; set; }
    public Case? RelatedCase { get; set; }
}
