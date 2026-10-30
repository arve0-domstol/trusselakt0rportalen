using Microsoft.AspNetCore.Mvc;

namespace Saksbehandling.Api.Controllers;

/// <summary>Basic health check used to verify the Docker Compose wiring.</summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", service = "Saksbehandling.Api" });
}
