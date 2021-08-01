using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using webapi_identity.DTOs;

namespace webapi_identity.Repositories
{
    public interface IAccountRepository
    {
        Task<Object> CreateUser(UserRegistrationRequestDto userRegisterDto);
        Task<IdentityUser> FindByEmailAsync(string email);
        string GenerateJwtToken(IdentityUser user);

        Task<bool> UserLogin(IdentityUser identityUser, string password);
    }
}