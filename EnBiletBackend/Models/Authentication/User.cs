using System.ComponentModel.DataAnnotations;

namespace EnBiletBackend.Models.Authentication
{
    public class User
    {
        public Guid Id { get; set; }
        [MaxLength(128)]

        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }

}
