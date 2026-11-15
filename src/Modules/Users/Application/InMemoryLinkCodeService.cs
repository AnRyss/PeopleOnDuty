using System.Collections.Concurrent;
using System.Security.Cryptography;
using PeopleOnDuty.src.Modules.TelegramBot;
namespace PeopleOnDuty.src.Modules.Users.Application;
public class InMemoryLinkCodeService : ILinkCodeService
{
    private static readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAt)> _codes = new();
    private const int ExpireMinutes = 10;
    public string Generate(Guid userId)
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var chars = new char[6];
        for (int i = 0; i < 6; i++)
        {
            int index = RandomNumberGenerator.GetInt32(alphabet.Length);
            chars[i] = alphabet[index];
        }

        var code = new string(chars);
        _codes[code] = (userId, DateTime.UtcNow.AddMinutes(ExpireMinutes));
        ;
        return new string(chars);
    }
    public Guid? Consume(string code)
    {

        if (_codes.TryRemove(code, out var data) && data.ExpiresAt > DateTime.UtcNow)
        {
            return data.UserId;
        }
        return null;
    }
}
