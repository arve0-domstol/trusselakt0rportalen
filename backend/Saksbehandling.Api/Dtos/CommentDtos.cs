using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Dtos;

public class CommentDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static CommentDto From(Comment c) => new()
    {
        Id = c.Id,
        Text = c.Text,
        IsInternal = c.IsInternal,
        AuthorName = c.AuthorUser?.FullName ?? string.Empty,
        CreatedAt = c.CreatedAt,
    };
}

public class CreateCommentRequest
{
    public string Text { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
