using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace infrastructure.Auth
{
    public interface IJwtFactory
    {
        Task<AccessToken> GenerateEncodedToken(string id, string userName);
    }

    internal sealed class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerConfig JwtConfig;
        private readonly IJwtTokenHandler JwtTokenHandler;

        public JwtFactory(IJwtTokenHandler jwtTokenHandler, IOptions<JwtIssuerConfig> jwtOptions)
        {
            JwtTokenHandler = jwtTokenHandler;
            JwtConfig = jwtOptions.Value;
            ThrowIfInvalidOptions(JwtConfig);
        }

        public async Task<AccessToken> GenerateEncodedToken(string id, string userName)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, await JwtConfig.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(JwtConfig.IssuedAt).ToString(),
                    ClaimValueTypes.Integer64),
                new Claim("UserId", id, ClaimValueTypes.String),
                new Claim("ArticleWriter", true.ToString(), ClaimValueTypes.Boolean)
            };

            // CreateAsync the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                JwtConfig.Issuer,
                JwtConfig.Audience,
                claims,
                JwtConfig.NotBefore,
                JwtConfig.Expiration,
                JwtConfig.SigningCredentials);

            return new AccessToken(JwtTokenHandler.WriteToken(jwt), (int)JwtConfig.ValidFor.TotalSeconds);
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
        {
            return (long)Math.Round((date.ToUniversalTime() -
                                     new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .TotalSeconds);
        }

        private static void ThrowIfInvalidOptions(JwtIssuerConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerConfig.ValidFor));
            }

            if (config.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerConfig.SigningCredentials));
            }

            if (config.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerConfig.JtiGenerator));
            }
        }
    }
}