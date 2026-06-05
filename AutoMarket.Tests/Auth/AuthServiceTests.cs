using AutoMapper;
using AutoMarket.Core.DTOs.Auth;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Services.Auth;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AutoMarket.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _mapperMock   = new Mock<IMapper>();
        _configMock   = new Mock<IConfiguration>();

        _configMock.Setup(c => c["Jwt:Key"]).Returns("TestKeyThatIsAtLeast32CharactersLong!");
        _configMock.Setup(c => c["Jwt:ExpiresInMinutes"]).Returns("60");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("AutoMarket");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("AutoMarketUsers");

        _service = new AuthService(_userRepoMock.Object, _mapperMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmailAndUsername_ReturnsAuthResponseWithToken()
    {
        var dto  = new RegisterRequestDto { Email = "new@test.com", Username = "newuser", Password = "Pass@123" };
        var user = new User { Id = 1, Email = dto.Email, Username = dto.Username, Role = UserRole.Buyer };

        _userRepoMock.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username)).ReturnsAsync((User?)null);
        _mapperMock.Setup(m => m.Map<User>(It.IsAny<RegisterRequestDto>())).Returns(user);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(user);

        var result = await _service.RegisterAsync(dto);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(dto.Username, result.Username);
        Assert.Equal(dto.Email, result.Email);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsArgumentException()
    {
        var dto = new RegisterRequestDto { Email = "exists@test.com", Username = "user", Password = "Pass@123" };
        _userRepoMock.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ThrowsArgumentException()
    {
        var dto = new RegisterRequestDto { Email = "new@test.com", Username = "taken", Password = "Pass@123" };

        _userRepoMock.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username))
            .ReturnsAsync(new User { Id = 99, Username = dto.Username });

        await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponseWithToken()
    {
        const string password = "Pass@123";
        var user = new User
        {
            Id = 1, Email = "user@test.com", Username = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Buyer
        };
        var dto = new LoginRequestDto { Email = user.Email, Password = password };
        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        var result = await _service.LoginAsync(dto);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsArgumentException()
    {
        var dto = new LoginRequestDto { Email = "ghost@test.com", Password = "any" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsArgumentException()
    {
        var user = new User
        {
            Id = 1, Email = "user@test.com", Username = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword"),
            Role = UserRole.Buyer
        };
        var dto = new LoginRequestDto { Email = user.Email, Password = "wrongPassword" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidId_ReturnsUserDto()
    {
        var user = new User { Id = 1, Username = "u", Email = "u@t.com" };
        var userDto = new UserDto { Id = 1, Username = "u", Email = "u@t.com" };

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        var result = await _service.GetCurrentUserAsync(1);

        Assert.Equal(userDto.Username, result.Username);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetCurrentUserAsync(99));
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(r => r.ExistsAsync(404)).ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteUserAsync(404));
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmailAlreadyTaken_ThrowsArgumentException()
    {
        var existing = new User { Id = 1, Username = "u", Email = "old@test.com" };
        var dto = new UpdateProfileDto { Email = "taken@test.com" };

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _userRepoMock.Setup(r => r.EmailExistsAsync("taken@test.com")).ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateProfileAsync(1, dto));
    }
}
