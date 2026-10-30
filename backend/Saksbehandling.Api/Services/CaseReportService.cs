using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;

namespace Saksbehandling.Api.Services;

public class DepartmentCaseCountRow
{
    public string DepartmentName { get; set; } = string.Empty;
    public int CaseCount { get; set; }
}

/// <summary>
/// Small internal reporting helper used for a one-off department workload
/// count. Not wired into the main case list - it was added for a
/// department-lead request and never got a UI of its own.
/// </summary>
public class CaseReportService
{
    private readonly AppDbContext _db;

    public CaseReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DepartmentCaseCountRow>> GetCaseCountsByDepartmentAsync(string? departmentCode)
    {
        var sql = @"
            SELECT d.""Name"" AS ""DepartmentName"", COUNT(c.""Id"") AS ""CaseCount""
            FROM ""Departments"" d
            LEFT JOIN ""Cases"" c ON c.""DepartmentId"" = d.""Id""";

        if (!string.IsNullOrWhiteSpace(departmentCode))
        {
            sql += $@" WHERE d.""Code"" = '{departmentCode}'";
        }

        sql += @" GROUP BY d.""Name""";

        return await _db.Database.SqlQueryRaw<DepartmentCaseCountRow>(sql).ToListAsync();
    }
}
