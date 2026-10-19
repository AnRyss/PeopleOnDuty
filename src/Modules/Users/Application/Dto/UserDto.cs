namespace PeopleOnDuty.src.Modules.Users
{
    public record struct UserDto
    {
        public string Name;
        public UserDto(string name)
        {
            Name = name;
        }
    }
}