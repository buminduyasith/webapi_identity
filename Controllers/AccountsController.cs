
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
using webapi_identity.Repositories;

namespace webapi_identity.Controllers
{


    [Route("api/[controller]")] // api/authmanagement
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountsController> _logger;
        private readonly JwtConfig _jwtConfig;

        private readonly IAccountRepository _accountRepository;

        public AccountsController(UserManager<IdentityUser> userManager,
        IOptionsMonitor<JwtConfig> optionsMonitor, ILogger<AccountsController> logger, IAccountRepository accountRepository)
        {
            _userManager = userManager;
            _logger = logger;
            _jwtConfig = optionsMonitor.CurrentValue;
            _accountRepository = accountRepository;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Register(UserRegistrationRequestDto user)
        {
            var newUser = await _accountRepository.CreateUser(user);
            var jwtToken = await _accountRepository.GenerateJwtToken(newUser);
            return Ok(jwtToken);
        }


        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Signin([FromBody] UserLoginRequest user)
        {

            var result = await _accountRepository.UserLogin(user);
            return Ok(result);
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var res = await _accountRepository.VerifyToken(tokenRequest);

                if (res == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                    "Invalid tokens"
                },
                        Success = false
                    });
                }

                if (res.Errors != null && res.Errors.Count > 0)
                {
                    return BadRequest(new
                    {
                        result = res,
                        todo = "res.erros && res.errors.count"

                    });
                }

                return Ok(res);
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                "Invalid payload"
            },
                Success = false
            });
        }


        [HttpGet]
        [Route("loginwithgoogle/{idToken}")]
        public async Task<IActionResult> Idtoken(string idToken)
        {
            var jwtToken = await _accountRepository.VerifyFBIdToken(idToken);

            _logger.LogInformation("run");
            return Ok(jwtToken);
        }


    }



}