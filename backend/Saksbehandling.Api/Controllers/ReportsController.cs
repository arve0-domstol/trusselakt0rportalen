using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saksbehandling.Api.Services;

namespace Saksbehandling.Api.Controllers;

[ApiController]
[Route("api/v2/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly CaseReportService _reportService;

    public ReportsController(CaseReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("department-case-counts")]
    public async Task<IActionResult> DepartmentCaseCounts([FromQuery] string? departmentCode)
    {
        var rows = await _reportService.GetCaseCountsByDepartmentAsync(departmentCode);
        return Ok(rows);
    }
}
