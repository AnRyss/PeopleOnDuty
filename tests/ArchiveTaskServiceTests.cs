using FluentAssertions;
using Moq;
using PeopleOnDuty.src.Modules.Archive;

using PeopleOnDuty.src.Modules.Archive.Infrastructure;
using PeopleOnDuty.src.Modules.ArchiveTask;
using PeopleOnDuty.src.Modules.Shared;
using Xunit;

namespace PeopleOnDuty.Tests.Services;

public class ArchiveTaskServiceTests
{
    private readonly Mock<IArchiveTaskRepository> _repo = new();
    private readonly ArchiveTaskService _sut;

    public ArchiveTaskServiceTests() => _sut = new ArchiveTaskService(_repo.Object);

    [Fact]
    public async Task SaveAsync_WhenAlreadyExists_Throws()
    {
        var originalId = Guid.NewGuid();
        _repo.Setup(r => r.GetByOriginalTaskIdAsync(originalId, default))
             .ReturnsAsync(new ArchivedTask(originalId, "n", "d", "r", Guid.NewGuid(), "a"));

        var act = () => _sut.SaveAsync(new ArchivedTaskDto
        {
            OriginalTaskId = originalId,
            TaskName = "n",
            TaskDescription = "d",
            Reward = "r",
            AuthorId = Guid.NewGuid(),
            Answer = "a",
            CompletedAt = DateTime.UtcNow
        }, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]

    public async Task SaveAsync_WhenOk_Saves()
    {
        var originalId = Guid.NewGuid();
        _repo.Setup(r => r.GetByOriginalTaskIdAsync(originalId, default))
             .ReturnsAsync((ArchivedTask?)null);

        await _sut.SaveAsync(new ArchivedTaskDto
        {
            OriginalTaskId = originalId,
            TaskName = "name",
            TaskDescription = "desc",
            Reward = "reward",
            AuthorId = Guid.NewGuid(),
            Answer = "answer",
            CompletedAt = DateTime.UtcNow
        }, default);

        _repo.Verify(r => r.AddAsync(
            It.Is<ArchivedTask>(t =>
                t.OriginalTaskId == originalId &&
                t.Name == "name" &&
                t.Answer == "answer"),
            default), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_FiltersByNameAndPages()
    {
        _repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<ArchivedTask>
        {
            new(Guid.NewGuid(), "Купить хлеб", null, "r", Guid.NewGuid(), "a"),
            new(Guid.NewGuid(), "Купить молоко", null, "r", Guid.NewGuid(), "a"),
            new(Guid.NewGuid(), "Помыть пол", null, "r", Guid.NewGuid(), "a")
        });

        var (items, total) = await _sut.SearchAsync("Купить", 1, 5, default);

        total.Should().Be(2);
        items.Should().OnlyContain(i => i.TaskName.Contains("Купить"));
    }
}