using System;
using webapi_identity.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using webapi_identity.configuration;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using webapi_identity.DTOs;
using webapi_identity.Domains;
using webapi_identity.DataAccess;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using webapi_identity.Exceptions;

namespace webapi_identity.Services
{
    public class AccountService : IAccountRepository
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly TestDbContext _apiDbContext;



        public AccountService(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor,
        ILoggerFactory logger, TokenValidationParameters tokenValidationParameters,
        TestDbContext apiDbContext)

        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _logger = logger.CreateLogger("weatherService");
            _tokenValidationParameters = tokenValidationParameters;
            _apiDbContext = apiDbContext;
        }

        public async Task<IdentityUser> CreateUser(UserRegistrationRequestDto userRegisterDto)
        {

            var existingUser = await FindByEmailAsync(userRegisterDto.Email);

            if (existingUser != null)
            {
                throw new UserAlreadyExistsException("user already exists in the system");
            }


            var newUser = new IdentityUser() { Email = userRegisterDto.Email, UserName = userRegisterDto.Email };
            var isCreated = await _userManager.CreateAsync(newUser, userRegisterDto.Password);

            if (isCreated.Succeeded)
            {
                return newUser;
            }

            else
            {
                // TODO: what if user not created propely or something went wrong in the database

                throw new Exception("user not created");
            }

            //   var r = (IdentityResult)jwtToken;
            //     //     return BadRequest(new
            //     //     {
            //     //         Success = false,
            //     //         Errors = r.Errors.Select(x => x.Description).ToList(),
            //     //     });

        }

        public async Task<IdentityUser> FindByEmailAsync(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            return existingUser;
        }

        public async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            // define the jwt token which will be responsible of creating our tokens
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            // We get our secret from the appsettings
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // the JTI is used for our refresh token which we will be convering in the next video
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
                // the life span of the token needs to be shorter and utilise refresh token to keep the user signedin
                // but since this is a demo app we can extend it to fit our current need
                // Expires = DateTime.UtcNow.AddHours(1),
                Expires = DateTime.Now.AddMinutes(4),
                // here we are adding the encryption alogorithim information which will be used to decrypt our token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                IsRevoked = false,
                Token = RandomString(25) + Guid.NewGuid()
            };

            await _apiDbContext.RefreshToken.AddAsync(refreshToken);

            await _apiDbContext.SaveChangesAsync();

            return new AuthResult()
            {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResult> UserLogin(UserLoginRequest user)
        {

            var existingUser = await FindByEmailAsync(user.Email);

            if (existingUser == null)
            {
                throw new UserNotFoundException("User Not found");
            }

            var isSuccessLogin = await _userManager.CheckPasswordAsync(existingUser, user.Password);

            if (isSuccessLogin)
            {
                var jwtToken = await GenerateJwtToken(existingUser);
                return jwtToken;
            }

            else
            {
                throw new InvalidUserException();
            }





            //  return BadRequest(new RegistrationResponse()
            //     {
            //         Success = false,
            //         Errors = new List<string>() { "no user found" }
            //     });


            // TODO: what if user not created propely or something went wrong in the database
        }

        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(x => x[random.Next(x.Length)]).ToArray());
        }
        public async Task<AuthResult> VerifyToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // This validation function will make sure that the token meets the validation parameters
                // and its an actual jwt token not just a random string
                _tokenValidationParameters.ValidateLifetime = false;
                var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);
                _tokenValidationParameters.ValidateLifetime = true;
                // Now we need to check if the token has a valid security algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);

                    _logger.LogError(jwtSecurityToken.Header.Alg);
                    if (result == false)
                    {
                        return null;
                    }
                }

                // Will get the time stamp in unix time

                var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                // we convert the expiry date from seconds to the date
                var expDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "We cannot refresh this since the token has not expired" },
                        Success = false
                    };
                }

                // Check the token we got if its saved in the db
                var storedRefreshToken = await _apiDbContext.RefreshToken.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

                if (storedRefreshToken == null)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "refresh token doesnt exist" },
                        Success = false
                    };
                }

                // Check the date of the saved token if it has expired
                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has expired, user needs to relogin" },
                        Success = false
                    };
                }

                // check if the refresh token has been used
                if (storedRefreshToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has been used" },
                        Success = false
                    };
                }

                // Check if the token is revoked
                if (storedRefreshToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has been revoked" },
                        Success = false
                    };
                }

                // we are getting here the jwt token id
                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                // check the id that the recieved token has against the id saved in the db
                if (storedRefreshToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "the token doenst mateched the saved token" },
                        Success = false
                    };
                }

                storedRefreshToken.IsUsed = true;
                _apiDbContext.RefreshToken.Update(storedRefreshToken);
                await _apiDbContext.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch


            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }


    }
}
