using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using PeopleOnDuty.src.Modules.Users;
using src.Modules.Users.Application;
using Xunit;

namespace PeopleOnDuty.src.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
     private readonly Mock<IConfiguration> _configMock = new Mock<IConfiguration>();
    private  Mock<TokenService> _tokens => new Mock<TokenService>(MockBehavior.Default,_configMock.Object);
    private readonly UserService _sut;

    public UserServiceTests() => _sut = new UserService(_repo.Object, _tokens.Object);

    [Fact]
    public async Task GetOrCreateByTelegramIdAsync_WhenExists_ReturnsUser()
    {
        var existing = new User(123456);
        _repo.Setup(r => r.GetByTelegramIdAsync(123456, default)).ReturnsAsync(existing);

        var result = await _sut.GetOrCreateByTelegramIdAsync(123456, default);

        result.Should().Be(existing);
        _repo.Verify(r => r.CreateNew(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateByTelegramIdAsync_WhenNotExists_CreatesUser()
    {
        _repo.Setup(r => r.GetByTelegramIdAsync(123456, default)).ReturnsAsync((User?)null);

        var result = await _sut.GetOrCreateByTelegramIdAsync(123456, default);

        result.TelegramId.Should().Be(123456);
        _repo.Verify(r => r.CreateNew(It.Is<User>(u => u.TelegramId == 123456), default), Times.Once);
    }
}