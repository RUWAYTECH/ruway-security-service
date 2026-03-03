using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ruway.Events.Command.Configuration;
using Ruway.Events.Command.Interfaces.Events;

namespace Ruway.Security.Subscription.Hub.Tests.Services;

/// <summary>
/// Pruebas unitarias puntuales para publicación de eventos People
/// </summary>
public class EventPublisherTests
{
    [Fact]
    public async Task PublishAsync_CuandoPeopleCreated_DeberiaPublicarEvento_E2E()
    {
        var peopleEvent = new PeopleCreatedEvent(
            PeopleId: Guid.NewGuid(),
            EmployeeId: Guid.NewGuid(),
            FirstName: "Juan",
            LastName: "Pérez",
            DocumentNumber: "12345678",
            Email: "juan.perez@test.com",
            Phone: "555-0123",
            Relationship: "Titular",
            IsExternal: false,
            BirthDate: new DateTime(1990, 1, 1),
            IsActive: true
        );

        using var scope = BuildProvider().CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var act = async () => await publisher.PublishAsync(peopleEvent, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_CuandoPeopleUpdated_DeberiaPublicarEvento_E2E()
    {
        var peopleEvent = new PeopleUpdatedEvent(
            PeopleId: Guid.NewGuid(),
            EmployeeId: Guid.NewGuid(),
            FirstName: "Juan Carlos",
            LastName: "Pérez González",
            DocumentNumber: "12345678",
            Email: "juan.carlos@test.com",
            Phone: "555-9876",
            Relationship: "Titular",
            IsExternal: false,
            BirthDate: new DateTime(1990, 1, 1),
            IsActive: true
        );

        using var scope = BuildProvider().CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var act = async () => await publisher.PublishAsync(peopleEvent, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private static ServiceProvider BuildProvider()
    {
        var settings = LoadRabbitSettings();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEventPublisher(settings);

        return services.BuildServiceProvider();
    }

    private static RabbitMQSettings LoadRabbitSettings()
    {
        var hubSettingsPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "../../../../Ruway.Security.Subscription.Hub"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(hubSettingsPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var section = configuration.GetSection("RabbitMQ");

        return new RabbitMQSettings
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? section["HostName"] ?? "localhost",
            Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var envPort)
                ? envPort
                : section.GetValue<int?>("Port") ?? 5672,
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? section["UserName"] ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? section["Password"] ?? "guest",
            VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? section["VirtualHost"] ?? "/",
            EventsExchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? section["EventsExchange"] ?? "rokys.events",
            MicroserviceName = Environment.GetEnvironmentVariable("RABBITMQ_MICROSERVICE")
                ?? section["MicroserviceName"]
                ?? "security",
            ConnectionTimeout = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION_TIMEOUT"), out var timeout)
                ? timeout
                : section.GetValue<int?>("ConnectionTimeout") ?? 30000,
            EnableRetries = bool.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_ENABLE_RETRIES"), out var retries)
                ? retries
                : section.GetValue<bool?>("EnableRetries") ?? true,
            MaxRetries = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_MAX_RETRIES"), out var maxRetries)
                ? maxRetries
                : section.GetValue<int?>("MaxRetries") ?? 3,
            ClientProvidedName = Environment.GetEnvironmentVariable("RABBITMQ_CLIENT_NAME")
                ?? section["ClientProvidedName"]
                ?? "Ruway.Security.Subscription.Hub.Tests"
        };
    }
}