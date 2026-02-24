using Microsoft.AspNetCore.Mvc;

namespace ScimTest.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly AppDbContext _ctx;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, AppDbContext ctx)
    {
        _logger = logger;
        _ctx = ctx;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
    
    [HttpDelete(Name = "GetWeatherForecast")]
    public async Task<IActionResult> Delete()
    {
        var allusers= _ctx.Users.ToList();
        _ctx.Users.RemoveRange(allusers);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}