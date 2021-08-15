using System;
using Data.BlobStorage;
using Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddDatabaseDependencies(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            return services;
        }

        private static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
        {
            var identityBuilder = services.AddIdentityCore<IdentityUser>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 8;
                o.Lockout.MaxFailedAccessAttempts = 4;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(8);
                o.SignIn.RequireConfirmedEmail = false;
                o.User.RequireUniqueEmail = true;
            });

            identityBuilder =
                new IdentityBuilder(identityBuilder.UserType, typeof(IdentityRole), identityBuilder.Services);
            identityBuilder.AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();
            identityBuilder.AddRoleValidator<RoleValidator<IdentityRole>>();
            identityBuilder.AddRoleManager<RoleManager<IdentityRole>>();
            identityBuilder.AddSignInManager<SignInManager<IdentityUser>>();

            return services;
        }


        private static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddTransient<IContentStorageService, ContentStorageService>();
            return services;
        }


        public static IServiceCollection AddData(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            return serviceCollection.AddIdentityConfiguration().AddDatabaseDependencies(configuration)
                .AddDataServices();
        }
    }
}