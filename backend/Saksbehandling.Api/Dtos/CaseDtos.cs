using Saksbehandling.Api.Models;
using Saksbehandling.Api.Repositories;

namespace Saksbehandling.Api.Dtos;

public class CaseSummaryDto
{
    public int Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }

    public static CaseSummaryDto From(CaseSearchRow row) => new()
    {
        Id = row.Id,
        CaseNumber = row.CaseNumber,
        Title = row.Title,
        Status = ((CaseStatus)row.Status).ToString(),
        Priority = ((CasePriority)row.Priority).ToString(),
        CustomerName = row.CustomerName,
        DepartmentName = row.DepartmentName,
        AssignedToName = row.AssignedToName,
        CreatedAt = row.CreatedAt,
        Deadline = row.Deadline,
    };
}

public class CaseDetailDto
{
    public int Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? ClosedAt { get; set; }

    public static CaseDetailDto From(Case c) => new()
    {
        Id = c.Id,
        CaseNumber = c.CaseNumber,
        Title = c.Title,
        Description = c.Description,
        Status = c.Status.ToString(),
        Priority = c.Priority.ToString(),
        CustomerId = c.CustomerId,
        CustomerName = c.Customer?.Name ?? string.Empty,
        DepartmentId = c.DepartmentId,
        DepartmentName = c.Department?.Name ?? string.Empty,
        AssignedToUserId = c.AssignedToUserId,
        AssignedToName = c.AssignedToUser?.FullName,
        ApprovedByUserId = c.ApprovedByUserId,
        ApprovedByName = c.ApprovedByUser?.FullName,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Deadline = c.Deadline,
        ClosedAt = c.ClosedAt,
    };
}

public class CaseListQuery
{
    public int? DepartmentId { get; set; }
    public bool? Mine { get; set; }
    public CaseStatus? Status { get; set; }
    public CasePriority? Priority { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateCaseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? DepartmentId { get; set; }
    public CasePriority Priority { get; set; } = CasePriority.Normal;
    public DateTime? Deadline { get; set; }
}

public class UpdateCaseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CaseStatus Status { get; set; }
    public CasePriority Priority { get; set; }
    public int? AssignedToUserId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime? Deadline { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class AssignCaseRequest
{
    public int AssignedToUserId { get; set; }
}

public class CloseCaseRequest
{
    public string? DecisionText { get; set; }
}
