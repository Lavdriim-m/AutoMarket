using System.Security.Claims;
using System.Text;
using AutoMapper;
using AutoMarket.Core.DTOs.Auth;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AutoMarket.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepo, IMapper mapper, IConfiguration configuration)
    {
        _userRepo = userRepo;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new ArgumentException($"Email '{dto.Email}' is already registered.");

        if (await _userRepo.GetByUsernameAsync(dto.Username) != null)
            throw new ArgumentException($"Username '{dto.Username}' is already taken.");

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var created = await _userRepo.AddAsync(user);

        var token = GenerateJwtToken(created, out var expiresAt);
        return BuildAuthResponse(created, token, expiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new ArgumentException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new ArgumentException("Invalid email or password.");

        var token = GenerateJwtToken(user, out var expiresAt);
        return BuildAuthResponse(user, token, expiresAt);
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        if (dto.Email != null && dto.Email != user.Email)
        {
            if (await _userRepo.EmailExistsAsync(dto.Email))
                throw new ArgumentException($"Email '{dto.Email}' is already in use.");
            user.Email = dto.Email;
        }

        if (dto.Username != null && dto.Username != user.Username)
        {
            if (await _userRepo.GetByUsernameAsync(dto.Username) != null)
                throw new ArgumentException($"Username '{dto.Username}' is already taken.");
            user.Username = dto.Username;
        }

        await _userRepo.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResult<UserDto>> GetAllUsersAsync(QueryParams queryParams)
    {
        var users = (await _userRepo.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
            users = users.Where(u =>
                u.Username.Contains(queryParams.Search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(queryParams.Search, StringComparison.OrdinalIgnoreCase));

        users = queryParams.SortDescending
            ? users.OrderByDescending(u => u.CreatedAt)
            : users.OrderBy(u => u.CreatedAt);

        var list = users.ToList();
        var totalCount = list.Count;
        var data = list
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToList();

        return new PagedResult<UserDto>
        {
            Data = _mapper.Map<List<UserDto>>(data),
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task DeleteUserAsync(int userId)
    {
        if (!await _userRepo.ExistsAsync(userId))
            throw new KeyNotFoundException($"User {userId} not found.");
        await _userRepo.DeleteAsync(userId);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private string GenerateJwtToken(User user, out DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");
        expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role.ToString())
            }),
            Expires = expiresAt,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static AuthResponseDto BuildAuthResponse(User user, string token, DateTime expiresAt)
        => new()
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        };
}
