using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace infrastructure.Auth
{
    public interface IJwtTokenValidator
    {
        ClaimsPrincipal GetPrincipalFromToken(string token, string signingKey);
    }

    public sealed class JwtTokenValidator : IJwtTokenValidator
    {
        private readonly IJwtTokenHandler JwtTokenHandler;

        public JwtTokenValidator(IJwtTokenHandler jwtTokenHandler)
        {
            JwtTokenHandler = jwtTokenHandler;
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token, string signingKey)
        {
            return JwtTokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ValidateLifetime = false // we parses data from expired tokens here
            });
        }
    }
}