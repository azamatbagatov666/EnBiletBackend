using System.ComponentModel.DataAnnotations;

namespace EnBiletBackend.Models.Authentication
{
    public class RegisterRequest
    {
        [MaxLength(128)]

        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
