using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace infrastructure.Auth
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddCustomJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var authSettings = configuration.GetSection(nameof(AuthConfig));
            services.Configure<AuthConfig>(authSettings);


            var signingKey =
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authSettings[nameof(AuthConfig.SecretKey)]));


            var jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerConfig));
            services.Configure<JwtIssuerConfig>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerConfig.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerConfig.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });


            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerConfig.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerConfig.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,

                // Allows for skew of one minute for token lifespan validation
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerConfig.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;

                configureOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }

                        return Task.CompletedTask;
                    }
                };
            });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("Author", builder => builder.RequireClaim("ArticleWriter"));
                options.AddPolicy("User", builder => builder.RequireClaim("UserId"));
            });

            return services;
        }
    }
}