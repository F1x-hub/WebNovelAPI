using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Validation;
using FluentValidation;
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
            services.AddControllers();
            
            // Updated FluentValidation configuration
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<UserValidator>();

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
            var redisConfig = configuration["Redis:Configuration"];
            var redisInstance = configuration["Redis:InstanceName"];

            if (!string.IsNullOrEmpty(redisConfig) && !string.IsNullOrEmpty(redisInstance))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConfig;
                    options.InstanceName = redisInstance;
                    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
                    {
                        AbortOnConnectFail = false,
                        EndPoints = { redisConfig },
                        ConnectRetry = 1,
                        ConnectTimeout = 1000
                    };
                });
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

            // Register dependency services including INovelRepository
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
                            "http://localhost:5173",
                            "https://www.webnovel-project.click",
                            "https://webnovel-project.click",
                            "https://api.webnovel-project.click",
                            "https://api.www.webnovel-project.click"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

        }
    }
}
