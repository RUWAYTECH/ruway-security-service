using Rokys.Memo.Common.Constant;
using Ruway.Events.Command.Interfaces.Events;
using Ruway.Security.Subscription.Hub.Models;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.Request.User;
using System.Numerics;

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
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            // Buscar usuario por algún criterio único - como email + documentNumber
            var existingUserResponse = await FindUserByPeopleDataAsync(peopleEvent.Email, peopleEvent.DocumentNumber);
            if (existingUserResponse.Success && existingUserResponse.UserId.HasValue)
            {
                var updateRequest = new UserRequestDto
                {
                    FirstName = peopleEvent.FirstName,
                    LastName = peopleEvent.LastName,
                    Email = peopleEvent.Email,
                    PhoneNumber = peopleEvent.Phone,
                    IsExternal = peopleEvent.IsExternal,
                    RoleCode = peopleEvent.RoleCode,
                    Status = peopleEvent.IsActive ? "Active" : "Inactive"
                };

                var updateResult = await userService.Update(existingUserResponse.UserId.Value, updateRequest);
                
                if (updateResult.IsValid && updateResult.Data != null)
                {
                    return UserCreationResult.CreateSuccess(updateResult.Data.UserId, updateResult.Data.UserName, "");
                }
                else
                {
                    return UserCreationResult.CreateError($"Error actualizando usuario: {string.Join(", ", updateResult.Messages.Select(m => m.Message))}");
                }
            }
            else
            {
                _logger.LogInformation("Usuario no encontrado para persona {PeopleId}, creando nuevo usuario", peopleEvent.UserReferenceId);
                return await CreateUserAsync(new PeopleCreatedEvent(peopleEvent.UserReferenceId, peopleEvent.EmployeeId, peopleEvent.FirstName, peopleEvent.LastName, peopleEvent.DocumentNumber, peopleEvent.Email, peopleEvent.PersonalEmail, peopleEvent.Phone, peopleEvent.RoleCode, peopleEvent.IsExternal, peopleEvent.IsActive));
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
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
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
                Status = userModel.IsActive ? "Active" : "Inactive"
            };

            var result = await userService.Create(userRequest);

            if (result.IsValid && result.Data != null)
            {
                _logger.LogInformation(
                    "Usuario creado exitosamente: {UserId} para {UserType} {Name}",
                    result.Data.UserId,
                    userModel.IsExternal ? "Empleado" : "Beneficiario",
                    $"{userModel.FirstName} {userModel.LastName}"
                );

                return UserCreationResult.CreateSuccess(result.Data.UserId, userModel.DocumentNumber, temporaryPassword);
            }
            else
            {
                var errorMessage = string.Join(", ", result.Messages.Select(m => m.Message));
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
    /// Busca un usuario por datos de persona
    /// </summary>
    private async Task<UserCreationResult> FindUserByPeopleDataAsync(string email, string documentNumber)
    {
        try
        {
            // Similar al método anterior - necesitaríamos métodos adicionales en UserService
            return UserCreationResult.CreateError("Método de búsqueda por datos de persona no implementado");
        }
        catch (Exception ex)
        {
            return UserCreationResult.CreateError($"Error buscando usuario: {ex.Message}", ex);
        }
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
}