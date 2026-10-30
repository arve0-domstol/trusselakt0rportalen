using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Data;
using Saksbehandling.Api.Dtos;
using Saksbehandling.Api.Repositories;

namespace Saksbehandling.Api.Controllers;

/// <summary>
/// Original case API, kept around for an older mobile client that still
/// calls it directly. Superseded by <c>api/v2/cases</c>, which the current
/// frontend uses.
/// </summary>
[ApiController]
[Route("api/v1/cases")]
[Authorize]
public class CasesV1Controller : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CaseRepository _repository;

    public CasesV1Controller(AppDbContext db, CaseRepository repository)
    {
        _db = db;
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] CaseListQuery query)
    {
        var parameters = new CaseSearchParameters
        {
            DepartmentId = query.DepartmentId,
            Status = query.Status,
            Priority = query.Priority,
            Search = query.Search,
            Sort = query.Sort,
            SortDirection = query.SortDirection,
            Page = query.Page,
            PageSize = query.PageSize,
        };

        var (items, totalCount) = await _repository.SearchAsync(parameters);

        return Ok(new PagedResult<CaseSummaryDto>
        {
            Items = items.Select(CaseSummaryDto.From).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page < 1 ? 1 : parameters.Page,
            PageSize = parameters.PageSize is < 1 or > 100 ? 20 : parameters.PageSize,
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var found = await _db.Cases
            .Include(c => c.Customer)
            .Include(c => c.Department)
            .Include(c => c.AssignedToUser)
            .Include(c => c.ApprovedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (found is null)
        {
            return NotFound();
        }

        return Ok(CaseDetailDto.From(found));
    }
}
