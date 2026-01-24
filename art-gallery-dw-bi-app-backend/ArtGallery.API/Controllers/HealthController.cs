using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Infrastructure.Data;

namespace ArtGallery.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Basic health check endpoint.
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Checks = new
            {
                Api = "healthy"
            }
        });
    }

    /// <summary>
    /// Readiness check that verifies database connectivity.
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (canConnect)
            {
                return Ok(new
                {
                    Status = "healthy",
                    Timestamp = DateTime.UtcNow,
                    Checks = new
                    {
                        Database = "healthy",
                        Api = "healthy"
                    }
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                Status = "unhealthy",
                Timestamp = DateTime.UtcNow,
                Checks = new
                {
                    Database = "unhealthy",
                    Api = "healthy"
                },
                Error = ex.Message
            });
        }

        return StatusCode(503, new
        {
            Status = "unhealthy",
            Timestamp = DateTime.UtcNow
        });
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InfoController : ControllerBase
{
    /// <summary>
    /// Gets API version and metadata.
    /// </summary>
    [HttpGet]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            Name = "Art Gallery Management API",
            Version = "1.0.0",
            Description = "RESTful API for Art Gallery Management System",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Framework = ".NET 10",
            Timestamp = DateTime.UtcNow
        });
    }
}
