using System.Net;
using System.Text.Json;
using Data.BlobStorage;
using Domain;
using infrastructure.Auth;
using infrastructure.Helpers;
using infrastructure.Services.ActionHandlers;
using infrastructure.Services.External;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace infrastructure
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json").AddEnvironmentVariables();
            Configuration = builder.Build();
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var authSettings = Configuration.GetSection(nameof(AuthConfig));
            services.Configure<AuthConfig>(authSettings);

            var mailingSettings = Configuration.GetSection(nameof(MailSenderConfig));
            services.Configure<MailSenderConfig>(mailingSettings);


            var storageSettings = Configuration.GetSection(nameof(StorageConfig));
            services.Configure<StorageConfig>(storageSettings);

            services.AddCustomJwt(Configuration);

            services.AddDomain(Configuration);

            services.AddMvcCore()
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.IgnoreNullValues = false;
                });


            services.AddTransient<IAuthActionHandler, AuthActionHandler>();
            services.AddTransient<IUserActionHandler, UserActionHandler>();
            services.AddTransient<IArticlesActionHandler, ArticlesActionHandler>();

            services.AddTransient<IJwtFactory, JwtFactory>();
            services.AddTransient<IJwtTokenValidator, JwtTokenValidator>();
            services.AddTransient<IJwtTokenHandler, JwtTokenHandler>();

            services.AddTransient<IMailService, MailService>();

            services.AddSwaggerGen();


            services.AddControllers();
            //services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(
                builder =>
                {
                    builder.Run(
                        async context =>
                        {
                            context.Response.StatusCode = HttpStatusCode.InternalServerError.AsInt();
                            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                            var error = context.Features.Get<IExceptionHandlerFeature>();
                            if (error != null)
                            {
                                context.Response.AddApplicationError(error.Error.Message);
                                await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                            }
                        });
                });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage(); Requires nuget package
            }


            app.UseHttpsRedirection();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());


            app.UseSwagger(c => { c.SerializeAsV2 = true; });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WzrastajFoundation - infrastructure v1.");
            });


            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}