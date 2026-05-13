using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnBiletBackend.Connection;
using EnBiletBackend.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;


namespace EnBiletBackend.Services
{
    public class AuthenticationService
    {
        private readonly IDbConnection _connection;
        private readonly IConfiguration _config;

        public AuthenticationService(DapperContext context, IConfiguration config)
        {
            _connection = context.CreateConnection();
            _config = config;
        }



        public AuthenticateResponse Authenticate(AuthenticateRequest request)
        {
            var query = @"SELECT * FROM Users WHERE Username = @username";

            var user = _connection.QueryFirstOrDefault<User>(
                query,
                new { username = request.Username }
            );

            if (user == null)
                throw new Exception("Hatalı kullanıcı adı veya parola.");

            var hasher = new PasswordHasher<User>();

            var result = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Hatalı kullanıcı adı veya parola.");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            SaveRefreshToken(user.Id, refreshToken);

            return new AuthenticateResponse(user, accessToken, refreshToken);

        }

        public void Register(RegisterRequest request)
        {
            // Check if username already exists
            var exists = _connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Users WHERE Username = @username",
                new { username = request.Username }
            );

            if (exists > 0)
                throw new Exception("Username already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username
            };

            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, request.Password);

            var insertQuery = @"
        INSERT INTO Users (Id, Username, PasswordHash)
        VALUES (@Id, @Username, @PasswordHash)
    ";

            _connection.Execute(insertQuery, user);
        }


        public User GetUser(Guid id)
        {
            var query = @"
            Select * from Users where Id = @id";

            var user = _connection.QueryFirstOrDefault<User>(query, new { id });
            if (user == null)
            {
                throw new Exception("Düzgün gir lo");
            }
            return user;
        }

        public AuthenticateResponse Refresh(string refreshToken)
        {
            var token = _connection.QueryFirstOrDefault<RefreshToken>(
                @"SELECT * FROM RefreshTokens
          WHERE Token = @token
            AND ExpiresAt > GETUTCDATE()
            AND (
                RevokedAt IS NULL
                OR RevokedAt > DATEADD(SECOND, -10, GETUTCDATE())
            )",
                new { token = refreshToken }
            );

            if (token == null)
                throw new UnauthorizedAccessException();

            var user = GetUser(token.UserId);

            // revoke only once
            if (token.RevokedAt == null)
            {
                RevokeRefreshToken(token.Id);
            }

            var newRefresh = GenerateRefreshToken();
            SaveRefreshToken(user.Id, newRefresh);

            var newJwt = GenerateJwtToken(user);

            return new AuthenticateResponse(user, newJwt, newRefresh);
        }

        public void Logout(string refreshToken)
        {
            var token = _connection.QueryFirstOrDefault<RefreshToken>(
                @"SELECT * FROM RefreshTokens
          WHERE Token = @token AND RevokedAt IS NULL",
                new { token = refreshToken }
            );

            //Console.WriteLine("logged out: " + refreshToken);

            if (token == null)
                return;

            _connection.Execute(
                @"UPDATE RefreshTokens
          SET RevokedAt = GETUTCDATE()
          WHERE Id = @id",
                new { id = token.Id }
            );
        }



        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var secret = _config["Jwt:Secret"];
            var key = Encoding.UTF8.GetBytes(secret);

            var expiresMinutes = int.Parse(_config["Jwt:AccessTokenMinutes"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        }),
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64)
        );
    }

        private void SaveRefreshToken(Guid userId, string token)
        {
            var days = int.Parse(_config["Jwt:RefreshTokenDays"]);

            _connection.Execute(
                @"INSERT INTO RefreshTokens (Id, UserId, Token, ExpiresAt)
          VALUES (@Id, @UserId, @Token, @ExpiresAt)",
                new
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddDays(days)
                }
            );
        }

        private void RevokeRefreshToken(Guid refreshTokenId)
        {
            _connection.Execute(
                @"UPDATE RefreshTokens
          SET RevokedAt = GETUTCDATE()
          WHERE Id = @id",
                new { id = refreshTokenId }
            );
        }



    }
}
