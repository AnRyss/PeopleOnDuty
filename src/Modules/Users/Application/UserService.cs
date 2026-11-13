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
        private readonly IUserRepository _userRepository = userRepository;
        public async Task<User?> GetUserAsync(Guid guid, CancellationToken ct)
        {
            return await _userRepository.GetUserAsync(guid, ct);
        }
        public async Task<IReadOnlyCollection<User>> GetUsersAsync(IEnumerable<Guid> guids, CancellationToken ct = default)
        {
            return await _userRepository.GetUsersAsync(guids, ct);
        }
        public async Task<User> GetOrCreateByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId, ct);
            if (user is not null) return user;

            // Создаем нового
            var newUser = new User(telegramId);
            await _userRepository.AddAsync(newUser, ct);
            return newUser;
        }
        public async Task<int> GetAllUsersCountAsync(CancellationToken ct)
        {
            return await _userRepository.GetCountAsync(ct);
        }
        public async Task<IReadOnlyList<long>> GetTelegramIdsBatchAsync(int skip, int take, CancellationToken ct)
        {
            return await _userRepository.GetTelegramIdsBatchAsync(skip, take, ct);
        }

        public async Task<string> Register(RegisterRequest registerRequest, CancellationToken ct)
        {

            var hash = Hasher.HashPassword(null!, registerRequest.Password);
            var user = await _userRepository.CreateNew(registerRequest.Name, hash, ct);

            return tokenService.Generate(user);



        }



    }
}