using System.Collections.Concurrent;

namespace src.Modules.TelegramBot
{
    // Для локов под конкретного пользователя (Разные юзеры - разные потоки. Один юзер - последовательная обработка апдейтов)
    public class UserLockProvider
    {
        private readonly ConcurrentDictionary<long, SemaphoreSlim> _locks = new();
        public SemaphoreSlim GetOrAdd(long userId) =>
            _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
    }
}