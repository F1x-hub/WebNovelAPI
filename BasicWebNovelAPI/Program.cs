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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();
