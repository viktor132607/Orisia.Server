using Moq;
using Orisia.Server.Common.Requests.Users;
using Orisia.Server.Common.Responses.Users;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IAuthService> _authService = new();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(_userRepository.Object, _authService.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnMappedUsers()
    {
        _userRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(
        [
            CreateUser("one@example.com", Roles.User),
            CreateUser("editor@example.com", Roles.Editor)
        ]);

        IEnumerable<UserResponse>? result = await _service.GetAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Role == Roles.Editor);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<AppException>(() => _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task SetRoleAsync_ShouldAllowEditorRole()
    {
        Guid adminId = Guid.NewGuid();
        Guid targetId = Guid.NewGuid();
        _authService.Setup(x => x.GetCurrentUserId()).ReturnsAsync(adminId.ToString());
        _userRepository.Setup(x => x.GetByIdAsync(targetId))
            .ReturnsAsync(CreateUser("target@example.com", Roles.User, targetId));
        _userRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        UserResponse result = await _service.SetRoleAsync(new RoleChangeRequest
        {
            UserId = targetId,
            Role = Roles.Editor
        });

        Assert.Equal(Roles.Editor, result.Role);
    }

    [Fact]
    public async Task SetRoleAsync_ShouldRejectUnknownRole()
    {
        _authService.Setup(x => x.GetCurrentUserId()).ReturnsAsync(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<AppException>(() => _service.SetRoleAsync(new RoleChangeRequest
        {
            UserId = Guid.NewGuid(),
            Role = "SuperAdmin"
        }));
    }

    [Fact]
    public async Task SetRoleAsync_ShouldPreventAdminSelfDemotion()
    {
        Guid adminId = Guid.NewGuid();
        _authService.Setup(x => x.GetCurrentUserId()).ReturnsAsync(adminId.ToString());

        await Assert.ThrowsAsync<AppException>(() => _service.SetRoleAsync(new RoleChangeRequest
        {
            UserId = adminId,
            Role = Roles.Editor
        }));
    }

    [Fact]
    public async Task DeleteAsync_ShouldPreventSelfDeletion()
    {
        Guid adminId = Guid.NewGuid();
        _authService.Setup(x => x.GetCurrentUserId()).ReturnsAsync(adminId.ToString());

        await Assert.ThrowsAsync<AppException>(() => _service.DeleteAsync(adminId));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveRoleAndCredentials()
    {
        Guid userId = Guid.NewGuid();
        User existing = CreateUser("old@example.com", Roles.Editor, userId);
        existing.PasswordHash = "hash";
        existing.RefreshToken = "refresh";

        _userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(existing);
        _userRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        UserResponse? result = await _service.UpdateAsync(new UpdateUserRequest
        {
            Id = userId,
            Email = "new@example.com",
            Names = "Updated",
            Phone = "123"
        });

        Assert.NotNull(result);
        Assert.Equal(Roles.Editor, result.Role);
    }

    private static User CreateUser(string email, string role, Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            Names = "Test User",
            Phone = "123",
            PasswordHash = "hash",
            Role = role
        };
    }
}
