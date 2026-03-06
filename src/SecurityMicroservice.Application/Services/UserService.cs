using AutoMapper;
using Ruway.Events.Command.Interfaces.Events;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.User;
using SecurityMicroservice.Shared.Response.Common;
using SecurityMicroservice.Shared.Response.User;

namespace SecurityMicroservice.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<ResponseDto<UserResponseDto?>> GetById(Guid userId)
    {
        var result = ResponseDto.Create<UserResponseDto>();
        try
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId);
            result.Data = user != null ? _mapper.Map<UserResponseDto>(user) : null;
        } catch (Exception ex)
        {
            result = ResponseDto.Error<UserResponseDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<UserResponseDto>> Create(UserRequestDto request)
    {
        var result = ResponseDto.Create<UserResponseDto>();
        try
        {
            var validationUser = await _userRepository.GetFirstOrDefaultAsync(
                filter: x => x.UserName == request.Username
                        && (!request.EmployeeId.HasValue || x.EmployeeId == request.EmployeeId)
            );

            if (validationUser != null)
            {
                switch (validationUser.Status)
                {
                    case UserStatus.Inactive:
                        return ResponseDto.Error<UserResponseDto>("El usuario asociado a este nombre de usuario o empleado est� inactivo.");

                    case UserStatus.Locked:
                        return ResponseDto.Error<UserResponseDto>("El usuario asociado a este nombre de usuario o empleado est� bloqueado.");

                    case UserStatus.Suspended:
                        return ResponseDto.Error<UserResponseDto>("El usuario asociado a este nombre de usuario o empleado est� suspendido.");

                    case UserStatus.Active:
                        return ResponseDto.Error<UserResponseDto>("Ya existe un usuario con el mismo nombre de usuario o empleado.");
                }

                return ResponseDto.Error<UserResponseDto>("Ya existe un usuario con el mismo nombre de usuario o empleado.");
            }
            var user = new User
            {
                UserName = request.Username,
                PasswordHash = _passwordService.HashPassword(request.Password),
                FirstName = request.FirstName ?? "",
                LastName = request.LastName ?? "",
                DateOfBirth = request.DateOfBirth ?? null,
                Email = request.Email ?? "",
                PhoneNumber = request.PhoneNumber ?? "",
                IsExternal = request.IsExternal ?? false,
                EmployeeId = request.EmployeeId,
                Status = UserStatus.Active,
                
            };
            if (request.UserId.HasValue)
            {
                user.UserId = request.UserId.Value;
            }

            _userRepository.Insert(user);

            result.Data = _mapper.Map<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserResponseDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<UserResponseDto>> Update(object userId, UserRequestDto request)
    {
        var result = ResponseDto.Create<UserResponseDto>();
        try
        {
            var entity = await _userRepository.GetByKeyAsync(userId);
            if (entity == null)
            {
                result = ResponseDto.Error<UserResponseDto>("No se pudo encontrar el permiso");
                return result;
            }

            entity.UserName = string.IsNullOrWhiteSpace(request.Username) ? entity.UserName : request.Username;
            entity.PasswordHash = string.IsNullOrWhiteSpace(request.Password) ? entity.PasswordHash : _passwordService.HashPassword(request.Password);
            entity.FirstName = string.IsNullOrWhiteSpace(request.FirstName) ? entity.FirstName : request.FirstName;
            entity.LastName = string.IsNullOrWhiteSpace(request.LastName) ? entity.LastName : request.LastName;
            entity.DateOfBirth = request.DateOfBirth ?? entity.DateOfBirth;
            entity.Email = string.IsNullOrWhiteSpace(request.Email) ? entity.Email : request.Email;
            entity.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? entity.PhoneNumber : request.PhoneNumber;
            entity.IsExternal = request.IsExternal ?? entity.IsExternal;
            entity.EmployeeId = request.EmployeeId != Guid.Empty ? request.EmployeeId : entity.EmployeeId;
            entity.Status = !string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<UserStatus>(request.Status, true, out var parsedStatus)
                            ? parsedStatus
                            : entity.Status;


            _userRepository.Update(entity);

            result.Data = _mapper.Map<UserResponseDto>(entity);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserResponseDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<PaginationResponseDto<UserResponseDto>>> GetPaged(UserPaginationRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<UserResponseDto>>();
        try
        {
            System.Linq.Expressions.Expression<System.Func<User, bool>> filter = x => x.Status == UserStatus.Active;
            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                var filterLower = requestDto.Filter.ToLower();

                filter = x =>
                    x.UserName.ToLower().Contains(filterLower) ||
                    x.FirstName.ToLower().Contains(filterLower) ||
                    x.LastName.ToLower().Contains(filterLower) ||
                    ((x.FirstName ?? "") + " " + (x.LastName ?? "")).ToLower().Contains(filterLower) ||
                    ((x.LastName ?? "") + " " + (x.FirstName ?? "")).ToLower().Contains(filterLower); ;
            }

            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = q => q.OrderBy(x => x.CreatedAt);

            var (items, totalRows) = await _userRepository.GetUserPagedAsync(
                filter: filter,
                orderBy: orderBy,
                applicationCode: requestDto.ApplicationCode,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize
            );

            response.Data = new PaginationResponseDto<UserResponseDto>
            {
                Items = _mapper.Map<IEnumerable<UserResponseDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<UserResponseDto>>(ex.Message);
        }
        return response;
    }

    public async Task<ResponseDto<UserResponseDto>> GetById(object id)
    {
        var result = ResponseDto.Create<UserResponseDto>();
        try
        {
            var entity = await _userRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == (Guid)id);
            if (entity == null)
            {
                result = ResponseDto.Error<UserResponseDto>("No se pudo encontrar el permiso");
                return result;
            }
            result.Data = _mapper.Map<UserResponseDto>(entity);

        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserResponseDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto> Delete(object id)
    {
        var result = ResponseDto.Create();
        try
        {
            var entity = await _userRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == (Guid)id);
            if (entity == null)
            {
                result = ResponseDto.Error("No se pudo encontrar el permiso");
                return result;
            }
            entity.Status = UserStatus.Inactive;
            _userRepository.Update(entity);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error(ex.Message);
        }
        return result;
    }
}