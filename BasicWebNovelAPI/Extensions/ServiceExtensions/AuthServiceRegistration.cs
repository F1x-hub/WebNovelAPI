using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BasicWebNovelAPI.Extensions.ServiceExtensions
{
    public static class AuthServiceRegistration
    { 
        public static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(option =>
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
                   });

            services.AddAuthorization();

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = FacebookDefaults.AuthenticationScheme;
            })
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            })
            .AddFacebook(options =>
            {
                options.AppId = configuration["Authentication:Facebook:AppId"];
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
            });
        }
    }
}
