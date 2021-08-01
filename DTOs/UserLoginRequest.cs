using System.ComponentModel.DataAnnotations;

namespace webapi_identity.DTOs
{
    public class UserLoginRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}