using System;
using FluentAssertions;
using Moq;
using PeopleOnDuty.src.Modules.Shared;
using PeopleOnDuty.src.Modules.Tasks;
using Xunit;

namespace PeopleOnDuty.src.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repo = new();
    private readonly Mock<IUserService> _users = new();
    private readonly TaskService _sut;

    public TaskServiceTests() => _sut = new TaskService(_repo.Object, _users.Object);

    [Fact]
    public async Task TakeTaskAsync_WhenUserIsAuthor_Throws()
    {
        var authorId = Guid.NewGuid();
        var task = CreateOpenTask(authorId);
        _repo.Setup(r => r.GetAsync(task.TaskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var act = () => _sut.TakeTaskAsync(task.TaskId, authorId, default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Author can not take his own task");
    }

    [Fact]
    public async Task TakeTaskAsync_WhenAlreadyTaken_Throws()
    {
        var authorId = Guid.NewGuid();
        var takerId = Guid.NewGuid();
        var task = CreateOpenTask(authorId);
        _repo.Setup(r => r.GetAsync(task.TaskId, default)).ReturnsAsync(task);
        _repo.Setup(r => r.TryTakeAsync(task.TaskId, takerId, default)).ReturnsAsync(false);

        var act = () => _sut.TakeTaskAsync(task.TaskId, takerId, default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Task already taken by someone else");
    }

    [Fact]
    public async Task TakeTaskAsync_WhenOk_TakesTask()
    {
        var authorId = Guid.NewGuid();
        var takerId = Guid.NewGuid();
        var task = CreateOpenTask(authorId);
        _repo.Setup(r => r.GetAsync(task.TaskId, default)).ReturnsAsync(task);
        _repo.Setup(r => r.TryTakeAsync(task.TaskId, takerId, default)).ReturnsAsync(true);

        await _sut.TakeTaskAsync(task.TaskId, takerId, default);

        task.Status.Should().Be(Modules.Tasks.TaskStatus.TAKEN);
        task.TakenBy.Should().Be(takerId);
    }

    [Fact]
    public async Task AbandonTaskAsync_WhenNotTaker_Throws()
    {
        var task = CreateTakenTask(Guid.NewGuid(), Guid.NewGuid());
        _repo.Setup(r => r.GetAsync(task.TaskId, default)).ReturnsAsync(task);

        var act = () => _sut.AbandonTaskAsync(task.TaskId, Guid.NewGuid(), default);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CompleteTaskAsync_WhenNotTaken_Throws()
    {
        var task = CreateOpenTask(Guid.NewGuid());
        _repo.Setup(r => r.GetAsync(task.TaskId, default)).ReturnsAsync(task);

        var act = () => _sut.CompleteTaskAsync(task.TaskId, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static DutyTask CreateOpenTask(Guid authorId, bool isPublic = false) =>
    new DutyTask("desc", "name", "reward", null, authorId, isPublic);

    private static DutyTask CreateTakenTask(Guid authorId, Guid takerId, bool isPublic = false)
{
    var task = CreateOpenTask(authorId, isPublic);
    task.Take(takerId);
    return task;
}
}
