using Microsoft.AspNetCore.Mvc;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TestController> _logger;

    public TestController(IHttpClientFactory httpClientFactory, ILogger<TestController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Test Circuit Breaker by calling BookApi
    /// </summary>
    [HttpGet("circuit-breaker")]
    public async Task<IActionResult> TestCircuitBreaker()
    {
        var client = _httpClientFactory.CreateClient("BookApiClient");
        
        try
        {
            _logger.LogInformation("🔵 Attempting to call BookApi...");
            var response = await client.GetAsync("/api/books");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ BookApi call succeeded!");
                return Ok(new { status = "success", message = "BookApi is reachable" });
            }
            else
            {
                _logger.LogWarning("⚠️ BookApi returned: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, new { status = "error", message = $"BookApi returned {response.StatusCode}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Circuit breaker may be open or BookApi is down");
            return StatusCode(503, new { status = "circuit_open", message = ex.Message });
        }
    }
}