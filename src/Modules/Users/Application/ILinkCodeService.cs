namespace PeopleOnDuty.src.Modules.Users.Application;


public interface ILinkCodeService
{
    string Generate(Guid userId);
    Guid? Consume(string code);
}
