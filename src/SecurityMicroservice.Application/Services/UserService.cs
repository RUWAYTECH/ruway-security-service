using AutoMapper;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.User;
using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<ResponseDto<PaginationResponseDto<UserDto>>> GetPaged(UserPaginationRequestDto requestDto);
}

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

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordService.HashPassword(request.Password),
            EmployeeId = request.EmployeeId,
            Status = UserStatus.Active
        };

        var createdUser = await _userRepository.CreateAsync(user);
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(request.Username))
            user.Username = request.Username;

        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = _passwordService.HashPassword(request.Password);

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<UserStatus>(request.Status, out var status))
            user.Status = status;

        if (request.EmployeeId.HasValue)
            user.EmployeeId = request.EmployeeId.Value;

        var updatedUser = await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserDto>(updatedUser);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        await _userRepository.DeleteAsync(userId);
        return true;
    }

    public async Task<ResponseDto<PaginationResponseDto<UserDto>>> GetPaged(UserPaginationRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<UserDto>>();
        try
        {
            System.Linq.Expressions.Expression<System.Func<User, bool>> filter = x => x.UserApplications.Any(a => a.Application.Code == requestDto.ApplicationCode);
            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                var filterLower = requestDto.Filter.ToLower();

                filter = x =>
                    x.Username.ToLower().Contains(filterLower) ||
                    x.FirstName.ToLower().Contains(filterLower) ||
                    x.LastName.ToLower().Contains(filterLower);
            }

            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = q => q.OrderBy(x => x.CreatedAt);

            var (items, totalRows) = await _userRepository.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                applicationCode: requestDto.ApplicationCode,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize
            );

            response.Data = new PaginationResponseDto<UserDto>
            {
                Items = _mapper.Map<IEnumerable<UserDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<UserDto>>(ex.Message);
        }
        return response;
    }
}