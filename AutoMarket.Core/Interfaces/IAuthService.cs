using AutoMarket.Core.DTOs.Auth;
using AutoMarket.Core.Helpers;

namespace AutoMarket.Core.Interfaces;

public interface IAuthService
{
    /// <summary>Registers a new user and returns a JWT token response.</summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);

    /// <summary>Authenticates a user by email/password and returns a JWT token response.</summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);

    /// <summary>Returns the profile of the authenticated user.</summary>
    Task<UserDto> GetCurrentUserAsync(int userId);

    /// <summary>Updates username or email of the authenticated user.</summary>
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);

    /// <summary>Returns a paginated list of all users (Admin only).</summary>
    Task<PagedResult<UserDto>> GetAllUsersAsync(QueryParams queryParams);

    /// <summary>Deletes a user by id (Admin only).</summary>
    Task DeleteUserAsync(int userId);
}
