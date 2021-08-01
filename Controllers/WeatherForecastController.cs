using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using webapi_identity.Repositorys;

namespace webapi_identity.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRepoWeather _weather;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, UserManager<IdentityUser> userManager, IRepoWeather weather)
        {
            _logger = logger;
            _userManager = userManager;
            _weather = weather;
        }



        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //<IEnumerable<WeatherForecast>>
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;



            //  var userId = _userManager.GetUserId(currentUser);
            //var userId = currentUser.
            var rng = new Random();

            var userId = currentUser.FindFirst("Id").Value;
            var user = await _userManager.FindByIdAsync(userId);

            // ProfileUpdateModel model = new ProfileUpdateModel();
            // model.Email = user.Email;
            // model.FirstName = user.FirstName;
            // model.LastName = user.LastName;
            // model.PhoneNumber = user.PhoneNumber;

            _weather.UserInfo();
            return Ok(new
            {
                id = rng,
                name = userId,
                user = user
            });


            // return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            // {
            //     Date = DateTime.Now.AddDays(index),
            //     TemperatureC = rng.Next(-20, 55),
            //     Summary = Summaries[rng.Next(Summaries.Length)]
            // })
            // .ToArray();
        }
    }
}
