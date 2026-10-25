using System.Collections.Concurrent;

namespace PeopleOnDuty.src.Modules.TelegramBot
{
    public class InMemoryUserStateRepository : IUserStateRepository
    {
          private readonly ConcurrentDictionary<long, UserState> _states = new();

    public Task<UserState> GetOrCreateAsync(long userId, CancellationToken ct)
    {
        var state = _states.GetOrAdd(userId, id => new UserState 
        { 
            TelegramUserId = id, 
         //   CurrentState = Scenarios.StateType.MainMenu
        });
        return Task.FromResult(state);
    }

    public Task SaveAsync(UserState state, CancellationToken ct)
    {
        _states[state.TelegramUserId] = state;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(long userId, CancellationToken ct)
    {
        _states.TryRemove(userId, out _);
        return Task.CompletedTask;
    }
    }
}