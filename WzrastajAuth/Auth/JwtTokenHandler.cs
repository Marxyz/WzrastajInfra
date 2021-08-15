using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace WzrastajAuth.Auth
{
    public interface IJwtTokenHandler
    {
        string WriteToken(JwtSecurityToken jwt);
        ClaimsPrincipal ValidateToken(string token, TokenValidationParameters tokenValidationParameters);
    }
    public sealed class JwtTokenHandler : IJwtTokenHandler
    {
        private readonly JwtSecurityTokenHandler JwtSecurityTokenHandler;

        public JwtTokenHandler()
        {
            JwtSecurityTokenHandler ??= new JwtSecurityTokenHandler();
        }

        public string WriteToken(JwtSecurityToken jwt)
        {
            return JwtSecurityTokenHandler.WriteToken(jwt);
        }

        public ClaimsPrincipal ValidateToken(string token, TokenValidationParameters tokenValidationParameters)
        {
            try
            {
                var principal =
                    JwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}