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
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);






builder.Services.AddProjectServices(builder.Configuration);
builder.Services.AddSignalR();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Basic Web Novel API",
        Version = "v1",
        Description = "API for managing web novels, chapters, comments, and user libraries",
        Contact = new OpenApiContact { Name = "API Support", Email = "support@webnovelapi.com" }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});


builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);
builder.Logging.AddFilter("BasicWebNovelAPI.Controllers.AuthController", LogLevel.Debug);
builder.Logging.AddFilter("BasicWebNovelAPI.Service.Implementations.AuthorizationRepository", LogLevel.Debug);



var app = builder.Build();


app.UseForwardedHeaders();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configure static files for PDF access
string pdfPath = Path.Combine(app.Environment.ContentRootPath, "PdfFiles");
if (!Directory.Exists(pdfPath))
{
    Directory.CreateDirectory(pdfPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(pdfPath),
    RequestPath = "/pdf-files"
});

app.UseCookiePolicy();
app.UseSession();

app.UseRouting();

// CORS
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

// Exception middleware
app.UseMiddleware<ExceptionMiddleware>();


app.MapHub<CommentHub>("/commentHub");
app.MapControllers();
app.MapGet("/health", () => Results.Ok());

app.Run();
