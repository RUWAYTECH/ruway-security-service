using Ruway.Events.Command.Interfaces.Events;
using Ruway.Security.Subscription.Hub.Models;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.User;
using SecurityMicroservice.Shared.Response.Common;

namespace Ruway.Security.Subscription.Hub.Services;

/// <summary>
/// Servicio para crear usuarios desde eventos de empleados y beneficiarios
/// </summary>
public class EventUserManagementService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventUserManagementService> _logger;

    public EventUserManagementService(
        IServiceProvider serviceProvider,
        ILogger<EventUserManagementService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    /// <summary>
    /// Actualiza un usuario desde un evento de persona actualizada
    /// </summary>
    public async Task<UserCreationResult> UpdateUserFromPeopleAsync(PeopleUpdatedEvent peopleEvent)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sp = scope.ServiceProvider;
            var userService = sp.GetRequiredService<IUserService>();

            // Un IsActive=false acotado a una aplicación es una revocación de ESA aplicación,
            // no una baja de la persona: el mismo usuario puede seguir siendo empleado en
            // MITALLA aunque deje de ser beneficiario en PORTALCLIENTE.
            if (peopleEvent.IsActive == false)
            {
                return await RevokeApplicationAccessAsync(peopleEvent.UserReferenceId, peopleEvent.ApplicationCode);
            }

            // Buscar usuario por algún criterio único - como email + documentNumber
            var existingUserResponse = await userService.GetById(peopleEvent.UserReferenceId);
            var temporaryPassword = "";
            if (existingUserResponse.Data != null && existingUserResponse.Data.UserId != Guid.Empty)
            {
                var existingUser = existingUserResponse.Data;

                // Se resuelven aplicación y rol ANTES de tocar los accesos: los repositorios
                // hacen SaveChanges por operación, así que el remove + add de más abajo no está
                // envuelto en ninguna transacción. Si el rol del evento no se puede resolver es
                // preferible abortar y dejar al usuario con el rol que ya tenía, antes que
                // quitárselo y no poder devolverle ninguno.
                var (app, role, resolveError) = await ResolveAppAndRoleAsync(sp, peopleEvent.ApplicationCode, peopleEvent.RoleCode);
                if (resolveError != null)
                {
                    _logger.LogError(
                        "No se actualizaron los accesos del usuario {UserId}: {Error}. Se conservan sus accesos actuales",
                        existingUser.UserId, resolveError);
                    return UserCreationResult.CreateError(resolveError);
                }

                var updateRequest = new UserRequestDto
                {
                    Username = peopleEvent.DocumentNumber,
                    FirstName = peopleEvent.FirstName,
                    LastName = peopleEvent.LastName,
                    Email = peopleEvent.Email,
                    PhoneNumber = peopleEvent.Phone,
                    IsExternal = peopleEvent.IsExternal,
                    RoleCode = peopleEvent.RoleCode,
                    EmployeeId = peopleEvent.EmployeeId != Guid.Empty ? peopleEvent.EmployeeId : existingUser.EmployeeId,
                    Status = peopleEvent.IsActive ? "Active" : "Inactive"
                };
                if (existingUser.Email != updateRequest.Email)
                {
                    temporaryPassword = GenerateTemporaryPassword();
                }

                var updateResult = await userService.UpdatePartial(existingUser.UserId, updateRequest, temporaryPassword);

                if (updateResult.IsValid && updateResult.Data != null)
                {
                    var syncResult = await SyncUserApplicationAsync(
                        existingUser.UserId, app!, role!, peopleEvent.RoleCode);
                    if (!syncResult.IsValid)
                    {
                        return UserCreationResult.CreateError(Describe(syncResult));
                    }

                    return UserCreationResult.CreateSuccess(updateResult.Data.UserId.Value, updateResult.Data.Username, "");
                }
                else
                {
                    return UserCreationResult.CreateError($"Error actualizando usuario: {Describe(updateResult)}");
                }
            }
            else
            {
                _logger.LogInformation("Usuario no encontrado para persona {PeopleId}, creando nuevo usuario", peopleEvent.UserReferenceId);
                return await CreateUserAsync(new PeopleCreatedEvent(peopleEvent.UserReferenceId, peopleEvent.EmployeeId,
                peopleEvent.FirstName, peopleEvent.LastName, peopleEvent.DocumentNumber, peopleEvent.Email, peopleEvent.PersonalEmail,
                 peopleEvent.Phone, peopleEvent.ApplicationCode, peopleEvent.RoleCode, peopleEvent.IsExternal, peopleEvent.IsActive));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando usuario desde evento de persona {PeopleId}", peopleEvent.UserReferenceId);
            return UserCreationResult.CreateError($"Error procesando actualización de persona: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Crea un usuario genérico desde un modelo de evento
    /// </summary>
    public async Task<UserCreationResult> CreateUserAsync(PeopleCreatedEvent userModel)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sp = scope.ServiceProvider;
            var userService = sp.GetRequiredService<IUserService>();

            // Igual que en el update: si la aplicación o el rol del evento no existen, no tiene
            // sentido crear un usuario al que después no se le va a poder dar ningún acceso.
            var (app, role, resolveError) = await ResolveAppAndRoleAsync(sp, userModel.ApplicationCode, userModel.RoleCode);
            if (resolveError != null)
            {
                _logger.LogError("No se creó el usuario para persona {PeopleId}: {Error}", userModel.UserReferenceId, resolveError);
                return UserCreationResult.CreateError(resolveError);
            }

            // Generar contraseña temporal
            var temporaryPassword = GenerateTemporaryPassword();

            var userRequest = new UserRequestDto
            {
                UserId = userModel.UserReferenceId,
                Username = userModel.DocumentNumber,
                Password = temporaryPassword,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                Email = userModel.Email,
                PhoneNumber = userModel.Phone,
                IsExternal = userModel.IsExternal,
                EmployeeId = userModel.EmployeeId,
                RoleCode = userModel.RoleCode,
                Status = userModel.IsActive ? "Active" : "Inactive",
                IsSizing = userModel.IsSizing
            };

            var result = await userService.Create(userRequest);

            if (result.IsValid && result.Data != null)
            {
                _logger.LogInformation(
                    "Usuario creado exitosamente: {UserId} para {UserType} {Name}",
                    result.Data.UserId,
                    // IsExternal=true es beneficiario (sizing lo publica así desde
                    // SaleBeneficiaryMassiveService); IsExternal=false es empleado.
                    userModel.IsExternal ? "Beneficiario" : "Empleado",
                    $"{userModel.FirstName} {userModel.LastName}"
                );

                // Se usa el UserId que devuelve Create, no el del evento: cuando un beneficiario
                // pasa a colaborador, Create reutiliza el usuario que ya existía y su UserId
                // puede no coincidir con el UserReferenceId que trae el evento.
                var syncResult = await SyncUserApplicationAsync(
                    result.Data.UserId, app!, role!, userModel.RoleCode);
                if (!syncResult.IsValid)
                {
                    // El usuario existe pero sin acceso a ninguna aplicación: no puede reportarse
                    // como éxito o nadie se enterará de que hay que arreglarlo a mano.
                    return UserCreationResult.CreateError(Describe(syncResult));
                }

                return UserCreationResult.CreateSuccess(result.Data.UserId, userModel.DocumentNumber, temporaryPassword);
            }
            else
            {
                var errorMessage = Describe(result);
                _logger.LogWarning("Error creando usuario: {Error}", errorMessage);
                return UserCreationResult.CreateError(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateUserAsync");
            return UserCreationResult.CreateError($"Error interno: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Elimina un usuario
    /// </summary>
    public async Task<UserCreationResult> DeleteUserAsync(PeopleDeletedEvent userEvent)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var entity = await userService.GetById(userEvent.UserId);
            if (entity.Data == null)
            {
                return UserCreationResult.CreateError($"No existe el usuario {userEvent.UserId}; no se eliminó nada");
            }

            var result = await userService.PhysicallyDelete(userEvent.UserId);

            if (result.IsValid)
            {
                _logger.LogInformation("Usuario eliminado exitosamente: {UserId}", userEvent.UserId);
                return UserCreationResult.CreateSuccess(userEvent.UserId, entity.Data.UserName, null);
            }
            else
            {
                var errorMessage = Describe(result);
                _logger.LogWarning("Error eliminando usuario: {Error}", errorMessage);
                return UserCreationResult.CreateError(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en DeleteUserAsync");
            return UserCreationResult.CreateError($"Error interno: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Revoca el acceso del usuario a UNA aplicación y deja intactas las demás. Si esa era su
    /// última aplicación activa, además se desactiva el usuario para no dejar cuentas activas
    /// sin ningún acceso.
    /// </summary>
    private async Task<UserCreationResult> RevokeApplicationAccessAsync(Guid userId, string applicationCode)
    {
        // Un scope propio por operación, por lo mismo que en SyncUserApplicationAsync: los
        // repositorios consultan con AsNoTracking y adjuntan al actualizar, así que compartir
        // DbContext entre operaciones sobre la misma fila termina en conflicto de tracking.
        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var userService = sp.GetRequiredService<IUserService>();
        var appService = sp.GetRequiredService<IApplicationService>();

        var existingUserResponse = await userService.GetById(userId);
        if (existingUserResponse.Data == null || existingUserResponse.Data.UserId == Guid.Empty)
        {
            return UserCreationResult.CreateError($"No existe el usuario {userId}; no se revocó ningún acceso");
        }
        var userName = existingUserResponse.Data.UserName;

        // Sin aplicación no hay forma de acotar la revocación: se conserva el comportamiento
        // anterior de desactivar al usuario completo.
        if (string.IsNullOrWhiteSpace(applicationCode))
        {
            _logger.LogWarning("Evento sin ApplicationCode para el usuario {UserId}: se desactiva el usuario completo", userId);
            return await DeactivateUserAsync(userId, userName);
        }

        var app = await appService.GetByCodeAsync(applicationCode);
        if (app.Data == null)
        {
            return UserCreationResult.CreateError(
                $"No se encontró la aplicación {applicationCode} (o está inactiva); no se revocó ningún acceso del usuario {userId}");
        }

        var revoked = await RemoveUserFromApp(userId, app.Data.ApplicationId, applicationCode);
        if (!revoked.IsValid)
        {
            return UserCreationResult.CreateError(
                $"Error revocando {applicationCode} al usuario {userId}: {Describe(revoked)}");
        }

        _logger.LogInformation("Acceso revocado: usuario {UserId}, aplicación {AppCode}", userId, applicationCode);

        // GetByUserIdAsync no filtra por UserApplication.IsActive (solo por el estado del User),
        // así que hay que filtrar explícitamente las que siguen activas.
        List<UserApplicationDto> remaining;
        using (var readScope = _serviceProvider.CreateScope())
        {
            remaining = await readScope.ServiceProvider
                .GetRequiredService<IUserApplicationService>()
                .GetByUserIdAsync(userId);
        }

        if (remaining.Any(ua => ua.IsActive))
        {
            return UserCreationResult.CreateSuccess(userId, userName, null);
        }

        _logger.LogInformation("Al usuario {UserId} no le quedan aplicaciones activas: se desactiva", userId);
        return await DeactivateUserAsync(userId, userName);
    }

    private async Task<UserCreationResult> DeactivateUserAsync(Guid userId, string userName)
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var deactivated = await userService.Delete(userId);
        return deactivated.IsValid
            ? UserCreationResult.CreateSuccess(userId, userName, null)
            : UserCreationResult.CreateError($"Error desactivando al usuario {userId}: {Describe(deactivated)}");
    }

    /// <summary>
    /// Resuelve la aplicación y el rol que trae el evento. Devuelve un mensaje de error en vez de
    /// null silencioso para que el llamador pueda abortar antes de modificar accesos.
    /// </summary>
    private async Task<(ApplicationDto? App, RoleDto? Role, string? Error)> ResolveAppAndRoleAsync(
        IServiceProvider sp, string applicationCode, string roleCode)
    {
        var appService = sp.GetRequiredService<IApplicationService>();
        var roleService = sp.GetRequiredService<IRoleService>();

        if (string.IsNullOrWhiteSpace(applicationCode))
        {
            return (null, null, "El evento no trae ApplicationCode");
        }

        var app = await appService.GetByCodeAsync(applicationCode);
        if (app.Data == null)
        {
            return (null, null, $"No se encontró la aplicación {applicationCode} (o está inactiva)");
        }

        var roles = await roleService.GetByApplicationIdAsync(app.Data.ApplicationId);
        var role = roles.FirstOrDefault(a => string.Equals(a.Code?.Trim(), roleCode?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (role == null)
        {
            return (app.Data, null, $"No se encontró el rol {roleCode} para la aplicación {applicationCode}");
        }

        return (app.Data, role, null);
    }

    /// <summary>
    /// Genera una contraseña temporal segura
    /// </summary>
    private string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Deja al usuario con exactamente el rol que trae el evento en esa aplicación.
    /// CreateAsync rechaza una asignación que ya está activa, así que primero se revoca la que
    /// exista; si no hay ninguna activa se salta ese paso, porque DeleteAsync devolvería error y
    /// bloquearía una primera asignación legítima.
    /// </summary>
    private async Task<ResponseDto> SyncUserApplicationAsync(
        Guid userId, ApplicationDto app, RoleDto role, string roleCode)
    {
        // Cada operación usa su propio scope, es decir su propio DbContext. Los repositorios
        // consultan con AsNoTracking y luego hacen Attach al actualizar: si el revoke y el
        // add compartieran contexto, el segundo intentaría adjuntar una segunda instancia de
        // la misma UserApplication y EF lo rechazaría, dejando el acceso revocado a medias.
        List<UserApplicationDto> assignments;
        using (var scope = _serviceProvider.CreateScope())
        {
            assignments = await scope.ServiceProvider
                .GetRequiredService<IUserApplicationService>()
                .GetByUserIdAsync(userId);
        }

        var hasActiveAssignment = assignments.Any(ua => ua.ApplicationId == app.ApplicationId && ua.IsActive);

        if (hasActiveAssignment)
        {
            var removeResult = await RemoveUserFromApp(userId, app.ApplicationId, app.Code);
            if (!removeResult.IsValid)
            {
                _logger.LogError(
                    "No se pudo revocar {AppCode} al usuario {UserId} antes de reasignar el rol {RoleCode}: {Error}",
                    app.Code, userId, roleCode, Describe(removeResult));
                return removeResult;
            }
        }

        var addResult = await AddUserToApp(userId, app.ApplicationId, role.RoleId);
        if (!addResult.IsValid)
        {
            // Si se llegó aquí después del revoke, el usuario quedó sin rol en esa aplicación y no
            // hay transacción que deshacerlo. Se registra con todo el contexto porque el subscriber
            // consume con autoAck y sin DLQ: no habrá reintento automático.
            _logger.LogError(
                "Usuario {UserId} quedó SIN ROL en {AppCode}: falló la asignación de {RoleCode}: {Error}",
                userId, app.Code, roleCode, Describe(addResult));
        }

        return addResult;
    }

    private async Task<ResponseDto> AddUserToApp(Guid userId, Guid applicationId, Guid roleId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userAppService = scope.ServiceProvider.GetRequiredService<IUserApplicationService>();

            // CreateAsync atrapa sus propias excepciones y las devuelve como ResponseDto.Error,
            // así que el resultado hay que inspeccionarlo: descartarlo esconde el fallo.
            var created = await userAppService.CreateAsync(new SecurityMicroservice.Shared.Request.UserApplication.CreateUserApplicationRequest
            {
                UserId = userId,
                ApplicationId = applicationId,
                RoleIds = new List<Guid> { roleId }
            }, isPublishEvent: false);

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error agregando usuario a la aplicación para usuario {UserId}", userId);
            return ResponseDto.Error($"Error agregando el usuario {userId} a la aplicación: {ex.Message}");
        }
    }

    private async Task<ResponseDto> RemoveUserFromApp(Guid userId, Guid applicationId, string appCode)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userAppService = scope.ServiceProvider.GetRequiredService<IUserApplicationService>();
            return await userAppService.DeleteAsync(userId, applicationId, isPublishEvent: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removiendo usuario {UserId} de la aplicación {AppCode}", userId, appCode);
            return ResponseDto.Error($"Error removiendo el usuario {userId} de {appCode}: {ex.Message}");
        }
    }

    private static string Describe(ResponseDto response) =>
        string.Join(", ", response.Messages.Select(m => m.Message));
}
