using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using webapi_identity.Domains;
using webapi_identity.DTOs;

namespace webapi_identity.Repositories
{
    public interface IAccountRepository
    {
        Task<IdentityUser> CreateUser(UserRegistrationRequestDto userRegisterDto);
        Task<IdentityUser> FindByEmailAsync(string email);
        Task<AuthResult> GenerateJwtToken(IdentityUser user);

        Task<AuthResult> UserLogin(UserLoginRequest user);

        Task<AuthResult> VerifyToken(TokenRequest tokenRequest);
    }
}