using FluentAssertions;
using Moq;
using PeopleOnDuty.src.Modules.Users;
using Xunit;

namespace PeopleOnDuty.src.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly UserService _sut;

    public UserServiceTests() => _sut = new UserService(_repo.Object);

    [Fact]
    public async Task GetOrCreateByTelegramIdAsync_WhenExists_ReturnsUser()
    {
        var existing = new User(123456);
        _repo.Setup(r => r.GetByTelegramIdAsync(123456, default)).ReturnsAsync(existing);

        var result = await _sut.GetOrCreateByTelegramIdAsync(123456, default);

        result.Should().Be(existing);
        _repo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateByTelegramIdAsync_WhenNotExists_CreatesUser()
    {
        _repo.Setup(r => r.GetByTelegramIdAsync(123456, default)).ReturnsAsync((User?)null);

        var result = await _sut.GetOrCreateByTelegramIdAsync(123456, default);

        result.TelegramId.Should().Be(123456);
        _repo.Verify(r => r.AddAsync(It.Is<User>(u => u.TelegramId == 123456), default), Times.Once);
    }
}