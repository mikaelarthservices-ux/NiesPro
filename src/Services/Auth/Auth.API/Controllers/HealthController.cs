using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Vérification de l'état de l'API
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            Status = "OK", 
            Service = "NiesPro Auth API",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}