using System.Text.Json;
using PeopleOnDuty.src.Modules.TelegramBot;
using StackExchange.Redis;

namespace src.Modules.TelegramBot.Infrastructure
{
    public class RedisUserStateRepository(IConnectionMultiplexer redis) : IUserStateRepository
    {
        private IDatabase _db => redis.GetDatabase();
        private const string KeyPrefix = "user_state:";
        private static readonly TimeSpan Ttl = TimeSpan.FromHours(5);
        public async Task DeleteAsync(long userId, CancellationToken ct)
        {
            await _db.KeyDeleteAsync($"{KeyPrefix}{userId}");
        }

        public async Task<UserState> GetOrCreateAsync(long userId, CancellationToken ct)
        {
            var key = $"{KeyPrefix}{userId}";
            var cached = await _db.StringGetAsync(key);
            if (cached.IsNullOrEmpty)
            {

                return new UserState { TelegramUserId = userId };
            }
            try
            {
                return JsonSerializer.Deserialize<UserState>(cached.ToString())
                       ?? new UserState { TelegramUserId = userId };
            }
            catch (JsonException)
            {
                await _db.KeyDeleteAsync(key);
                return new UserState { TelegramUserId = userId };
            }
        }

        public async Task SaveAsync(UserState state, CancellationToken ct)
        {
            var key = $"{KeyPrefix}{state.TelegramUserId}";
            var json = JsonSerializer.Serialize(state);
            await _db.StringSetAsync(key, json, Ttl);
        }

    }
}