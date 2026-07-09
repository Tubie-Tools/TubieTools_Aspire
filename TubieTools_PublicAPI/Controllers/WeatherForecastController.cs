using Microsoft.AspNetCore.Mvc;

namespace TubieTools_PublicAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IServiceTenant _serviceTenant;
        /// <summary>
        /// Service tenant for multi tenants.
        /// </summary>
        /// <param name="serviceTenant"></param>
        /// <param name="logger"></param>
        public WeatherForecastController(IServiceTenant serviceTenant, ILogger<WeatherForecastController> logger)
        {
            _serviceTenant = serviceTenant;
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var resolved = _serviceTenant.GetType().FullName;

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
