using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using PeopleOnDuty.src.Modules.Shared;
using src.Modules.Shared;
using src.Modules.Users.Application;

namespace PeopleOnDuty.src.Modules.Users
{
    public class UserService(IUserRepository userRepository, TokenService tokenService) : IUserService
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

        public async Task<string> Register(RegisterRequest registerRequest, CancellationToken ct)
        {   // Чтобы кто-то не украл айди потеницального тг юзера
            if (registerRequest.Name.All(Char.IsDigit))
                throw new InvalidDataException("Имя не может состоять только из чисел");

            var hash = Hasher.HashPassword(null!, registerRequest.Password);
            var user = await userRepository.CreateNew(registerRequest.Name, hash, ct);

            return tokenService.Generate(user);



        }



    }
}