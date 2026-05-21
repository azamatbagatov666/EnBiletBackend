using System.ComponentModel.DataAnnotations;

namespace EnBiletBackend.Models.Authentication
{
    public class AuthenticateRequest
    {
        [MaxLength(128)]

        public string? Username { get; set; }

        public string? Password { get; set; }
    
    }
}
