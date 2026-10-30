using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Repositories;

/// <summary>Flat projection used by the case list/search endpoint.</summary>
public class CaseSearchRow
{
    public int Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Priority { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? Deadline { get; set; }
}

public class CaseSearchParameters
{
    public int? DepartmentId { get; set; }
    public int? AssignedToUserId { get; set; }
    public CaseStatus? Status { get; set; }
    public CasePriority? Priority { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CaseRepository
{
    private readonly AppDbContext _db;

    public CaseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<CaseSearchRow> Items, int TotalCount)> SearchAsync(CaseSearchParameters p)
    {
        var (whereSql, whereParams) = BuildWhere(p);

        // EF Core's SqlQueryRaw<T> for a primitive result type requires the
        // returned column to be named "Value".
        var countSql = $@"SELECT COUNT(*) AS ""Value"" FROM ""Cases"" c {whereSql}";
        var totalCount = await _db.Database
            .SqlQueryRaw<int>(countSql, ToNpgsqlParams(whereParams))
            .FirstAsync();

        // A handful of sort keys are mapped to real column names; anything
        // else is passed straight through so new frontend sort options work
        // without a backend change.
        var sortColumn = p.Sort switch
        {
            "title" => "Title",
            "deadline" => "Deadline",
            "priority" => "Priority",
            "status" => "Status",
            "caseNumber" => "CaseNumber",
            null or "" => "CreatedAt",
            _ => p.Sort,
        };
        var direction = string.Equals(p.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

        var page = p.Page < 1 ? 1 : p.Page;
        var pageSize = p.PageSize is < 1 or > 100 ? 20 : p.PageSize;

        var dataParams = new List<(string Name, object Value)>(whereParams)
        {
            ("limit", pageSize),
            ("offset", (page - 1) * pageSize),
        };

        var dataSql = $@"
            SELECT
                c.""Id"", c.""CaseNumber"", c.""Title"", c.""Description"", c.""Status"", c.""Priority"",
                c.""DepartmentId"", d.""Name"" AS ""DepartmentName"",
                c.""CustomerId"", cu.""Name"" AS ""CustomerName"",
                c.""AssignedToUserId"", u.""FullName"" AS ""AssignedToName"",
                c.""CreatedAt"", c.""UpdatedAt"", c.""Deadline""
            FROM ""Cases"" c
            JOIN ""Departments"" d ON d.""Id"" = c.""DepartmentId""
            JOIN ""Customers"" cu ON cu.""Id"" = c.""CustomerId""
            LEFT JOIN ""Users"" u ON u.""Id"" = c.""AssignedToUserId""
            {whereSql}
            ORDER BY c.""{sortColumn}"" {direction}
            LIMIT @limit OFFSET @offset";

        var items = await _db.Database
            .SqlQueryRaw<CaseSearchRow>(dataSql, ToNpgsqlParams(dataParams))
            .ToListAsync();

        return (items, totalCount);
    }

    private static (string Sql, List<(string Name, object Value)> Params) BuildWhere(CaseSearchParameters p)
    {
        var sql = new StringBuilder("WHERE 1=1");
        var parameters = new List<(string Name, object Value)>();

        if (p.DepartmentId.HasValue)
        {
            sql.Append(@" AND c.""DepartmentId"" = @departmentId");
            parameters.Add(("departmentId", p.DepartmentId.Value));
        }

        if (p.AssignedToUserId.HasValue)
        {
            sql.Append(@" AND c.""AssignedToUserId"" = @assignedToUserId");
            parameters.Add(("assignedToUserId", p.AssignedToUserId.Value));
        }

        if (p.Status.HasValue)
        {
            sql.Append(@" AND c.""Status"" = @status");
            parameters.Add(("status", (int)p.Status.Value));
        }

        if (p.Priority.HasValue)
        {
            sql.Append(@" AND c.""Priority"" = @priority");
            parameters.Add(("priority", (int)p.Priority.Value));
        }

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            // Free-text search across the case's own title/description/number.
            // Built directly from the query term so partial-word matches
            // (ILIKE '%term%') behave the same way for every field.
            sql.Append($@" AND (c.""Title"" ILIKE '%{p.Search}%' OR c.""Description"" ILIKE '%{p.Search}%' OR c.""CaseNumber"" ILIKE '%{p.Search}%')");
        }

        return (sql.ToString(), parameters);
    }

    private static NpgsqlParameter[] ToNpgsqlParams(IEnumerable<(string Name, object Value)> parameters) =>
        parameters.Select(p => new NpgsqlParameter(p.Name, p.Value)).ToArray();
}
