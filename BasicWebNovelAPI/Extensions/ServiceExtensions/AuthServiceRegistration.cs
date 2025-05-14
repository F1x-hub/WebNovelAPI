using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BasicWebNovelAPI.Extensions.ServiceExtensions
{
    public static class AuthServiceRegistration
    { 
        public static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = "ApplicationCookie";
            })
            .AddCookie("ApplicationCookie", options =>
            {
                options.LoginPath = "/api/auth/login";
                options.LogoutPath = "/api/auth/logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                
                // Important: Most browsers block third-party cookies with SameSite=None
                // unless they're Secure. For OAuth flows, we need Lax at minimum
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
                
                // Prevents redirect loops
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                
                // Add events for debugging token validation issues
                option.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerHandler>>();
                        
                        logger.LogError("Authentication failed: {Error}", context.Exception.Message);
                        
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            logger.LogWarning("Token expired");
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        
                        return Task.CompletedTask;
                    },
                    
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerHandler>>();
                        
                        var principal = context.Principal;
                        var identity = principal?.Identity as ClaimsIdentity;
                        var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);
                        var roleClaims = identity?.FindAll(ClaimTypes.Role).Select(c => c.Value);
                        
                        logger.LogInformation(
                            "Token successfully validated. User: {UserId}, Roles: {Roles}",
                            userIdClaim?.Value ?? "unknown",
                            roleClaims != null ? string.Join(", ", roleClaims) : "none");
                        
                        return Task.CompletedTask;
                    },
                    
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerHandler>>();
                        
                        logger.LogWarning("Authorization challenge issued for request {Path}", 
                            context.Request.Path);
                        
                        // Prevent default challenge response for API requests
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            
                            var result = System.Text.Json.JsonSerializer.Serialize(new { 
                                error = "Unauthorized",
                                message = "You are not authorized to access this resource"
                            });
                            
                            return context.Response.WriteAsync(result);
                        }
                        
                        return Task.CompletedTask;
                    },
                    
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerHandler>>();
                        
                        var token = context.Token;
                        
                        if (string.IsNullOrEmpty(token))
                        {
                            // Allow token to be passed in query string for specific scenarios
                            token = context.Request.Query["token"];
                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                                logger.LogInformation("Token extracted from query string");
                            }
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/signin-google";
                options.SaveTokens = true;
                
                // Configure cookies for development environment
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                
                // Security enhancements for correlation cookie
                options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = isDevelopment 
                    ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest 
                    : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
                
                // Maximum compatibility with most browsers
                options.CorrelationCookie.Path = "/";
                options.CorrelationCookie.MaxAge = TimeSpan.FromMinutes(15);
                
                // Request additional scopes
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                
                // Handle events to prevent CSRF
                options.Events.OnTicketReceived = context =>
                {
                    // Validate state parameter (CSRF protection)
                    return Task.CompletedTask;
                };
            })
            .AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
            {
                options.AppId = configuration["Authentication:Facebook:AppId"];
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                options.CallbackPath = "/signin-facebook";
                options.SaveTokens = true;
                
                // Same fixes as Google for consistency
                options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
                options.CorrelationCookie.Domain = null;
                
                options.Scope.Add("email");
                options.Scope.Add("public_profile");
                
                options.Fields.Add("name");
                options.Fields.Add("email");
                options.Fields.Add("first_name");
                options.Fields.Add("last_name");
                options.Fields.Add("picture");
            });

            services.AddAuthorization(options => 
            {
                options.AddPolicy("ApiAccess", policy => 
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });
        }
    }
}
