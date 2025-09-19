# Security Microservice - .NET 9 with OpenIddict

A comprehensive security microservice built with .NET 9 and OpenIddict 4.x that provides complete authentication and authorization capabilities for enterprise applications.

## Architecture

The solution follows a layered architecture pattern:

- **API Layer**: REST endpoints and controllers
- **Application Layer**: Business logic and use cases
- **Domain Layer**: Entities and business rules
- **Infrastructure Layer**: Data access, repositories, and external services
- **Shared Layer**: Common DTOs and contracts

## Features

### Core Functionality
- User management (CRUD operations)
- Application management 
- Role-based access control (RBAC)
- Fine-grained permissions system
- JWT token generation with custom claims
- Password hashing with PBKDF2
- Audit logging
- **Dynamic Menu Generation**: Menu system based on user permissions per application

### Security Features
- OpenIddict 4.x integration for OAuth2/OpenID Connect
- Custom permission-based authorization
- Secure password storage
- Token introspection endpoint
- Refresh token support

### Database Entities
- **Users**: Authentication and basic user info
- **Applications**: Different systems (AUDITORIA, MEMOS, etc.)
- **Modules**: Logical groupings within applications
- **Options**: API endpoints/routes within modules
- **Roles**: Application-specific roles
- **Permissions**: Role + Option + Action combinations
- **Audit**: System activity logging

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB for development)
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository
```bash
git clone https://github.com/cnvillegaschavez/ruway-security-service.git
cd ruway-security-service
```

2. Update connection string in `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SecurityMicroserviceDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

3. Run database migrations
```bash
cd src/SecurityMicroservice.Api
dotnet ef database update
```

4. Start the application
```bash
dotnet run
```

The API will be available at `https://localhost:7001` with Swagger UI at `https://localhost:7001/swagger`

### Default Credentials
- **Username**: admin
- **Password**: admin123

## API Usage

### Authentication

#### Get Access Token
```bash
curl -X POST https://localhost:7001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=admin123"
```

**Response:**
```json
{
  "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "def502004a8b7...",
  "scope": "auditoria memos security"
}
```

#### Token Claims Structure
```json
{
  "sub": "user-123",
  "employee_id": "employee-777",
  "scope": "auditoria memos",
  "roles": ["AUDITORIA_AUDITOR_ADMIN", "MEMOS_MEMO_USER"],
  "permissions": [
    "AUDITORIA:EXPEDIENTES:READ",
    "AUDITORIA:EXPEDIENTES:CREATE",
    "MEMOS:DOCUMENTOS:READ"
  ]
}
```

### Dynamic Menu System

#### Get User Menu by Application
```bash
curl -X GET https://localhost:7001/api/menu/SECURITY \
  -H "Authorization: Bearer <access_token>"
```

#### Get All User Menus
```bash
curl -X GET https://localhost:7001/api/menu \
  -H "Authorization: Bearer <access_token>"
```

**Menu Response Example:**
```json
{
  "applicationCode": "SECURITY",
  "applicationName": "Sistema de Seguridad",
  "modules": [
    {
      "moduleId": "...",
      "code": "USER_ADMIN",
      "name": "Administración de Usuarios",
      "description": "Gestión de usuarios del sistema",
      "icon": "users",
      "order": 1,
      "options": [
        {
          "optionId": "...",
          "code": "USER_CRUD",
          "name": "Gestión de Usuarios",
          "route": "/api/users",
          "httpMethod": "GET",
          "icon": "user-cog",
          "allowedActions": ["READ", "CREATE", "UPDATE"]
        }
      ]
    }
  ]
}
```

### User Management

#### Get All Users
```bash
curl -X GET https://localhost:7001/api/users \
  -H "Authorization: Bearer <access_token>"
```

#### Create User
```bash
curl -X POST https://localhost:7001/api/users \
  -H "Authorization: Bearer <access_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "password": "password123",
    "employeeId": "00000000-0000-0000-0000-000000000001",
    "applicationIds": ["<app-id>"]
  }'
```

### Authorization Example

For consumer microservices, validate permissions:

```csharp
[HttpGet("expedientes")]
[Authorize(Policy = "Permission:AUDITORIA:EXPEDIENTES:GET")]
public async Task<IActionResult> GetExpedientes()
{
    // This endpoint requires AUDITORIA:EXPEDIENTES:READ permission
    return Ok(expedientes);
}
```

## Database Schema

### Key Relationships
- Users ↔ Applications (Many-to-Many via UserApplications)
- Users ↔ Roles (Many-to-Many via UserRoles)  
- Applications → Modules → Options (One-to-Many chain)
- Roles → Application (Many-to-One)
- Permissions → Role + Option (Many-to-One each)

### Seed Data
The system includes initial seed data:
- Admin user with full permissions
- AUDITORIA, MEMOS, and SECURITY applications
- Sample modules and options structure
- Basic roles and permissions
- Sample API options

## Testing

### Run Unit Tests
```bash
dotnet test tests/SecurityMicroservice.UnitTests/
```

### Test Coverage
- Authentication service validation
- Permission authorization handler
- Password hashing/verification
- Token claim generation
- Menu service functionality

## Configuration

### Environment Variables
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection="<connection-string>"
OpenIddict__Issuer="https://localhost:7001/"
```

### CORS Configuration
Update `appsettings.json` for production environments:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://auditoria.company.com",
      "https://memos.company.com"
    ]
  }
}
```

## Deployment

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "SecurityMicroservice.Api.dll"]
```

### Production Considerations
- Replace development certificates with production certificates
- Configure proper connection strings
- Enable HTTPS redirection
- Configure proper CORS policies
- Set up proper logging (Application Insights, etc.)
- Consider using Azure Key Vault for secrets

## API Documentation

Complete API documentation is available via Swagger UI at `/swagger` when running in development mode.

### Available Endpoints

#### Authentication
- `POST /connect/token` - OAuth2 token endpoint
- `POST /api/auth/forgot-password` - Password recovery
- `POST /api/auth/reset-password` - Password reset

#### User Management  
- `GET /api/users` - List users
- `POST /api/users` - Create user
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

#### Menu System
- `GET /api/menu` - Get all user menus
- `GET /api/menu/{applicationCode}` - Get menu by application

### Scopes
- `auditoria`: Access to audit system
- `memos`: Access to memo system  
- `security`: Access to security administration

### Permission Format
Permissions follow the pattern: `{APPLICATION}:{OPTION}:{ACTION}`

Examples:
- `AUDITORIA:EXPEDIENTES:READ`
- `AUDITORIA:EXPEDIENTES:CREATE` 
- `MEMOS:DOCUMENTOS:READ`
- `MEMOS:DOCUMENTOS:EXPORT`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License.
