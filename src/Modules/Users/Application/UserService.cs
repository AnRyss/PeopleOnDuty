using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PeopleOnDuty.src.Modules.Shared;
using src.Modules.Shared;
using src.Modules.Users.Application;

namespace PeopleOnDuty.src.Modules.Users
{
    public class UserService(IUserRepository userRepository, TokenService tokenService, ILogger<UserService> logger) : IUserService
    {
        private static readonly PasswordHasher<User> Hasher = new();
        public async Task<User?> GetUserAsync(Guid guid, CancellationToken ct)
        {
            return await userRepository.GetUserAsync(guid, ct);
        }
        public async Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
        {
            return await userRepository.GetUsersAsync(guids, ct);
        }
        public async Task<User> GetOrCreateByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            var user = await userRepository.GetByTelegramIdAsync(telegramId, ct);
            if (user is not null) return user;


            var newUser = new User(telegramId);
            await userRepository.CreateNew(newUser, ct);
            return newUser;
        }
        public async Task<int> GetAllUsersCountAsync(CancellationToken ct)
        {
            return await userRepository.GetCountAsync(ct);
        }
        public async Task<IReadOnlyList<long>> GetTelegramIdsBatchAsync(int skip, int take, CancellationToken ct)
        {
            return await userRepository.GetTelegramIdsBatchAsync(skip, take, ct);
        }

        public async Task<AuthResponse> Register(RegisterRequest registerRequest, CancellationToken ct)
        {   // Чтобы кто-то не украл айди потеницального тг юзера
            if (registerRequest.Name.All(Char.IsDigit))
                throw new InvalidDataException("Имя не может состоять только из чисел");

            var hash = Hasher.HashPassword(null!, registerRequest.Password);
            var user = await userRepository.CreateNew(registerRequest.Name, hash, ct);

            return new AuthResponse(tokenService.Generate(user), user.UserId);



        }

        public async Task<AuthResponse> Login(LoginRequest loginRequest, CancellationToken ct)
        {
            var user = await userRepository.GetByName(loginRequest.Name, ct)
             ?? throw new UnauthorizedAccessException("Неверное имя пользователя или пароль");
            if (user.PasswordHash is null)
                throw new UnauthorizedAccessException("Неверное имя пользователя или пароль");
            var verdict = Hasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);
            if (verdict is PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Неверное имя пользователя или пароль");
            }
            else
            {

                return new AuthResponse(tokenService.Generate(user), user.UserId);

            }
        }
        public async Task AttachTelegramIdAsync(Guid userId, long telegramId, CancellationToken ct)
        {
            var user = await userRepository.GetUserAsync(userId, ct)
                ?? throw new KeyNotFoundException("Пользователь не найден.");
            var existingTgUser = await userRepository.GetByTelegramIdAsync(telegramId, ct);
            if (existingTgUser != null)
            {
                if (existingTgUser.UserId == userId)
                    return; 
                throw new InvalidOperationException("Этот Telegram уже привязан к другому аккаунту. Обратитесь в поддержку.");
            }
                try
                {
                    user.TelegramId = telegramId;
                    await userRepository.SaveAsync(user, ct);
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError("Ошибка при связке аккаунтов {exc}", ex.Message);
                }
            }

        }
    }