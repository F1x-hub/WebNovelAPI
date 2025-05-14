using BasicWebNovelAPI.Data;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace BasicWebNovelAPI.Extensions.ServiceExtensions
{
    public static class ServiceRegistration
    {
        public static void AddProjectServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddFluentValidation(v =>
                {
                    v.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                });

            // Add session state configuration
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Configure cookie policy options
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            // Make Redis optional
            if (!string.IsNullOrEmpty(configuration["Redis:Configuration"]) && !string.IsNullOrEmpty(configuration["Redis:InstanceName"]))
            {
                try
                {
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = configuration["Redis:Configuration"];
                        options.InstanceName = configuration["Redis:InstanceName"];
                    });
                }
                catch (Exception)
                {
                    // Fall back to memory cache if Redis is not available
                    services.AddDistributedMemoryCache();
                }
            }
            else
            {
                // Use in-memory cache if Redis is not configured
                services.AddDistributedMemoryCache();
            }

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(option =>
            {
                option.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
                {
                    Description = "enter authorization token using bearer scheme (bearer {token})",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                option.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            services.AddAutoMapper(typeof(Program).Assembly);


            services.AddDbContext<BasicWebNovelContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddDependencyServices(configuration);

            services.AddAuthServices(configuration);


            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:4200",
                            "https://localhost:4200", 
                            "http://localhost:7153", 
                            "https://localhost:7153",
                            "http://localhost:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

        }
    }
}
