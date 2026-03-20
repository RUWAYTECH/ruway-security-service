using Ruway.Events.Command.Interfaces.Events;
using Ruway.Events.Command.Interfaces.Constants;

namespace Ruway.Security.Subscription.Hub.Services;

public class EventSubscriptionService : BackgroundService
{
    private readonly ILogger<EventSubscriptionService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IEventSubscriber? _eventSubscriber;

    public EventSubscriptionService(
        ILogger<EventSubscriptionService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando servicio de suscripción a eventos...");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Intentando establecer conexión con RabbitMQ...");

                using var scope = _serviceProvider.CreateScope();
                _eventSubscriber = scope.ServiceProvider.GetRequiredService<IEventSubscriber>();

                // Suscribirse a eventos de personas  
                await _eventSubscriber.SubscribeAsync<PeopleCreatedEvent>(
                    HandlePeopleCreatedAsync,
                    EventConstants.PeopleEvents.PeopleCreated,
                    stoppingToken);

                await _eventSubscriber.SubscribeAsync<PeopleUpdatedEvent>(
                    HandlePeopleUpdatedAsync, 
                    EventConstants.PeopleEvents.PeopleUpdated,
                    stoppingToken);

                await _eventSubscriber.SubscribeAsync<PeopleDeletedEvent>(
                    HandlePeopleDeletionAsync,
                    EventConstants.PeopleEvents.PeopleDeleted,
                    stoppingToken);

                _logger.LogInformation("Suscripciones a eventos configuradas correctamente");

                // Iniciar la escucha de eventos
                await _eventSubscriber.StartListeningAsync(stoppingToken);
                
                _logger.LogInformation("Conexión con RabbitMQ establecida exitosamente. Escuchando eventos...");

                // Mantener el servicio ejecutándose mientras la conexión esté activa
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Servicio de suscripción cancelado");
                break;
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                _logger.LogWarning(
                    "No se puede conectar a RabbitMQ. Reintentando en 30 segundos... Error: {ErrorMessage}",
                    ex.Message);
                
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error inesperado en el servicio de suscripción a eventos. Reintentando en 30 segundos...");
                
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            finally
            {
                // Limpiar recursos si es necesario
                if (_eventSubscriber != null)
                {
                    try
                    {
                        await _eventSubscriber.StopListeningAsync(CancellationToken.None);
                        _eventSubscriber.Dispose();
                        _eventSubscriber = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al limpiar recursos del EventSubscriber");
                    }
                }
            }
        }
        
        _logger.LogInformation("Servicio de suscripción a eventos finalizado");
    }
    private async Task HandlePeopleCreatedAsync(PeopleCreatedEvent peopleCreated)
    {
        try
        {
            _logger.LogInformation(
                "Procesando evento PeopleCreated para persona {PeopleId}: {FirstName} {LastName}, Empleado: {EmployeeId}",
                peopleCreated.UserReferenceId,
                peopleCreated.FirstName,
                peopleCreated.LastName,
                peopleCreated.EmployeeId);

            using var scope = _serviceProvider.CreateScope();
            var userManagementService = scope.ServiceProvider.GetRequiredService<EventUserManagementService>();
    
            var result = await userManagementService.CreateUserAsync(peopleCreated);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Usuario creado exitosamente para persona {PeopleId}. UserId: {UserId}, Username: {Username}",
                    peopleCreated.UserReferenceId,
                    result.UserId,
                    result.GeneratedUsername);

                await LogPeopleUserCreationAsync(peopleCreated, result);
            }
            else
            {
                _logger.LogError(
                    "Error creando usuario para persona {PeopleId}: {Error}",
                    peopleCreated.UserReferenceId,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando PeopleCreated para persona {UserReferenceId}", 
                peopleCreated.UserReferenceId);
        }
    }

    private async Task HandlePeopleUpdatedAsync(PeopleUpdatedEvent peopleUpdated)
    {
        try
        {
            _logger.LogInformation(
                "Procesando evento PeopleUpdated para persona {UserReferenceId}: {FirstName} {LastName}, Empleado: {EmployeeId}",
                peopleUpdated.UserReferenceId,
                peopleUpdated.FirstName,
                peopleUpdated.LastName,
                peopleUpdated.EmployeeId);

            using var scope = _serviceProvider.CreateScope();
            var userManagementService = scope.ServiceProvider.GetRequiredService<EventUserManagementService>();

            var result = await userManagementService.UpdateUserFromPeopleAsync(peopleUpdated);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Usuario actualizado exitosamente para persona {UserReferenceId}. UserId: {UserId}",
                    peopleUpdated.UserReferenceId,
                    result.UserId);
            }
            else
            {
                _logger.LogError(
                    "Error actualizando usuario para persona {UserReferenceId}: {Error}",
                    peopleUpdated.UserReferenceId,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando PeopleUpdated para persona {UserReferenceId}", 
                peopleUpdated.UserReferenceId);
        }
    }

    private async Task HandlePeopleDeletionAsync(PeopleDeletedEvent userDeleted)
    {
        try
        {
            _logger.LogInformation("Procesando evento UserDeleted para usuario {UserId}", userDeleted.UserId);
            using var scope = _serviceProvider.CreateScope();
            var userManagementService = scope.ServiceProvider.GetRequiredService<EventUserManagementService>();
            var result = await userManagementService.DeleteUserAsync(userDeleted.UserId);
            if (result.Success)
            {
                _logger.LogInformation("Usuario eliminado exitosamente: {UserId}", userDeleted.UserId);
            }
            else
            {
                _logger.LogWarning("Error eliminando usuario: {Error}", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando UserDeleted para usuario {UserId}", userDeleted.UserId);
        }
    }

    /// <summary>
    /// Registra la creación de usuario para empleado (para auditoría/notificaciones)
    /// </summary>
    private async Task LogUserCreationAsync(EmployeeCreatedEvent employeeEvent, Models.UserCreationResult result)
    {
        try
        {
            // Aquí podrías:
            // 1. Guardar en una tabla de auditoría
            // 2. Enviar email con credenciales
            // 3. Crear notificación para administradores
            // 4. Enviar a otro sistema de logging
            
            _logger.LogInformation(
                "USUARIO CREADO - Empleado: {EmployeeId}, Usuario: {UserId}, Username: {Username}, Email: {Email}",
                employeeEvent.EmployeeId,
                result.UserId,
                result.GeneratedUsername,
                employeeEvent.Email);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando creación de usuario para empleado {EmployeeId}", employeeEvent.EmployeeId);
        }
    }

    /// <summary>
    /// Registra la creación de usuario para persona (para auditoría/notificaciones)
    /// </summary>
    private async Task LogPeopleUserCreationAsync(PeopleCreatedEvent peopleEvent, Models.UserCreationResult result)
    {
        try
        {
            _logger.LogInformation(
                "USUARIO CREADO - Persona: {UserReferenceId}, Usuario: {UserId}, Username: {Username}, Email: {Email}, Empleado: {EmployeeId}",
                peopleEvent.UserReferenceId,
                result.UserId,
                result.GeneratedUsername,
                peopleEvent.Email,
                peopleEvent.EmployeeId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando creación de usuario para persona {UserReferenceId}", peopleEvent.UserReferenceId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo el servicio de suscripción a eventos...");

        if (_eventSubscriber != null)
        {
            await _eventSubscriber.StopListeningAsync(cancellationToken);
            _eventSubscriber.Dispose();
        }

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Servicio de suscripción a eventos detenido");
    }
}