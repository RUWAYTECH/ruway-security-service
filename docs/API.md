# Security Microservice API Documentation

## Overview
This API provides authentication and authorization services using OAuth2/OpenID Connect with OpenIddict.

## Base URL
```
https://localhost:7001
```

## Authentication Endpoints

### POST /connect/token
Obtain an access token using username and password.

**Request Body (form-urlencoded):**
```
grant_type=password
username=admin
password=admin123
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

### POST /connect/token (Refresh)
Refresh an existing access token.

**Request Body (form-urlencoded):**
```
grant_type=refresh_token
refresh_token=<refresh_token>
```

## User Management Endpoints

### GET /api/users
Get all users (requires authentication).

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:**
```json
[
  {
    "userId": "guid",
    "username": "admin",
    "status": "Active",
    "employeeId": "guid",
    "createdAt": "2024-01-01T00:00:00Z",
    "lastLoginAt": "2024-01-01T12:00:00Z",
    "applications": [...],
    "roles": [...]
  }
]
```

### GET /api/users/{id}
Get a specific user by ID.

### POST /api/users
Create a new user.

**Request Body:**
```json
{
  "username": "newuser",
  "password": "password123",
  "employeeId": "guid",
  "applicationIds": ["guid1", "guid2"]
}
```

### PUT /api/users/{id}
Update an existing user.

### DELETE /api/users/{id}
Delete a user.

## Token Claims

### Standard Claims
- `sub`: User ID
- `name`: Username
- `iat`: Issued at
- `exp`: Expiration time

### Custom Claims
- `employee_id`: Reference to employee in master system
- `roles`: Array of user roles in format "APP_ROLE"
- `permissions`: Array of permissions in format "APP:OPTION:ACTION"
- `scope`: Space-separated application scopes

### Example Token Payload
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "name": "admin",
  "employee_id": "660e8400-e29b-41d4-a716-446655440001",
  "scope": "auditoria memos security",
  "roles": [
    "SECURITY_SUPERADMIN",
    "AUDITORIA_AUDITOR_ADMIN",
    "MEMOS_MEMO_USER"
  ],
  "permissions": [
    "AUDITORIA:EXPEDIENTES:READ",
    "AUDITORIA:EXPEDIENTES:CREATE",
    "MEMOS:DOCUMENTOS:READ"
  ],
  "iat": 1640995200,
  "exp": 1640998800
}
```

## Error Responses

### 400 Bad Request
```json
{
  "error": "invalid_request",
  "error_description": "The request is missing a required parameter."
}
```

### 401 Unauthorized
```json
{
  "error": "invalid_grant",
  "error_description": "Invalid username or password."
}
```

### 403 Forbidden
```json
{
  "error": "insufficient_scope",
  "error_description": "The request requires higher privileges than provided."
}
```

## Consumer Microservice Integration

### Validating Tokens
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:7001";
        options.Audience = "resource_server";
    });
```

### Permission-Based Authorization
```csharp
[Authorize(Policy = "Permission:AUDITORIA:EXPEDIENTES:GET")]
public async Task<IActionResult> GetExpedientes()
{
    return Ok(data);
}
```

### Custom Authorization Handler
```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permissions")
            .Select(c => c.Value).ToList();
        
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

## Scopes

### auditoria
Access to the audit system
- Required for AUDITORIA application endpoints
- Includes audit-related permissions

### memos
Access to the memo system  
- Required for MEMOS application endpoints
- Includes document management permissions

### security
Access to security administration
- Required for user/role management
- Admin-level permissions

## Rate Limiting
- Token endpoint: 10 requests per minute per IP
- API endpoints: 100 requests per minute per user

## Security Headers
All responses include appropriate security headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Strict-Transport-Security: max-age=31536000`