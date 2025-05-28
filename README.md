# WebNovelAPI 📚

![.NET Core](https://img.shields.io/badge/.NET-8.0-512BD4) ![License](https://img.shields.io/badge/license-MIT-blue) ![Status](https://img.shields.io/badge/status-development-yellow)

## 📋 Table of Contents
- [Project Overview](#-project-overview)
- [Installation and Setup](#-installation-and-setup)
- [Configuration](#-configuration)
- [API Endpoints](#-api-endpoints)
- [Architecture and Modules](#-architecture-and-modules)
- [Migrations and Database](#-migrations-and-database)
- [Testing](#-testing)
- [Best Practices and Tips](#-best-practices-and-tips)
- [Documentation and Resources](#-documentation-and-resources)
- [Author and License](#-author-and-license)

[📄 Download PDF Document](https://drive.google.com/file/d/1-WhWNPPKVOwyOu75m3EblFrvEQEPOhDt/view?usp=drive_link)

## 🎯 Project Overview

WebNovelAPI is a powerful RESTful API for managing web novels, providing authors with the ability to publish their works and readers to discover, read, and track interesting content.

### Key Features

- 📚 Novel creation, management, and publication
- 📖 Chapter content management
- 👤 User authentication and authorization
- 🏷️ Genre categorization
- 📑 Reading progress tracking
- 📊 Viewing statistics
- 💬 Comments and ratings
- 🔞 Adult content filtering
- 🔍 Search and recommendations

### Technology Stack

- **Backend**: C# 10, ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server 2016+
- **Caching**: Redis
- **File Storage**: AWS S3
- **Authorization**: JWT tokens
- **API Documentation**: Swagger/OpenAPI
- **Logging**: Serilog

## 🚀 Installation and Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [SQL Server](https://www.microsoft.com/sql-server/) (2016+)
- [Redis](https://redis.io/download) (for distributed caching)
- [AWS account](https://aws.amazon.com/) (for S3 image storage)
- SMTP server (for email functionality)

### Clone and Install

```bash
# Clone the repository
git clone https://github.com/yourusername/WebNovelAPI.git
cd WebNovelAPI

# Restore NuGet packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Build the project
dotnet build
```

### Run the Application

```bash
# Run in development mode
dotnet run --project WebNovelAPI

# Run with specific configuration
dotnet run --project WebNovelAPI --configuration Release
```

The API will be available at `https://localhost:7153` (or another port specified in the configuration).

### Docker Deployment

```bash
# Build Docker image
docker build -t webnovelapi .

# Run container
docker run -d -p 8080:80 --name webnovelapi webnovelapi
```

## 🛠 Configuration

### Database Connection String

Database connection settings and other sensitive information are stored in `appsettings.json`. For production environments, use environment variables or secure secret storage.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=your-db;User ID=your-user;Password=your-password;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

### Redis Configuration

```json
{
  "Redis": {
    "Configuration": "your-redis-host:port",
    "InstanceName": "webnovel-redis"
  }
}
```

### JWT Authentication

```json
{
  "Jwt": {
    "Key": "your-secret-key-at-least-16-characters-long",
    "Audience": "your-audience",
    "Issuer": "your-issuer",
    "ExpirationMinutes": 60
  }
}
```

### AWS S3 Configuration

```json
{
  "AWS": {
    "Profile": "user-image",
    "ImageBucketName": "user-image-bucket",
    "NovelBucketName": "novel-image-bucket",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "Region": "your-region"
  }
}
```

### Environment Variables

For production deployments, we recommend using environment variables:

```
CONNECTIONSTRINGS__DEFAULTCONNECTION=your-connection-string
REDIS__CONFIGURATION=your-redis-connection
JWT__KEY=your-jwt-key
AWS__ACCESSKEY=your-aws-access-key
AWS__SECRETKEY=your-aws-secret-key
```

### Sample appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WebNovelDB;User ID=sa;Password=Password123!;Encrypt=True;TrustServerCertificate=True;"
  },
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "webnovel-dev"
  },
  "Jwt": {
    "Key": "development-secret-key-change-in-production",
    "Audience": "https://localhost:7153",
    "Issuer": "https://localhost:7153",
    "ExpirationMinutes": 60
  }
}
```

## 📡 API Endpoints

### Authentication

Most endpoints require JWT authentication. After logging in, include the token in the Authorization header.

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Users

#### User Registration

```
POST /api/User/register
```

Request:
```json
{
  "username": "newuser",
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}
```

Response (200 OK):
```json
{
  "id": 1,
  "username": "newuser",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### User Login

```
POST /api/User/login
```

Request:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

Response (200 OK):
```json
{
  "id": 1,
  "username": "newuser",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Novels

#### Get All Novels

```
GET /api/Novel/get-all-novels?pageNumber=1&pageSize=10&genreId=1&status=InProgress&sortBy=popular
```

Response (200 OK):
```json
{
  "items": [
    {
      "id": 1,
      "title": "Sample Novel",
      "description": "A captivating story...",
      "views": 1250,
      "status": "InProgress",
      "isAdultContent": false,
      "totalChapters": 15,
      "averageRating": 4.5,
      "genres": ["Fantasy", "Adventure"]
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5
}
```

#### Create Novel

```
POST /api/Novel/create-novel/{userId}
Authorization: Bearer {token}
```

Request:
```json
{
  "title": "My New Novel",
  "description": "An exciting story...",
  "status": "InProgress",
  "isAdultContent": false,
  "genreIds": [1, 3]
}
```

Response (201 Created):
```json
{
  "id": 10,
  "title": "My New Novel",
  "description": "An exciting story...",
  "status": "InProgress",
  "isAdultContent": false,
  "genres": ["Fantasy", "Sci-Fi"]
}
```

#### Get Novel by ID

```
GET /api/Novel/{id}
```

Response (200 OK):
```json
{
  "id": 1,
  "title": "Sample Novel",
  "description": "A captivating story...",
  "views": 1250,
  "status": "InProgress",
  "isAdultContent": false,
  "totalChapters": 15,
  "averageRating": 4.5,
  "authorId": 5,
  "authorName": "AuthorUsername",
  "genres": ["Fantasy", "Adventure"],
  "coverImageUrl": "https://s3.amazonaws.com/novel-bucket/covers/novel-1.jpg"
}
```

### User Library

#### Get User Library

```
GET /api/UserLibrary/user-library/{userId}
Authorization: Bearer {token}
```

Response (200 OK):
```json
[
  {
    "id": 1,
    "novelId": 5,
    "novelTitle": "Sample Novel",
    "lastReadChapter": 7,
    "totalChapters": 15
  }
]
```

#### Add Novel to Library

```
POST /api/UserLibrary/add-to-library/{userId}/{novelId}
Authorization: Bearer {token}
```

Request body: `10` (representing the last read chapter number)

Response (200 OK):
```json
{
  "id": 2,
  "novelId": 7,
  "novelTitle": "New Novel Title",
  "lastReadChapter": 10,
  "totalChapters": 25
}
```

### Status Codes and Errors

| Code | Description |
|-----|----------|
| 200 | OK - Request successful |
| 201 | Created - Resource successfully created |
| 204 | No Content - Successful request with no response body |
| 400 | Bad Request - Invalid request |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Access denied |
| 404 | Not Found - Resource not found |
| 422 | Unprocessable Entity - Validation error |
| 500 | Internal Server Error - Server error |

Error response example:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "traceId": "00-f528fe2f1e9f4eb38e9cad8c733a3a98-00f3421b5f594348-00",
  "errors": {
    "Email": ["The Email field is required."],
    "Password": ["The Password must be at least 8 characters long."]
  }
}
```

## 🏗 Architecture and Modules

### Project Structure

```
WebNovelAPI/
├── Controllers/          # API endpoints
├── Data/                 # DB context and configuration
├── Enum/                 # Enumerations
├── Exceptions/           # Custom exceptions
├── Extensions/           # Extension methods
│   └── ServiceExtensions/# DI extensions
├── Helper/               # Utility helpers
├── Hubs/                 # SignalR hubs
├── Middleware/           # Middleware
├── Migrations/           # EF Core migrations
├── Model/                # Domain models and DTOs
│   ├── Dto/              # Data Transfer Objects
│   ├── Errors/           # Error models
│   ├── Novels/           # Novel models
│   └── UserManagement/   # User models
├── Service/              # Business logic
│   ├── Abstractions/     # Service interfaces
│   ├── BackgroundServices/# Background services
│   └── Implementations/  # Service implementations
└── Validation/           # Validation rules
```

### Design Principles

The API is designed using modern architectural patterns and principles:

- **Clean Architecture**: Separation into layers with clear boundaries of responsibility
- **Repository Pattern**: Separating data access logic from business logic
- **Dependency Injection**: Injecting dependencies for loose coupling
- **DTO Pattern**: For data transfer between layers
- **Unit of Work**: For transaction management
- **CQRS-inspired**: Separate models for reading and writing

### SOLID Principles

- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Classes are open for extension but closed for modification
- **Liskov Substitution**: Subtypes are substitutable for their base types
- **Interface Segregation**: Clients don't depend on interfaces they don't use
- **Dependency Inversion**: High-level modules don't depend on low-level modules

## 🔧 Migrations and Database

### Creating a New Migration

```bash
# Create migration
dotnet ef migrations add MigrationName --project WebNovelAPI

# Generate SQL script for migration
dotnet ef migrations script --project WebNovelAPI
```

### Updating the Database

```bash
# Apply latest migration
dotnet ef database update --project WebNovelAPI

# Rollback to specific migration
dotnet ef database update MigrationName --project WebNovelAPI
```

### Sample Seed Data

```csharp
public static class SeedData
{
    public static void Initialize(WebNovelDbContext context)
    {
        // Check if genres exist
        if (!context.Genres.Any())
        {
            context.Genres.AddRange(
                new Genre { Name = "Fantasy", Description = "Genre based on magic and supernatural phenomena" },
                new Genre { Name = "Science Fiction", Description = "Genre based on scientific achievements, new technologies, and their impact on society" },
                new Genre { Name = "Romance", Description = "Genre focusing on romantic relationships between characters" },
                new Genre { Name = "Action", Description = "Genre with emphasis on action and conflict" },
                new Genre { Name = "Horror", Description = "Genre that evokes fear and anxiety" }
            );
            
            context.SaveChanges();
        }

        // Add test users
        if (!context.Users.Any())
        {
            // User creation logic...
        }
    }
}
```

## ✅ Testing

### Unit Tests

```bash
# Run all tests
dotnet test

# Run tests with specific filter
dotnet test --filter "Category=UnitTest"

# Generate code coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

### Unit Test Example

```csharp
[Fact]
public async Task GetUserLibrary_ReturnsLibraryItems()
{
    // Arrange
    var userId = 1;
    var mockRepository = new Mock<IUserLibraryRepository>();
    mockRepository.Setup(repo => repo.GetUserLibraryAsync(userId))
        .ReturnsAsync(GetTestLibraryItems());
    
    var controller = new UserLibraryController(mockRepository.Object);
    
    // Act
    var result = await controller.GetUserLibrary(userId);
    
    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var libraryItems = Assert.IsAssignableFrom<List<GetUserLibraryDto>>(okResult.Value);
    Assert.Equal(2, libraryItems.Count);
}

private List<GetUserLibraryDto> GetTestLibraryItems()
{
    return new List<GetUserLibraryDto>
    {
        new GetUserLibraryDto { Id = 1, NovelId = 5, NovelTitle = "Test Novel 1", LastReadChapter = 3, TotalChapters = 10 },
        new GetUserLibraryDto { Id = 2, NovelId = 8, NovelTitle = "Test Novel 2", LastReadChapter = 5, TotalChapters = 15 }
    };
}
```

### Integration Tests

```csharp
public class NovelControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public NovelControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Configure test services...
            });
        });
    }
    
    [Fact]
    public async Task GetAllNovels_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/Novel/get-all-novels");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }
}
```

## 📚 Best Practices and Tips

### Logging

The project uses Serilog for structured logging. Logging is configured in `Program.cs`:

```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/webnovelapi.log", rollingInterval: RollingInterval.Day));
```

### Exception Handling

The project uses a global exception handler middleware:

```csharp
app.UseMiddleware<ExceptionMiddleware>();
```

Implementation example:

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        context.Response.StatusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
        
        var response = new ErrorResponse
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message,
            // Don't expose stack details in production!
            StackTrace = Debugger.IsAttached ? exception.StackTrace : null
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### API Versioning

The project supports API versioning via URL path:

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

Controller example with versioning:

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class NovelController : ControllerBase
{
    // ...
}
```

### API Documentation (Swagger)

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Web Novel API",
        Version = "v1",
        Description = "API for web novel platform",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com",
            Url = new Uri("https://yourwebsite.com")
        }
    });
    
    // JWT auth configuration in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

## 📖 Documentation and Resources

### Official Documentation

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [.NET 8.0 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/8.0)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Authentication in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Redis Caching in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/)

### Useful Articles and Tutorials

- [REST API Best Practices](https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [Secure an ASP.NET Core Web API using JWT Authentication](https://medium.com/swlh/secure-an-asp-net-core-web-api-using-jwt-authentication-1c9273a45b71)
- [CQRS and MediatR in ASP.NET Core](https://code-maze.com/cqrs-mediatr-in-aspnet-core/)
- [Fluent Validation in ASP.NET Core](https://fluentvalidation.net/aspnet)
- [Setting up Swagger for ASP.NET Core Web API](https://medium.com/swlh/asp-net-core-3-0-web-api-documentation-with-swagger-ui-f422ddc641cd)

## ✍️ Author and License

### Author

**Fix**
- Email: iraklilagvilava975@gmail.com
- GitHub: [github.com/F1x-hub](https://github.com/F1x-hub)
- LinkedIn: [linkedin.com/in/yourprofile](https://linkedin.com/in/yourprofile)

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

[![Made with ❤️](https://img.shields.io/badge/Made%20with-%E2%9D%A4%EF%B8%8F-red)](https://github.com/yourusername/WebNovelAPI)
[![Support Project](https://img.shields.io/badge/Support-Project-blue)](https://github.com/yourusername/WebNovelAPI/issues) 
