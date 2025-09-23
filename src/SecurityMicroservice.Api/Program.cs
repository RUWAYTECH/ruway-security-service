using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Server;
using SecurityMicroservice.Api.Authorization;
using SecurityMicroservice.Application.Mappings;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Infrastructure.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP
    options.ListenLocalhost(7001, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});

// Add services to the container.
builder.Services.AddDbContext<SecurityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict();
    
    // Suppress pending model changes warning during development
    if (builder.Environment.IsDevelopment())
    {
        options.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }
});

// OpenIddict configuration
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<SecurityDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token")
               .SetIntrospectionEndpointUris("/connect/introspect")
               .SetAuthorizationEndpointUris("/connect/authorize")
               .SetLogoutEndpointUris("/connect/logout");

        options.AllowPasswordFlow()
               .AllowRefreshTokenFlow()
               .AllowAuthorizationCodeFlow()
               .AllowClientCredentialsFlow();

        options.AcceptAnonymousClients();

        // Use ephemeral keys for all local development scenarios
        // This avoids macOS keychain permission issues
        // WARNING: In production, replace with proper certificates!
        options.AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey()
               .DisableAccessTokenEncryption();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough()
               .EnableLogoutEndpointPassthrough()
               .DisableTransportSecurityRequirement(); // Allow HTTP in development
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// Add repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Business services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMenuService, MenuService>();

// Add AutoMapper
var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});
builder.Services.AddSingleton(mapperConfig.CreateMapper());

// Add authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<PermissionAuthorizationHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Security Microservice API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Configure HTTPS redirection based on environment
if (app.Environment.IsDevelopment())
{
    // In development, don't force HTTPS redirection to allow both HTTP and HTTPS
    app.UseHttpsRedirection();
}
else
{
    // In production, enforce HTTPS
    app.UseHttpsRedirection();
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    
    await context.Database.MigrateAsync();
    await SeedData.InitializeAsync(context, passwordService);
    await SeedClients.InitializeAsync(scope.ServiceProvider);
}

app.Run();