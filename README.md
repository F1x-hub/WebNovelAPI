# BasicWebNovelAPI

A robust .NET Core Web API for managing web novels with user authentication, library management, and content delivery features.

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [API Usage](#api-usage)
- [Project Structure](#project-structure)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)
- [Additional Resources](#additional-resources)

## Overview

BasicWebNovelAPI is a feature-rich web novel platform API that enables authors to publish their works and readers to discover, read, and track novels. The API supports user authentication, content management, and personalized user libraries.

## Features

- 📚 Novel creation, management, and publication
- 📖 Chapter content management
- 👤 User authentication and authorization
- 🏷️ Genre categorization
- 📑 User library tracking (reading progress)
- 📊 Novel viewing statistics
- 💬 Comments and ratings
- 🔞 Adult content filtering
- 🔍 Search and discovery

## Prerequisites

- .NET 8.0 SDK or higher
- SQL Server (2016+)
- Redis (for distributed caching)
- AWS account (for S3 image storage)
- SMTP server access (for email functionality)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/BasicWebNovelAPI.git
   cd BasicWebNovelAPI
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Update the database with migrations:
   ```bash
   dotnet ef database update
   ```

4. Build the project:
   ```bash
   dotnet build
   ```

5. Run the API:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7153` (or a different port as configured).

## Configuration

### Database Connection

Database connection strings and other sensitive information are stored in `appsettings.json`. For production, use environment variables or secret management.

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
    "Key": "your-secret-key",
    "Audience": "your-audience",
    "Issuer": "your-issuer"
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

For production deployments, consider using environment variables:

```
CONNECTIONSTRINGS__DEFAULTCONNECTION=your-connection-string
REDIS__CONFIGURATION=your-redis-connection
JWT__KEY=your-jwt-key
AWS__ACCESSKEY=your-aws-access-key
AWS__SECRETKEY=your-aws-secret-key
```

## API Usage

### Authentication

Most endpoints require JWT authentication. After login, include the token in the Authorization header.

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Novel Endpoints

#### Get All Novels

```
GET /api/Novel/get-all-novels?pageNumber=1&pageSize=10&genreId=1&status=InProgress&sortBy=popular
```

Response:
```json
[
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
]
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

### User Library Endpoints

#### Get User Library

```
GET /api/UserLibrary/user-library/{userId}
Authorization: Bearer {token}
```

Response:
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

#### Add/Remove Novel to/from Library

```
POST /api/UserLibrary/add-to-library/{userId}/{novelId}
Authorization: Bearer {token}
```

Request body: `10` (representing the last read chapter number)

## Project Structure

```
BasicWebNovelAPI/
├── Controllers/         # API endpoints
├── Data/                # Database context and configuration
├── Enum/                # Enumeration types
├── Exceptions/          # Custom exception classes
├── Extensions/          # Extension methods
├── Helper/              # Helper utilities
├── Middleware/          # Request/response middleware
├── Model/               # Domain models and DTOs
├── Service/             # Business logic implementation
│   ├── Abstractions/    # Interfaces
│   ├── Implementations/ # Concrete implementations
├── Validation/          # Request validation rules
└── Program.cs           # Application entry point
```

### Architecture Patterns

- **Repository Pattern**: Separates data access logic from business logic
- **Dependency Injection**: Used throughout the project for loose coupling
- **DTO Pattern**: For data transfer between layers
- **Unit of Work**: For transaction management
- **CQRS-inspired**: Separate models for reads and writes

## Testing

1. Run unit tests:
   ```bash
   dotnet test
   ```

Example test:
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
```

## Contributing

1. Fork the repository
2. Create your feature branch: `git checkout -b feature/my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin feature/my-new-feature`
5. Submit a pull request

### Issue Guidelines

When creating an issue, please include:
- Clear description of the problem
- Steps to reproduce
- Expected behavior
- Screenshots if applicable
- Environment details

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Authentication in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Redis Caching in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/) 
