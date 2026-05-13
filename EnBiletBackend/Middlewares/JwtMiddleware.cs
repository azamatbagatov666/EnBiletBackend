using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EnBiletBackend.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IConfiguration _config;

        public JwtMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }


        public async Task Invoke(HttpContext context, Services.AuthenticationService authService)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..];
                AttachUserToContext(context, authService, token);
            }

            await _next(context);
        }

        private void AttachUserToContext(
    HttpContext context,
    Services.AuthenticationService authService,
    string token
)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwt = (JwtSecurityToken)validatedToken;

                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid");
                if (userIdClaim == null) return;

                if (!Guid.TryParse(userIdClaim.Value, out var userId)) return;

                context.Items["User"] = authService.GetUser(userId);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("JWT FAIL: " + ex.Message);
            }
        }
    }
}
