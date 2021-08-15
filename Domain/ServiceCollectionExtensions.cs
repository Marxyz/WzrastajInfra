using Data;
using Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddData(configuration);

            services.AddTransient<IArticleDataProvider, ArticleDataService>();
            services.AddTransient<IArticleDataWriter, ArticleDataService>();

            services.AddTransient<IAuthorizationDataHandler, AuthorDataService>();
            services.AddTransient<IAuthorDataWriter, AuthorDataService>();
            services.AddTransient<IAuthorDataProvider, AuthorDataService>();

            services.AddTransient<ITokenFactory, TokenFactory>();
            services.AddTransient<IStoragePathResolver, StoragePathResolver>();

            services.AddTransient<IAuthorDataService, AuthorDataService>();
            services.AddTransient<IArticleDataService, ArticleDataService>();

            services.AddTransient<ICategoryDataService, CategoryDataService>();
            services.AddTransient<ICategoryDataProvider, CategoryDataService>();
            services.AddTransient<ICategoryDataWriter, CategoryDataService>();


            return services;
        }
    }
}