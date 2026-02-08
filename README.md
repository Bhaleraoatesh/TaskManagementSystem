# Task Management System

An enterprise-grade REST API for comprehensive task management with JWT-based authentication, role-based access control, and secure token management. Built with .NET 8.0 following Clean Architecture principles.

## 🌟 Features

### Authentication & Security
- **JWT Bearer Authentication** - Secure token-based authentication with configurable expiration
- **Refresh Token Mechanism** - Automatic token renewal with separate refresh tokens
- **Role-Based Access Control** - Admin and Manager role support with authorization policies
- **Token Management** - Automatic cleanup of expired tokens via background service
- **Security Headers** - CORS protection, XSS prevention, clickjacking protection
- **HTTPS Enforcement** - Secure communication enforced in production
- **HttpOnly Cookies** - Secure refresh token storage with SameSite policy

### Task Management
- **Task Assignment** - Assign tasks to users with tracking metadata
- **Task Retrieval** - Get all tasks assigned to a specific user with full details
- **Task Details** - Comprehensive task information including:
  - Task ID, Title, Description
  - Category and Priority levels
  - Status tracking and timestamps
  - User assignment information

### Developer Experience
- **API Documentation** - Interactive Swagger UI for API exploration and testing
- **Structured Logging** - Comprehensive logging with Serilog (Console, File, Elasticsearch)
- **Health Checks** - Endpoint to verify system health
- **Docker Support** - Multi-stage Dockerfile for containerized deployment

## 🏗️ Architecture

This project follows **Clean Architecture** organized into distinct layers:

```
TaskManagement.API/              # API Layer (Controllers, Middleware, Middleware Configuration)
TaskManagement.Application/      # Business Logic (Services, DTOs, Query Handlers)
TaskManagement.Domain/           # Core Domain (Entities, Repository Interfaces)
TaskManagement.Persistance/      # Data Access (Repositories, Database Configuration)
TaskManagement.Common/           # Shared Utilities (Cross-cutting concerns)
TaskManagement.Test/             # Unit & Integration Tests
```

### Key Technologies

| Component | Technology | Version |
|-----------|-----------|---------|
| **Framework** | .NET / ASP.NET Core | 8.0 |
| **ORM** | Dapper | 2.1.66 |
| **Database** | SQL Server | Latest |
| **Authentication** | JWT Bearer | Microsoft.IdentityModel.Tokens |
| **API Documentation** | Swagger/Swashbuckle | 6.6.2 |
| **Logging** | Serilog | 4.2.0 |
| **CQRS Pattern** | MediatR | 12.4.1 |
| **Validation** | Guard Clauses | Ardalis.GuardClauses 5.0.0 |
| **CI/CD** | GitHub Actions | - |

## 🚀 Getting Started

### Prerequisites

- **.NET 8.0 SDK** or later
- **SQL Server** (Express or Full)
- **Visual Studio 2022** (recommended) or VS Code with C# extensions

### Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Bhaleraoatesh/TaskManagementSystem.git
   cd TaskManagementSystem
   ```

2. **Configure Database Connection**
   - Update the connection string in `TaskManagement.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=TaskManagementDB;Trusted_Connection=true;"
   }
   ```
   - Or set via environment variable: `ConnectionStrings__DefaultConnection`

3. **Configure JWT Settings**
   - Update `appsettings.json` with your JWT configuration:
   ```json
   "JwtSettings": {
     "Key": "your-secret-key-min-32-characters",
     "Issuer": "your-issuer",
     "Audience": "your-audience",
     "AccessTokenExpirationMinutes": 15,
     "RefreshTokenExpirationDays": 7
   }
   ```

4. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

5. **Build the Solution**
   ```bash
   dotnet build
   ```

6. **Run the Application**
   ```bash
   cd TaskManagement.API
   dotnet run
   ```

   The API will be available at `https://localhost:5001` and Swagger UI at `https://localhost:5001/swagger`

## 🔌 API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| POST | `/api/auth/login` | User login with email and password | No |
| POST | `/api/auth/refresh` | Refresh access token | No |
| POST | `/api/auth/logout` | Logout and revoke token | Yes |
| GET | `/api/auth/me` | Get current user information | Yes |

### Task Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| GET | `/api/taskmanagement/{userId}` | Get tasks assigned to user | Yes |

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | System health check |

### Authentication Examples

**Login:**
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "4f12a7c3-2b8f-4d1e-9a5c-7b8e3d2f1a4b",
  "expiresIn": 900,
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  }
}
```

**Get Assigned Tasks:**
```bash
curl -X GET https://localhost:5001/api/taskmanagement/1 \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## 🐳 Docker

### Build Docker Image

```bash
docker build -t task-management:latest .
```

### Run Docker Container

```bash
docker run -d \
  -p 5001:8080 \
  -e ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;Database=TaskManagementDB;" \
  -e JwtSettings__Key="your-secret-key" \
  task-management:latest
```

## 🧪 Testing

Run unit and integration tests:

```bash
dotnet test
```

Run tests with coverage:

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

## 🔒 Security

### Security Features Implemented

- ✅ JWT Bearer token authentication with expiration
- ✅ Refresh token mechanism with separate storage and expiration
- ✅ SQL parameter binding (via Dapper) against SQL injection
- ✅ CORS protection with configurable origins
- ✅ Security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy)
- ✅ HTTPS enforcement in production
- ✅ HttpOnly, Secure, SameSite cookies for refresh tokens
- ✅ Role-based authorization policies
- ✅ IP tracking for audit logging

### CI/CD Security Pipeline

The project includes automated security checks via GitHub Actions:

```yaml
- CodeQL static analysis for C# code
- Gitleaks secret scanning
- Dependency vulnerability scanning
- Docker image scanning (Trivy)
```

## 📝 Configuration

### Development Environment (`appsettings.Development.json`)

- CORS: AllowAll (localhost:3000, 4200, 5173)
- Logging: Console and file output
- Database: Local SQL Server Express

### Production Environment (`appsettings.json`)

- CORS: Restricted to configured origins
- Logging: File with daily rolling and Elasticsearch sink
- HTTPS: Required
- Security Headers: Enabled

### Key Configuration Options

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "JwtSettings": {
    "Key": "your-secret-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;"
  }
}
```

## 📊 Logging

Logging is configured with Serilog and includes:

- **Console Output** - Real-time log viewing in development
- **File Logging** - Daily rolling log files with structured format
- **Elasticsearch Integration** - Enterprise logging and analysis

Log location: `logs/` directory in the application root

## 🤝 Contributing

We welcome contributions from the community! Whether you want to report a bug, suggest a feature, or submit code improvements, please feel free to participate.

### How to Contribute

**For Bugs and Issues:**
1. Check the [GitHub Issues](https://github.com/Bhaleraoatesh/TaskManagementSystem/issues) to see if your issue already exists
2. If not, create a new issue with a clear description and reproduction steps (if applicable)
3. Include your environment details (OS, .NET version, etc.)

**For Pull Requests:**
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes and commit with clear, descriptive messages
4. Push to your fork (`git push origin feature/AmazingFeature`)
5. Open a Pull Request with a detailed description of your changes
6. Link any related issues in the PR description

**For Feature Requests:**
1. Open a GitHub Issue with the title "Feature Request: [Your Idea]"
2. Clearly describe what you'd like to add and why
3. Provide examples if possible

### Code Quality Standards

Before submitting a PR, please ensure:
- Follow C# naming conventions (PascalCase for classes, camelCase for variables)
- Write unit tests for new features or bug fixes
- All existing tests pass: `dotnet test`
- Code follows Clean Architecture principles
- Commit messages are clear and descriptive
- No breaking changes to existing APIs (unless discussed in an issue first)

## 📦 Deployment

### Azure App Service

See `.github/workflows/release-2025.02_taskmanagement.yml` for automated deployment pipeline.

### Local IIS Deployment

1. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Create an IIS application and point to the publish folder
3. Configure application pool for .NET 8.0

## ❓ Troubleshooting

### Connection String Issues
- Verify SQL Server is running
- Check connection string format: `Server=SERVER_NAME;Database=DB_NAME;Trusted_Connection=true;`

### JWT Token Errors
- Ensure JWT secret key is at least 32 characters
- Verify token hasn't expired (check `expiresIn` in response)
- Use refresh token to get a new access token

### CORS Errors
- Check configured allowed origins in `appsettings.json`
- Verify request includes proper Content-Type header
- Enable credentials in CORS policy if needed

## 📄 License

This project is licensed under the Licensing Agreement - see LICENSE file for details.

## 👥 Support & Contact

**Found a bug or have ideas for improvement?**
- Open an issue on [GitHub Issues](https://github.com/Bhaleraoatesh/TaskManagementSystem/issues)
- Submit a pull request with your improvements
- Questions or suggestions? Create a GitHub Discussion

**For security vulnerabilities:**
- Please report responsibly by opening a private issue or contacting the maintainer directly

**For commercial inquiries:**
- See the LICENSE file for contact information regarding commercial use

## 📚 Additional Resources

- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [.NET 8.0 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)

---

**Last Updated:** February 2026
**Current Version:** 1.0.0
**Maintainer:** Atesh Bhalerao
