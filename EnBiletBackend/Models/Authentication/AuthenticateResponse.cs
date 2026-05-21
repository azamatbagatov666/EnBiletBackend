using System.ComponentModel.DataAnnotations;

namespace EnBiletBackend.Models.Authentication
{
    public class AuthenticateResponse
    {
        public Guid Id { get; set; }
        [MaxLength(128)]

        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public AuthenticateResponse(User user, string accessToken, string refreshToken)
        {
            Id = user.Id;
            Username = user.Username;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }

}
