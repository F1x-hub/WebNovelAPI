using BasicWebNovelAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.Implementations;
using FluentValidation.AspNetCore;
using System.Reflection;
using BasicWebNovelAPI.Middleware;
using BasicWebNovelAPI.Extensions.ServiceExtensions;
using BasicWebNovelAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProjectServices(builder.Configuration);

builder.Services.AddSignalR();

// Configure Swagger with XML documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Basic Web Novel API", 
        Version = "v1",
        Description = "API for managing web novels, chapters, comments, and user libraries",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@webnovelapi.com"
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    // Add examples
    c.ExampleFilters();
    
    
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Add cookie policy to address SameSite issues
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This allows cookies to be sent in cross-site requests (important for OAuth)
    options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    // During development, match the scheme of your application
    options.Secure = builder.Environment.IsDevelopment() 
        ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest 
        : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
});

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Match the secure policy with the cookie policy for consistency
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// Add logger configuration to show detailed logs for Auth components
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);
builder.Logging.AddFilter("BasicWebNovelAPI.Controllers.AuthController", LogLevel.Debug);
builder.Logging.AddFilter("BasicWebNovelAPI.Service.Implementations.AuthorizationRepository", LogLevel.Debug);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: Middleware order matters for authentication
// Apply cookie policy and session before any authentication middleware
app.UseCookiePolicy();
app.UseSession();

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

// Authentication should come before Authorization but after cookie policy
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<CommentHub>("/commentHub");

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();
