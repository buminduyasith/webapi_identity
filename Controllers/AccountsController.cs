
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using webapi_identity.configuration;
using webapi_identity.DTOs;
using webapi_identity.Repositorys;

namespace webapi_identity.Controllers
{


    [Route("api/[controller]")] // api/authmanagement
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountsController> _logger;
        private readonly JwtConfig _jwtConfig;

        private readonly IAccountRepository _weather;

        public AccountsController(UserManager<IdentityUser> userManager,
        IOptionsMonitor<JwtConfig> optionsMonitor, ILogger<AccountsController> logger, IAccountRepository weather)
        {
            _userManager = userManager;
            _logger = logger;
            _jwtConfig = optionsMonitor.CurrentValue;
            _weather = weather;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Register(UserRegistrationRequestDto user)
        {

            var existingUser = await _weather.FindByEmailAsync(user.Email);

            if (existingUser != null)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Result = false,
                    Errors = new List<string>() { "Email already exist" }
                });
            }

            else
            {

                var jwtToken = await _weather.CreateUser(user);

                if (jwtToken is string)
                {
                    return Ok(new RegistrationResponse()
                    {
                        Result = true,
                        Token = jwtToken.ToString()

                    });
                }
                else
                {
                    var r = (IdentityResult)jwtToken;
                    return BadRequest(new
                    {
                        Result = false,
                        Errors = r.Errors.Select(x => x.Description).ToList(),
                    });


                }
            }



        }


        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Signin([FromBody] UserLoginRequest user)
        {


            var existingUser = await _weather.FindByEmailAsync(user.Email);

            if (existingUser == null)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Result = false,
                    Errors = new List<string>() { "no user found" }
                });
            }

            else
            {
                var isSuccessLogin = await _weather.UserLogin(existingUser, user.Password);

                if (isSuccessLogin)
                {
                    var jwtToken = _weather.GenerateJwtToken(existingUser);

                    return Ok(new RegistrationResponse()
                    {
                        Result = true,
                        Token = jwtToken
                    });
                }
                else
                {

                    return BadRequest(new RegistrationResponse()
                    {
                        Result = false,
                        Errors = new List<string>(){
                                         "Invalid authentication request"
                                    }
                    });
                }
            }


        }

    }



}